using UnityEngine;
using UnityEngine.EventSystems;

public class HoverPopupVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static System.Action<HoverPopupVisual, bool> OnHovered = delegate { };

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHovered?.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHovered?.Invoke(this, false);
    }
}
