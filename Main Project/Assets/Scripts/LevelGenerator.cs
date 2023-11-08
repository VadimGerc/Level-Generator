using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum DirectionType
{
    X, Y, Z
}

/// <summary>
/// Describes one generation direction (x, z, -x или -z). Stores the edge point and the chunks amount   
/// </summary>
[Serializable]
public class DirectionValues
{
    /// <summary>
    /// Direction type
    /// </summary>
    public DirectionType directionType;
    
    /// <summary>
    /// Edge point of the generated level
    /// </summary>
    public float edgePosition;

    /// <summary>
    /// Number of generated chunks
    /// </summary>
    public int chunksCount;

    public void Initialize(float startChunkPosition, DirectionType directionType)
    {
        this.directionType = directionType;
        edgePosition = startChunkPosition;
    }

    public bool IsGenerationNeeded(float playerPositionValue)
    {
        return Mathf.Abs(playerPositionValue) > Mathf.Abs(edgePosition);
    }
}

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;
    
    /// <summary>
    /// If the character comes within this distance to the level edge, new chunks will be generated
    /// </summary>
    [SerializeField] private float chunksGenerationDistance;
    
    /// <summary>
    /// If the character moves away from the chunk by this distance, it will be hidden (for optimization)
    /// </summary>
    public float hideChunksDistance;
    
    /// <summary>
    /// The initial chunk, on which the character is spawned; also on the basis of which the other chunks are generated.
    /// </summary>
    [Space][SerializeField] private Transform startChunk;
    
    /// <summary>
    /// Prefabs of chunks of different types (e.g. with different surfaces or with various obstacles)
    /// </summary>
    [SerializeField] private List<Transform> chunksPrefabsToGenerate;

    /// <summary>
    /// List of items (trees, barrels, crates, etc.) that can be generated in the game
    /// </summary>
    [Space] public List<GameObject> itemsToGenerate;


    // 4 directions of level generation (in each direction chunks are generated independently of each other)
    private readonly DirectionValues _xDirection = new DirectionValues();
    private readonly DirectionValues _zDirection = new DirectionValues();
    private readonly DirectionValues _negativeXDirection = new DirectionValues();
    private readonly DirectionValues _negativeZDirection = new DirectionValues();

    /// <summary>
    /// Chunk side length - needed to correctly calculate the boundaries of the entire level
    /// </summary>
    private float _startChunkScale;

    /// <summary>
    /// Position of the initial chunk; the whole level is generated based on it
    /// </summary>
    private Vector3 _startChunkPosition;

    /// <summary>
    /// Link to the character in the game
    /// </summary>
    private Transform _player;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!startChunk)
        {
            Debug.LogError("Стартовый чанк не установлен!");
            return;
        }

        if (!CharacterController.Instance)
        {
            Debug.LogError("На сцене нет персонажа.");
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        _player = CharacterController.Instance.transform;
        
        _startChunkPosition = startChunk.position;
        _startChunkScale = startChunk.localScale.x;
        
        _xDirection.Initialize(_startChunkPosition.x, DirectionType.X);
        _zDirection.Initialize(_startChunkPosition.z, DirectionType.Z);
        _negativeXDirection.Initialize(_startChunkPosition.x, DirectionType.X);
        _negativeZDirection.Initialize(_startChunkPosition.z, DirectionType.Z);
    }

    /// <summary>
    /// Generate chunks in one of the directions
    /// </summary>
    /// <param name="direction">Direction of generation</param>
    /// <param name="isNegativeDirection">(if the direction is negative, we should not add distance, but subtract it).</param>
    private void GenerateChunks(DirectionValues direction, bool isNegativeDirection)
    {
        direction.edgePosition += _startChunkScale * (isNegativeDirection ? -1 : 1);
        direction.chunksCount++;

        // Generate base chunk
        var chunkToGenerate = chunksPrefabsToGenerate[Random.Range(0, chunksPrefabsToGenerate.Count)].gameObject;
        Instantiate(chunkToGenerate, new Vector3((direction.directionType == DirectionType.X ? direction.edgePosition : _startChunkPosition.x), _startChunkPosition.y, direction.directionType == DirectionType.Z ? direction.edgePosition : _startChunkPosition.z), Quaternion.identity, startChunk.parent);

        var subDirection = direction.directionType == DirectionType.X ? _zDirection : _xDirection;
        var negativeSubDirection = direction.directionType == DirectionType.X ? _negativeZDirection : _negativeXDirection;
        
        // Generate additional chunks in the same direction
        for (var i = 1; i <= subDirection.chunksCount; i++)
        {
            chunkToGenerate = chunksPrefabsToGenerate[Random.Range(0, chunksPrefabsToGenerate.Count)].gameObject; 
            Instantiate(chunkToGenerate, new Vector3((direction.directionType == DirectionType.Z ? _startChunkPosition.x + _startChunkScale * i : direction.edgePosition), _startChunkPosition.y, (direction.directionType == DirectionType.X ? _startChunkPosition.z + _startChunkScale * i : direction.edgePosition)), Quaternion.identity, startChunk.parent);
        }
        
        for (var i = 1; i <= negativeSubDirection.chunksCount; i++)
        {
            chunkToGenerate = chunksPrefabsToGenerate[Random.Range(0, chunksPrefabsToGenerate.Count)].gameObject; 
            Instantiate(chunkToGenerate, new Vector3((direction.directionType == DirectionType.Z ? _startChunkPosition.x - _startChunkScale * i : direction.edgePosition), _startChunkPosition.y, (direction.directionType == DirectionType.X ? _startChunkPosition.z - _startChunkScale * i : direction.edgePosition)), Quaternion.identity, startChunk.parent);
        }
    }

    void Update()
    {
        // If for some reason the player is below the level, stop generating it
        if (_player.position.y < startChunk.position.y) return;
        
        // If the player approaches the edge of the map with one of the sides, generate new chunks
        if (Mathf.Abs(_player.transform.position.x - _xDirection.edgePosition) < chunksGenerationDistance)
            GenerateChunks(_xDirection, false);
        
        if (Mathf.Abs(_player.transform.position.x - _negativeXDirection.edgePosition) < chunksGenerationDistance)
            GenerateChunks(_negativeXDirection, true);
        
        if (Mathf.Abs(_player.transform.position.z - _zDirection.edgePosition) < chunksGenerationDistance)
            GenerateChunks(_zDirection, false);
        
        if (Mathf.Abs(_player.transform.position.z - _negativeZDirection.edgePosition) < chunksGenerationDistance)
            GenerateChunks(_negativeZDirection, true);
    }
}
