using UnityEngine;

public class HoverPopup : MonoBehaviour
{
    [SerializeField] private Canvas _popupCanvas;

    private ShowPopupOnHover _current;

    private void Awake()
    {
        _popupCanvas.enabled = false;
    }

    private void OnEnable()
    {
        ShowPopupOnHover.OnHovered += HandleHover;
    }

    private void OnDisable()
    {
        ShowPopupOnHover.OnHovered -= HandleHover;
    }

    private void Update()
    {
        transform.position = PlayerInput.MousePosition;
    }

    private void HandleHover(ShowPopupOnHover visual, bool hovered)
    {
        Debug.Log($"{(hovered ? "Hover " : "Leave ")} from {visual.name}");
        if (hovered)
        {
            // Handle hiding _current visual if needed?

            SetupVisual(visual);
        }
        else if (_current == visual)
        {
            _popupCanvas.enabled = false;
            _current = null;
        }
    }

    private void SetupVisual(ShowPopupOnHover visual)
    {
        _current = visual;

        // TODO: Load data into visual here

        _popupCanvas.enabled = true;
    }
}
