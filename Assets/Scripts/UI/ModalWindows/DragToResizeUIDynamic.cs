using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

// Same as DragToResizeUI, but doesn't snap when starting stretched outside of min-max width and height
public class DragToResizeUIDynamic : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RectTransform _transformToResize;
    [SerializeField] private bool _rightSide = true;
    [SerializeField] private bool _bottom = true;
    [SerializeField] private Vector2 _widthMinMax = new Vector2(200, 600);
    [SerializeField] private Vector2 _heightMinMax = new Vector2(200, 600);

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _resizing;
    [SerializeField, ReadOnly] private Vector2 _startPos;
    [SerializeField, ReadOnly] private Vector2 _startMousePos;
    [SerializeField, ReadOnly] private Vector2 _startSize;
    [SerializeField, ReadOnly] private Vector2 _sizeOffset;
    [SerializeField, ReadOnly] private Vector2 _widthMinMaxDyn;
    [SerializeField, ReadOnly] private Vector2 _heightMinMaxDyn;

    private void OnValidate()
    {
        if (_transformToResize == null) _transformToResize = transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_resizing) return;

        _sizeOffset = ((Vector2)Input.mousePosition - _startMousePos);

        _sizeOffset.x = Mathf.Clamp(_startSize.x + (_rightSide ? _sizeOffset.x : -_sizeOffset.x) * 2f, _widthMinMaxDyn.x, _widthMinMaxDyn.y);
        if (_sizeOffset.x > _widthMinMaxDyn.x) _widthMinMaxDyn.x = Mathf.Min(_widthMinMax.x, _sizeOffset.x);
        if (_sizeOffset.x < _widthMinMaxDyn.y) _widthMinMaxDyn.y = Mathf.Max(_widthMinMax.y, _sizeOffset.x);
        _sizeOffset.x -= _startSize.x;

        _sizeOffset.y = Mathf.Clamp(_startSize.y + (_rightSide ? _sizeOffset.y : -_sizeOffset.y) * 2f, _heightMinMaxDyn.x, _heightMinMaxDyn.y);
        if (_sizeOffset.y > _heightMinMaxDyn.x) _heightMinMaxDyn.x = Mathf.Min(_heightMinMax.x, _sizeOffset.y);
        if (_sizeOffset.y < _heightMinMaxDyn.y) _heightMinMaxDyn.y = Mathf.Max(_heightMinMax.y, _sizeOffset.y);
        _sizeOffset.y -= _startSize.y;

        _transformToResize.sizeDelta = _startSize + _sizeOffset;

        if (!_rightSide) _sizeOffset.x = -_sizeOffset.x;
        if (_bottom) _sizeOffset.y = -_sizeOffset.y;

        _transformToResize.anchoredPosition = _startPos + _sizeOffset * 0.25f;

        if (Input.GetMouseButtonUp(0))
        {
            _resizing = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _resizing = true;
            _startSize.x = _transformToResize.rect.width;
            _widthMinMaxDyn.x = Mathf.Min(_startSize.x, _widthMinMax.x);
            _widthMinMaxDyn.y = Mathf.Max(_startSize.x, _widthMinMax.y);
            _startSize.y = _transformToResize.rect.height;
            _heightMinMaxDyn.x = Mathf.Min(_startSize.y, _heightMinMax.x);
            _heightMinMaxDyn.y = Mathf.Max(_startSize.y, _heightMinMax.y);
            _startPos = _transformToResize.anchoredPosition;
            _startMousePos = Input.mousePosition;
            _transformToResize.SetAsLastSibling();
        }
    }
}