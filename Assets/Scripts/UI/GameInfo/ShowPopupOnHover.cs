using UnityEngine;
using UnityEngine.EventSystems;

public class ShowPopupOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
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
