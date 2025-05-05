using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class GameCanvasManager : MonoBehaviour
{
    [SerializeField] private Canvas _pauseMenu;
    [SerializeField] private CanvasGroup _pauseMenuPanel;
    [SerializeField] private CanvasGroup _optionsMenuPanel;
    [SerializeField] private GameObject _pauseMenuFirstSelected;

    [SerializeField, ReadOnly] private bool _paused;
    [SerializeField, ReadOnly] private bool _pausedOptions;

    private void Awake()
    {
        _paused = false;
        _pauseMenu.enabled = false;
        UIHelper.SetGroupActive(_optionsMenuPanel, false);
        UIHelper.SetGroupActive(_pauseMenuPanel, true);
    }

    private void Update()
    {
        if (PlayerInput.Cancel)
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu() => SetPaused(!_paused);
    public void OpenOptionsMenu(bool open = true)
    {
        if (_paused)
        {
            _pausedOptions = open;
            UIHelper.SetGroupActive(_pauseMenuPanel, !open);
            UIHelper.SetGroupActive(_optionsMenuPanel, open);
        }
    }

    private void SetPaused(bool pause)
    {
        if (_pausedOptions)
        {
            _pausedOptions = false;
            UIHelper.SetGroupActive(_optionsMenuPanel, false);
            UIHelper.SetGroupActive(_pauseMenuPanel, true);
        }
        else
        {
            _paused = pause;
            _pauseMenu.enabled = _paused;
            MouseControllerManager.InGame = !_paused;
            MouseControllerManager.UpdateSelected(_paused ? _pauseMenuFirstSelected : null);
            MouseControllerManager.ActionsThisFrame++;
        }
        Time.timeScale = _paused ? 0 : 1;
    }

    public void SetFirstSelected(GameObject obj) => MouseControllerManager.UpdateSelected(obj);

    public void QuitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
