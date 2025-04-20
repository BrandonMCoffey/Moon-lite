using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipMenuManager : MonoBehaviour
{
    [SerializeField] private Canvas _hud;
    [SerializeField] private ShipMenu _pauseMenu;
    [SerializeField] private ShipMenu _shopMenu;
    [SerializeField] private ShipMenu _researchMenu;
    [SerializeField] private ShipMenu _manageGunsMenu;

    private ShipMenu _currentMenu;
    public static int ActionsThisFrame;

    private void Start()
    {
        _hud.enabled = false;
        _pauseMenu.CloseMenu();
        _shopMenu.CloseMenu();
        _researchMenu.CloseMenu();
        _manageGunsMenu.CloseMenu();
    }

    private void Update()
    {
        if (PlayerInput.Cancel)
        {
            if (_currentMenu != null && _currentMenu.CurrentlyOpen)
            {
                CloseOpenMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    public void ShowHud(bool show)
    {
        _hud.enabled = show;
    }

    public void OpenPauseMenu() => OpenMenu(0);
    public void OpenShopMenu() => OpenMenu(1);
    public void OpenResearchMenu() => OpenMenu(2);
    public void OpenManageGunsMenu() => OpenMenu(3);
    private void OpenMenu(int menuIndex)
    {
        if (_currentMenu != null && _currentMenu.CurrentlyOpen)
        {
            return;
        }
        switch (menuIndex)
        {
            case 0:
                _currentMenu = _pauseMenu;
                _pauseMenu.OpenMenu();
                break;
            case 1:
                _currentMenu = _shopMenu;
                _shopMenu.OpenMenu();
                break;
            case 2:
                _currentMenu = _researchMenu;
                _researchMenu.OpenMenu();
                break;
            case 3:
                _currentMenu = _manageGunsMenu;
                _manageGunsMenu.OpenMenu();
                break;
        }
    }

    public void CloseOpenMenu()
    {
        _currentMenu.CloseMenu();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
