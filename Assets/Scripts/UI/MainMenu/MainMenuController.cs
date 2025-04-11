using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private float _blendMenuTime = 0.25f;
    [SerializeField] private GameObject _firstSelected;
    [SerializeField] private CanvasGroup _splashScreen;
    [SerializeField] private CanvasGroup _mainMenu;
    [SerializeField] private CanvasGroup _options;
    [SerializeField] private CanvasGroup _credits;
    [SerializeField] private CanvasGroup _fadeout;

    private CanvasGroup _current;
    private bool _transitioning;

    private void Start()
    {
        _current = _splashScreen;
        SetGroupActive(_splashScreen, true);
        SetGroupActive(_mainMenu, false);
        SetGroupActive(_options, false);
        SetGroupActive(_credits, false);
        MouseControllerManager.UpdateSelected(_firstSelected);
    }

    private void SetGroupActive(CanvasGroup group, bool active)
    {
        group.alpha = active ? 1 : 0;
        group.interactable = active;
        group.blocksRaycasts = active;
    }

    public void SwitchToSplashScreen() => SwitchToMenu(_splashScreen);
    public void SwitchToMainMenu() => SwitchToMenu(_mainMenu);
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
        _mainMenu.interactable = false;
        _mainMenu.blocksRaycasts = false;
        MouseControllerManager.UpdateSelected(null);
        SoundManager.StopAllMusic();
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            _fadeout.alpha = t;
            yield return null;
        }
        LoadGame();
    }

    private void LoadGame()
    {
        SceneManager.LoadScene(1);
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
