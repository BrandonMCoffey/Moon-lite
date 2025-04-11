using UnityEngine;

public class ShipMenu : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _firstSelected;

    public bool CurrentlyOpen => _canvas.enabled;

    private void OnValidate()
    {
        if (_canvas == null) _canvas = GetComponent<Canvas>();
    }

    public void OpenMenu()
    {
        _canvas.enabled = true;
        MouseControllerManager.InGame = false;
        MouseControllerManager.UpdateSelected(_firstSelected);
        MouseControllerManager.ActionsThisFrame++;
        SoundManager.MusicModifier.SetMuffled(true);
        Time.timeScale = 0;
    }

    public void CloseMenu()
    {
        _canvas.enabled = false;
        MouseControllerManager.InGame = true;
        MouseControllerManager.UpdateSelected(null);
        MouseControllerManager.ActionsThisFrame++;
        SoundManager.MusicModifier.SetMuffled(false);
        Time.timeScale = 1;
    }
}
