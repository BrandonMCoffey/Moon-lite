using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class TriggerInteractable : MonoBehaviour
{
    [SerializeField] private bool _invokeExitOnGameStart;
    [SerializeField] private UnityEvent _onPlayerEnter;
    [SerializeField] private UnityEvent _onPlayerExit;
    [SerializeField] private UnityEvent _onPlayerInteract;

    [SerializeField, ReadOnly] private bool _insideTrigger;

    private void Start()
    {
        if (_invokeExitOnGameStart)
        {
            _onPlayerExit?.Invoke();
        }
    }

    private void Update()
    {
        if (_insideTrigger && PlayerInput.InteractOrSubmit && MouseControllerManager.ActionsThisFrame == 0)
        {
            _onPlayerInteract?.Invoke();
        }
    }

    // TODO: Fix this to work with new player raycast system
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            _insideTrigger = true;
            _onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            _insideTrigger = false;
            _onPlayerExit?.Invoke();
        }
    }
}
