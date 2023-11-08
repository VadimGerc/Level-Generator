using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// For better randomization the chunk is divided into several virtual segments (see project description)
/// This class contains information about each segment
/// </summary>
[Serializable]
public class VirtualSegment
{
    /// <summary>
    /// Center of the virtual segment (real point in the scene)
    /// </summary>
    public Vector3 center;

    /// <summary>
    /// Is the segment available for generating an object in it (not if an object has already been generated in it)?
    /// </summary>
    public bool isBusy;
}

[Serializable]
public class Zone
{
    /// <summary>
    /// Percentage value indicating the area in which objects can be generated: 100% - area of the whole chunk / 50% - half of the chunk / etc.
    /// </summary>
    private float _zoneVerticalOffset;
    private float _zoneHorizontalOffset;

    /// <summary>
    /// The whole chunk will be divided into virtual segments with sides equal to this value
    /// (The larger the value, the more randomly objects will be generated within each segment; but on the other hand, fewer such segments will fit on the chunk)
    /// </summary>
    private float _visualSegmentSize;

    /// <summary>
    /// The distance between virtual segments (actually the distance between aitems in the game)
    /// </summary>
    private float _distanceBetweenItems;

    /// <summary>
    /// All virtual segments generated for the current chunk
    /// </summary>
    public List<VirtualSegment> virtualSegments = new List<VirtualSegment>();

    public Zone(float visualSegmentSize, float distanceBetweenItems, float zoneHorizontalOffset, float zoneVerticalOffset, Bounds bounds)
    {
        _visualSegmentSize = visualSegmentSize;
        _distanceBetweenItems = distanceBetweenItems;

        _zoneHorizontalOffset = zoneHorizontalOffset;
        _zoneVerticalOffset = zoneVerticalOffset;
        
        Initialize(bounds);
    }

    public void Initialize(Bounds bounds)
    {
        var minPoint = Vector3.zero;
        var maxPoint = Vector3.zero;
        
        CalculateMinMaxPoints(bounds, ref minPoint, ref maxPoint);
        GenerateVirtualSegments(minPoint, new Vector3(maxPoint.x, minPoint.y, maxPoint.z));
    }

    /// <summary>
    /// Calculate the minimum and maximum chunk points
    /// </summary>
    private void CalculateMinMaxPoints(Bounds bounds, ref Vector3 minPoint, ref Vector3 maxPoint)
    {
        var size = new Vector3(bounds.size.x * _zoneHorizontalOffset / 100, 0, bounds.size.z * _zoneVerticalOffset / 100);
                
        minPoint = new Vector3(bounds.center.x - size.x / 2 , bounds.center.y, bounds.center.z - size.z / 2);
        maxPoint = new Vector3(bounds.center.x + size.x / 2, bounds.center.y, bounds.center.z + size.z / 2);
    }

    /// <summary>
    /// Calculate how many virtual segments can fit in the plane of one chunk
    /// </summary>
    private int CalculateVirtualSegmentsCount(float minValue, float maxValue)
    {
        var virtualSegmentsCount = 0;
        var leftLenght = maxValue - minValue;
        
        while (leftLenght > _visualSegmentSize / 2f)
        {
            leftLenght -= _visualSegmentSize;
            leftLenght -= _distanceBetweenItems;
            virtualSegmentsCount++;
        }
        
        return virtualSegmentsCount;
    }

    /// <summary>
    /// Divide the chunk into equal squares (virtual segments) to generate obstacles in them later on
    /// </summary>
    private void GenerateVirtualSegments(Vector3 minPoint, Vector3 maxPoint)
    {
        var xAxisSegmentsCount = CalculateVirtualSegmentsCount(minPoint.x, maxPoint.x);
        var zAxisSegmentsCount = CalculateVirtualSegmentsCount(minPoint.z, maxPoint.z);

        virtualSegments.Clear();

        for (var i = 0; i < xAxisSegmentsCount; i++)
        {
            var xPos = minPoint.x + (_visualSegmentSize + _distanceBetweenItems) * i;

            for (var j = 0; j < zAxisSegmentsCount; j++)
            {
                var zPos = minPoint.z + (_visualSegmentSize + _distanceBetweenItems) * j;

                virtualSegments.Add(new VirtualSegment {center = new Vector3(xPos + _visualSegmentSize / 2f, minPoint.y, zPos + _visualSegmentSize / 2f), isBusy = false});
            }
        }
    }
}

public class ChunkObject : MonoBehaviour
{
    /// <summary>
    /// All visual objects on the chunk (floor, walls, trees, obstacles, etc.)
    /// </summary>
    [SerializeField] private GameObject visualPart;

    /// <summary>
    /// Renderer component attached to the floor (required to initialize chunk sizes)
    /// </summary>
    [Space][SerializeField] private Renderer floorRendererComponent;

    /// <summary>
    /// Number of objects to generate on this chunk (no more objects will be generated than virtual segments)
    /// </summary>
    [Space][SerializeField] private int obstaclesCount;

    /// <summary>
    /// The whole chunk will be divided into virtual segments with sides equal to this value
    /// (The larger the value, the more randomly objects will be generated within each segment; but on the other hand, fewer such segments will fit on the chunk)
    /// </summary>
    [SerializeField] private int visualSegmentSize;
    
    /// <summary>
    /// The distance between virtual segments (actually the distance between aitems in the game)
    /// </summary>
    [SerializeField] private float distanceBetweenItems;
    
    /// <summary>
    /// Percentage value indicating the area in which objects can be generated: 100% - area of the whole chunk / 50% - half of the chunk / etc.
    /// </summary>
    [Space] [SerializeField] private float zoneVerticalOffset = 60;
    [SerializeField] private float zoneHorizontalOffset = 60;

    /// <summary>
    /// Area for generating obstacles and other objects on the chunk
    /// </summary>
    private Zone _areaToGenerateObstacles;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!floorRendererComponent)
        {
            Debug.LogError("Не указан Renderer компонент пола!");
            return;
        }
        
        _areaToGenerateObstacles = new Zone(visualSegmentSize, distanceBetweenItems, zoneHorizontalOffset, zoneVerticalOffset, floorRendererComponent.bounds);
        GenerateObjects();
    }

    /// <summary>
    /// Generate obstacles and items on the chunk
    /// </summary>
    /// <param name="itemsToGenerate">Items list to be generated</param>
    private void GenerateObjects()
    {
        if (LevelGenerator.Instance.itemsToGenerate.Count == 0)
        {
            Debug.LogError("Не указано ни одно препятсвие для генерации!", LevelGenerator.Instance);
            return;
        }
        
        for (var i = 0; i < obstaclesCount; i++)
        {
            // Get all available virtual segments on the chunk
            var suitableSegments = new List<VirtualSegment>(_areaToGenerateObstacles.virtualSegments.Where(segment => !segment.isBusy));

            if (suitableSegments.Count <= 0)
                continue;

            // Choose one random segment
            var randomSegment = suitableSegments[Random.Range(0, suitableSegments.Count)];
            _areaToGenerateObstacles.virtualSegments.Find(virtualSegment => virtualSegment.center == randomSegment.center).isBusy = true;

            // Get min/max points of the segment
            var minPointInSegment = new Vector3(randomSegment.center.x - visualSegmentSize / 2f, randomSegment.center.y, randomSegment.center.z - visualSegmentSize / 2f);
            var maxPointInSegment = new Vector3(randomSegment.center.x + visualSegmentSize / 2f, randomSegment.center.y, randomSegment.center.z + visualSegmentSize / 2f);

            // Choose a random item to generate 
            var randomItem = LevelGenerator.Instance.itemsToGenerate[Random.Range(0, LevelGenerator.Instance.itemsToGenerate.Count)];

            if (randomItem == null) continue;

            // Spawn the selected object at a random point within the selected virtual segment
            var instantiatedItem = Instantiate(randomItem, new Vector3(Random.Range(minPointInSegment.x, maxPointInSegment.x), Random.Range(minPointInSegment.y, maxPointInSegment.y), Random.Range(minPointInSegment.z, maxPointInSegment.z)), Quaternion.Euler( 0 , Random.Range(0, 360) , 0));
            instantiatedItem.transform.SetParent(visualPart.transform);
        }
    }

    private void Update()
    {
        if (!visualPart || !CharacterController.Instance) return;

        // If the player moves away from this chunk at a certain distance, disable visualization (for optimization); and vice versa, if the player approaches - enable it.
        visualPart.SetActive(Vector3.Distance(CharacterController.Instance.transform.position, transform.position) < LevelGenerator.Instance.hideChunksDistance);
    }
}
