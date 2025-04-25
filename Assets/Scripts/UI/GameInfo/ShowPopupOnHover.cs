using UnityEngine;
using UnityEngine.EventSystems;

public class ShowPopupOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static System.Action<ShowPopupOnHover, bool> OnHovered = delegate { };

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHovered?.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHovered?.Invoke(this, false);
    }

    public virtual string GetHoverText()
    {
        return "Hover Placeholder\nText";
    }
}
