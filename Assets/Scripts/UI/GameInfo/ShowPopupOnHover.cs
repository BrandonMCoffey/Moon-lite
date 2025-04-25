using UnityEngine;
using UnityEngine.EventSystems;

public class ShowPopupOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public static bool IsUIHovered = false;
    public static System.Action<ShowPopupOnHover, bool> OnHovered = delegate { };

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsUIHovered = true;
        OnHovered?.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsUIHovered = false;
        OnHovered?.Invoke(this, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ClickOnVisual();
        }
    }

    public virtual void ClickOnVisual()
    {
    }

    public virtual string GetHoverText()
    {
        return "Hover Placeholder\nText";
    }
}
