using TMPro;
using UnityEngine;

public class HoverPopup : MonoBehaviour
{
    [SerializeField] private Canvas _popupCanvas;
    [SerializeField] private TMP_Text _popupText;

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
            _popupText.text = visual.GetHoverText();
            _popupCanvas.enabled = true;
        }
        else if (_current == visual)
        {
            _popupCanvas.enabled = false;
            _current = null;
        }
    }
}
