using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
public class DraggableUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField, ReadOnly] private bool _isDragging;
    [SerializeField, ReadOnly] private Vector3 _mouseOffset;
    [SerializeField, ReadOnly] private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_isDragging) return;

        Vector3 newPos = PlayerInput.MousePosition + _mouseOffset;
        _rect.position = newPos;
        var corners = new Vector3[4];
        _rect.GetWorldCorners(corners);
        if (corners[0].x < 0) newPos.x -= corners[0].x;
        if (corners[0].y < 0) newPos.y -= corners[0].y;
        if (corners[2].x > Screen.width) newPos.x -= corners[2].x - Screen.width;
        if (corners[2].y > Screen.height) newPos.y -= corners[2].y - Screen.height;
        _rect.position = newPos;

        if (PlayerInput.MouseLeftReleased)
        {
            _isDragging = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _isDragging = true;
            _mouseOffset = _rect.position - PlayerInput.MousePosition;
            transform.SetAsLastSibling();
        }
    }
}