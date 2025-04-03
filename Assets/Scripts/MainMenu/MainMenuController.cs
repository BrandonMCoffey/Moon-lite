using CoffeyUtils;
using System.Collections;
using System.Collections.Generic;
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

    private CanvasGroup _current;

    private void Start()
    {
        _current = _splashScreen;
        _splashScreen.interactable = true;
        _splashScreen.alpha = 1;
        _mainMenu.interactable = false;
        _mainMenu.alpha = 0;
        _options.interactable = false;
        _options.alpha = 0;
        _credits.interactable = false;
        _credits.alpha = 0;
        MouseControllerManager.UpdateSelected(_firstSelected);
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
        MouseControllerManager.UpdateSelected(obj);
    }

    public void StartGame()
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
        float mult = 1f / _blendMenuTime;
        _current.interactable = false;
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
        MouseControllerManager.UpdateSelected(_firstSelected);
    }
}
