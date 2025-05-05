using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIHelper
{
    public static void SetGroupActive(CanvasGroup group, bool active)
    {
        group.alpha = active ? 1 : 0;
        group.interactable = active;
        group.blocksRaycasts = active;
    }
}
