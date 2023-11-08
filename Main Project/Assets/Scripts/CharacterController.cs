using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public static CharacterController Instance;

    /// <summary>
    /// Character Movement Speed
    /// </summary>
    [SerializeField] private float speed;
    
    /// <summary>
    /// How much inertia affects (lower values - the character is slower to stop)
    /// </summary>
    [SerializeField] private float inertiaForce;
    
    /// <summary>
    /// Character Rotation Speed 
    /// </summary>
    [SerializeField] private float rotationSpeed;

    /// <summary>
    /// A camera linked to the character
    /// </summary>
    [Space] [SerializeField] private Transform mainCamera;

    /// <summary>
    /// Animator component that visualizes the character movement
    /// </summary>
    [Space] [SerializeField] private Animator animator;

    /// <summary>
    /// Target motion vector (calculated from input vector and inertia)
    /// </summary>
    private Vector3 _targetMovementVector;
    
    /// <summary>
    /// Player's input data (what buttons he pressed)
    /// </summary>
    private Vector3 _inputDirection;

    /// <summary>
    /// A reference to the (Unity) [CharacterController] component that moves the player
    /// </summary>
    private UnityEngine.CharacterController _characterController;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       Initialize();
    }

    private void Initialize()
    {
        _characterController = GetComponent<UnityEngine.CharacterController>();

        if (!_characterController)
        {
            Debug.LogError("На персонаже отсутствует компонент CharacterController");
        }
    }

    /// <summary>
    /// Getting input from a player
    /// </summary>
    private void GetInput()
    {
        _inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }
    
    /// <summary>
    /// Character Movement Process
    /// </summary>
    private void MoveProcess()
    {
        // Apply inertia and move the character
        _targetMovementVector = Vector3.MoveTowards(_targetMovementVector, _inputDirection.normalized, inertiaForce * Time.deltaTime);
        _characterController.Move(_targetMovementVector * speed * Time.deltaTime);
        
        // Apply inertia and move the character
        if(animator) animator.SetFloat("Velocity", _characterController.velocity.magnitude);
    }

    /// <summary>
    /// Rotates the character in the direction of movement
    /// </summary>
    private void RotationProcess()
    {
        if(!mainCamera) return;
        
        var eulerAngles = transform.eulerAngles;
        
        // Calculate an angle between the camera and the character's movement direction
        var angleBetweenCharacterAndCamera = CalculationsHelper.AngleBetween(transform.TransformDirection(new Vector3(-_inputDirection.x, _inputDirection.y, _inputDirection.z)), mainCamera.forward);
        
        // Turn the character in the direction of movement (by the value calculated above)
        var targetAngle = Quaternion.Euler(eulerAngles.x, eulerAngles.y + angleBetweenCharacterAndCamera, eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, rotationSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        GetInput();
    }

    private void Update()
    {
        MoveProcess();
        RotationProcess();
    }
}
