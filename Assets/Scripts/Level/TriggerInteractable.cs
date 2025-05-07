using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class TriggerInteractable : MonoBehaviour
{
    [SerializeField] private float _triggerRadius = 2;
    [SerializeField] private bool _invokeExitOnGameStart;
    [SerializeField] private UnityEvent _onPlayerEnter;
    [SerializeField] private UnityEvent _onPlayerExit;
    [SerializeField] private UnityEvent _onPlayerInteract;

    [SerializeField, ReadOnly] private bool _insideTrigger;

    private Transform _player;

    private void Start()
    {
        if (_invokeExitOnGameStart)
        {
            _onPlayerExit?.Invoke();
        }
        _player = Player.Instance.transform;
    }

    private void Update()
    {
        if (_player == null)
        {
            // Lazy load if errored
            _player = Player.Instance.transform;
            return;
        }
        float dist = Vector3.Distance(transform.position, _player.position);
        if (!_insideTrigger && dist <= _triggerRadius)
        {
            _insideTrigger = true;
            _onPlayerEnter?.Invoke();
        }
        else if (_insideTrigger && dist > _triggerRadius)
        {
            _insideTrigger = false;
            _onPlayerExit?.Invoke();
        }
        if (_insideTrigger && PlayerInput.InteractOrSubmit)
        {
            _onPlayerInteract?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _triggerRadius);
    }
}
