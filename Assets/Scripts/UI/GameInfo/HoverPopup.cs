using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPopup : MonoBehaviour
{
    [SerializeField] private Canvas _popupCanvas;

    private HoverPopupVisual _current;

    private void OnEnable()
    {
        HoverPopupVisual.OnHovered += HandleHover;
    }

    private void OnDisable()
    {
        HoverPopupVisual.OnHovered -= HandleHover;
    }

    private void HandleHover(HoverPopupVisual visual, bool hovered)
    {
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

    private void SetupVisual(HoverPopupVisual visual)
    {
        _current = visual;

        // TODO: Load data into visual here

        _popupCanvas.enabled = true;
    }
}
