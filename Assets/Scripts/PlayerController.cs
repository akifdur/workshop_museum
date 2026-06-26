using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private PresentationElementsSource presentationElements;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float movementAccelerationTime = 0.1f;
    [SerializeField] private Vector2 mouseSensitivity = new (2, -4f);
    
    [Header("Rendering Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera uiCamera;
    
    private Vector3 _movementVector;
    private Vector3 _smoothDampVelocity;
    
    private int _activeElementIndex;
    private NavMeshPath _path;
    private bool _movementFree;

    private static Vector2 GetMovementInput() => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    private static Vector2 GetLookInput() => new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    
    private void Start()
    {
        _path = new NavMeshPath();
        
        cinemachineBrain.enabled = false;
        for (int i = 0; i < presentationElements.ElementsCount; i++) 
            presentationElements.GetElement(i).VirtualCamera.enabled = false;
        
        playerCamera.enabled = true;
        cinemachineBrain.enabled = true;

        _movementFree = true;
        SetCursorLocked(_movementFree);
    }
    
    public void Update()
    {
        int t = presentationElements.ElementsCount;
        if (Input.GetKeyDown(KeyCode.E))
        {
            var nextPresentationIndex = _activeElementIndex;
            nextPresentationIndex++;
            nextPresentationIndex = (nextPresentationIndex + t) % t;
            InitiateMoveToPresentation(nextPresentationIndex);
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var nextPresentationIndex = _activeElementIndex;
            nextPresentationIndex--;
            nextPresentationIndex = (nextPresentationIndex + t) % t;
            InitiateMoveToPresentation(nextPresentationIndex);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _movementFree = !_movementFree;
            SetCursorLocked(_movementFree);
            if (_movementFree)
                SetMovementFree();
            else
                InitiateMoveToPresentation(_activeElementIndex);

            return;
        }

        if (_movementFree)
        {
            if (GetCameraIsAtViewport(playerCamera)) 
                HandleFreeMovement();
        }
        else
        {
            var presentationElement = presentationElements.GetElement(_activeElementIndex);
            var currentPresentationCamera = presentationElement.VirtualCamera;

            if ((agent.remainingDistance <= agent.stoppingDistance) && 
                GetCameraIsAtViewport(currentPresentationCamera))
            {
                var targetRight = currentPresentationCamera.transform.right;
                var targetForward = Vector3.Cross(targetRight, Vector3.up);
                transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);
                playerCamera.transform.localRotation = Quaternion.identity;

                presentationElement.Focus();
            }
            else if(agent.remainingDistance <= 2)
            {
                currentPresentationCamera.enabled = true;
                playerCamera.enabled = false;
            }
        }
    }

    private void LateUpdate()
    {
        uiCamera.projectionMatrix = mainCamera.projectionMatrix;
    }

    private bool GetCameraIsAtViewport(CinemachineCamera testCamera)
    {
        return (CinemachineCamera)cinemachineBrain.ActiveVirtualCamera == testCamera && !cinemachineBrain.IsBlending;
    }

    private static void SetCursorLocked(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }

    private void SetMovementFree()
    {
        playerCamera.transform.DOKill();

        var presentationElement = presentationElements.GetElement(_activeElementIndex);
        presentationElement.Dismiss();
        agent.enabled = false;
        playerCamera.enabled = true;
    }

    private void HandleFreeMovement()
    {
        var currentPosition = transform.position;
        var forward = Vector3.Cross(playerCamera.transform.right, Vector3.up);
        var right = Vector3.Cross(Vector3.up, forward);
            
        var movementInput = GetMovementInput();
        var targetMovementVector = forward * movementInput.y + right * movementInput.x;
        targetMovementVector *= movementSpeed;
        
        _movementVector = Vector3.SmoothDamp(_movementVector, targetMovementVector, ref _smoothDampVelocity, movementAccelerationTime);
            
        var targetPosition = currentPosition + _movementVector * Time.deltaTime;
        if (NavMesh.SamplePosition(targetPosition, out var hit, 1f, NavMesh.AllAreas)) 
            transform.position = hit.position;
        
        var lookInput = GetLookInput() * mouseSensitivity;
        playerCamera.transform.Rotate(Vector3.up, lookInput.x, Space.World);
        playerCamera.transform.Rotate(Vector3.right, lookInput.y, Space.Self);
    }

    private void InitiateMoveToPresentation(int presentationIndex)
    {
        presentationElements.GetElement(_activeElementIndex).Dismiss();
        
        _activeElementIndex = presentationIndex;
        var targetCamera = presentationElements.GetElement(_activeElementIndex).VirtualCamera;
        
        var startPosition = transform.position;
        var targetPosition = targetCamera.transform.position;
        var pathCalculated = NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, _path);
        if (!pathCalculated)
        {
            Debug.LogError("Could not generate path!");
            return;
        }

        agent.enabled = true;
        agent.path = _path;
        
        playerCamera.enabled = true;
        playerCamera.transform.DOKill();
        playerCamera.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.5f).SetEase(Ease.OutSine);
        _movementFree = false;
    }
}