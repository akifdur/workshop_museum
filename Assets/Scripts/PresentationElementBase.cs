using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class PresentationElementBase : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    
    public CinemachineCamera VirtualCamera => virtualCamera;

    public event System.Action OnPresentationFocus;
    public event System.Action OnPresentationDismiss;

    public void Focus()
    {
        OnPresentationFocus?.Invoke();
    }
    
    public void Dismiss()
    {
        virtualCamera.enabled = false;
        OnPresentationDismiss?.Invoke();
    }

    
#if UNITY_EDITOR
    private Vector3 _sourcePosition;
    private Vector3 _targetPosition;
    private bool _targetFound;
    private void UpdateTargetPosition()
    {
        var sourcePosition = virtualCamera.transform.position;
        if (_sourcePosition == sourcePosition)
            return;

        _sourcePosition = sourcePosition;
        _targetFound = NavMesh.SamplePosition(_sourcePosition, out var hit, 3F, NavMesh.AllAreas);
        _targetPosition = hit.position;
    }

    private void OnDrawGizmos()
    {
        UpdateTargetPosition();

        if (_targetFound)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_sourcePosition, _targetPosition);   
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_sourcePosition, 0.1F);
        }
    }
#endif
}
