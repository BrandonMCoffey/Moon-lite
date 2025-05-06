using TMPro;
using UnityEngine;

public class HoverPopup : MonoBehaviour
{
    [SerializeField] private Canvas _popupCanvas;
    [SerializeField] private RectTransform _popupCanvasPivot;
    [SerializeField] private TMP_Text _popupText;

    private ShowPopupOnHover _current;
    private bool _leftSide = true;
    private Vector2 _rectPos;

    private void Awake()
    {
        _popupCanvas.enabled = false;
        _rectPos = _popupCanvasPivot.anchoredPosition;
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
        if (_leftSide && _popupCanvasPivot.position.x > Screen.width * 0.5f)
        {
            _leftSide = false;
            if (_rectPos.x > 0) _rectPos.x *= -1;
            _popupCanvasPivot.anchoredPosition = _rectPos;
        }
        else if (!_leftSide && _popupCanvasPivot.position.x < Screen.width * 0.5f)
        {
            _leftSide = true;
            if (_rectPos.x < 0) _rectPos.x *= -1;
            _popupCanvasPivot.anchoredPosition = _rectPos;
        }
    }

    private void HandleHover(ShowPopupOnHover visual, bool hovered)
    {
        //Debug.Log($"{(hovered ? "Hover " : "Leave ")} from {visual.name}");
        if (hovered)
        {
            _current = visual;
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
