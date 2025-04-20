using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class GameCanvasManager : MonoBehaviour
{
    [SerializeField] private Canvas _pauseMenu;
    [SerializeField] private GameObject _pauseMenuFirstSelected;

    [SerializeField, ReadOnly] private bool _paused;

    private void Update()
    {
        if (PlayerInput.Cancel)
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu() => SetPaused(!_paused);

    private void SetPaused(bool pause)
    {
        _paused = pause;
        _pauseMenu.enabled = _paused;
        MouseControllerManager.InGame = !_paused;
        MouseControllerManager.UpdateSelected(_paused ? _pauseMenuFirstSelected : null);
        MouseControllerManager.ActionsThisFrame++;
        Time.timeScale = _paused ? 0 : 1;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
