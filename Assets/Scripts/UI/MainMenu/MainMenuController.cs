using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private float _blendMenuTime = 0.25f;
    [SerializeField] private GameObject _firstSelected;
    [SerializeField] private CanvasGroup _splashScreen;
    [SerializeField] private CanvasGroup _mainMenu;
    [SerializeField] private CanvasGroup _gameSelect;
    [SerializeField] private CanvasGroup _options;
    [SerializeField] private CanvasGroup _credits;
    [SerializeField] private CanvasGroup _fadeout;
    [SerializeField] private List<LoadGameButton> _loadGameButtons = new List<LoadGameButton>();

    private CanvasGroup _current;
    private bool _transitioning;

    private void Start()
    {
        _current = _splashScreen;
        UIHelper.SetGroupActive(_splashScreen, true);
        UIHelper.SetGroupActive(_mainMenu, false);
        UIHelper.SetGroupActive(_gameSelect, false);
        UIHelper.SetGroupActive(_options, false);
        UIHelper.SetGroupActive(_credits, false);
        _splashScreen.gameObject.SetActive(true);
        _mainMenu.gameObject.SetActive(true);
        _gameSelect.gameObject.SetActive(true);
        _options.gameObject.SetActive(true);
        _credits.gameObject.SetActive(true);
        MouseControllerManager.UpdateSelected(_firstSelected);

        for (int i = 0; i < _loadGameButtons.Count; i++)
        {
            _loadGameButtons[i].LinkGameSave(GameManager.GetSaveFile(i + 1));
        }
        LoadGameButton.OnPlayGame += StartGame;
    }

    public void SwitchToSplashScreen() => SwitchToMenu(_splashScreen);
    public void SwitchToMainMenu() => SwitchToMenu(_mainMenu);
    public void SwitchToGameSelect() => SwitchToMenu(_gameSelect);
    public void SwitchToOptions() => SwitchToMenu(_options);
    public void SwitchToCredits() => SwitchToMenu(_credits);
    private void SwitchToMenu(CanvasGroup menu)
    {
        if (_current == menu) return;
        StartCoroutine(SwitchMenu(menu));
    }

    public void SetFirstSelected(GameObject obj)
    {
        _firstSelected = obj;
        if (!_transitioning) MouseControllerManager.UpdateSelected(obj);
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        _gameSelect.interactable = false;
        _gameSelect.blocksRaycasts = false;
        MouseControllerManager.UpdateSelected(null);
        SoundManager.StopAllMusic();
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            _fadeout.alpha = t;
            yield return null;
        }
        GameManager.LoadGameScene();
    }

    public void QuitToDesktop()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator SwitchMenu(CanvasGroup next)
    {
        _transitioning = true;
        MouseControllerManager.UpdateSelected(null);
        _current.interactable = false;
        _current.blocksRaycasts = false;

        float mult = 1f / _blendMenuTime;
        for (float t = 0; t < 1f; t += Time.deltaTime * mult)
        {
            _current.alpha = 1 - t;
            yield return null;
        }
        _current.alpha = 0;
        _current = next;
        for (float t = 0; t < 1f; t += Time.deltaTime * mult)
        {
            next.alpha = t;
            yield return null;
        }
        next.alpha = 1;

        next.interactable = true;
        next.blocksRaycasts = true;
        MouseControllerManager.UpdateSelected(_firstSelected);
        _transitioning = false;
    }
}
