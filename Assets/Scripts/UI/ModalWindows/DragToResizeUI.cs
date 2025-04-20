using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragToResizeUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RectTransform _transformToResize;
    [SerializeField] private bool _rightSide = true;
    [SerializeField] private bool _bottom = true;
    [SerializeField] private Vector2 _widthMinMax = new Vector2(200, 600);
    [SerializeField] private Vector2 _heightMinMax = new Vector2(200, 600);
    [SerializeField] private bool _snapSegmented;
    [Tooltip("Number (Not including min and max) of snap points (Spread evenly)")]
    [SerializeField, ShowIf(nameof(_snapSegmented))] private int _widthStepCount = 2;
    [SerializeField, ShowIf(nameof(_snapSegmented))] private int _heightStepCount = 2;

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _resizing;
    [SerializeField, ReadOnly] private Vector2 _startPos;
    [SerializeField, ReadOnly] private Vector2 _startMousePos;
    [SerializeField, ReadOnly] private Vector2 _startSize;
    [SerializeField, ReadOnly] private Vector2 _sizeOffset;

    private void OnValidate()
    {
        if (_transformToResize == null) _transformToResize = transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_resizing) return;

        _sizeOffset.x = Mathf.Clamp(Input.mousePosition.x, 0, Screen.width) - _startMousePos.x;
        _sizeOffset.y = Mathf.Clamp(Input.mousePosition.y, 0, Screen.height) - _startMousePos.y;

        // New width and height
        _sizeOffset.x = Mathf.Clamp(_startSize.x + (_rightSide ? _sizeOffset.x : -_sizeOffset.x) * 2f, _widthMinMax.x, _widthMinMax.y);
        _sizeOffset.y = Mathf.Clamp(_startSize.y + (_bottom ? -_sizeOffset.y : _sizeOffset.y) * 2f, _heightMinMax.x, _heightMinMax.y); 
        if (_snapSegmented)
        {
            float totalSize = (_widthMinMax.y - _widthMinMax.x) / _widthStepCount;
            float steppedValue = Mathf.Round((_sizeOffset.x - _widthMinMax.x) / totalSize);
            _sizeOffset.x = steppedValue * totalSize + _widthMinMax.x;

            totalSize = (_heightMinMax.y - _heightMinMax.x) / _heightStepCount;
            steppedValue = Mathf.Round((_sizeOffset.y - _heightMinMax.x) / totalSize);
            _sizeOffset.y = steppedValue * totalSize + _heightMinMax.x;
        }
        // Reset back to offset values
        _sizeOffset.x -= _startSize.x;
        _sizeOffset.y -= _startSize.y;


        _transformToResize.sizeDelta = _startSize + _sizeOffset;

        if (!_rightSide) _sizeOffset.x = -_sizeOffset.x;
        if (_bottom) _sizeOffset.y = -_sizeOffset.y;

        _transformToResize.anchoredPosition = _startPos + _sizeOffset * 0.5f;

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
            _startSize.x = Mathf.Clamp(_transformToResize.rect.width, _widthMinMax.x, _widthMinMax.y);
            _startSize.y = Mathf.Clamp(_transformToResize.rect.height, _heightMinMax.x, _heightMinMax.y);
            _startPos = _transformToResize.anchoredPosition;
            _startMousePos = Input.mousePosition;
            _transformToResize.SetAsLastSibling();
        }
    }
}