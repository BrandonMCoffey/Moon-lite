using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class ShipMenu : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _firstSelected;
    [SerializeField] private bool _closeReturnsToFirstGroup = false;
    [SerializeField] private List<CanvasGroup> _groups;
    [SerializeField, ReadOnly] private int _currentGroup;

    public bool CurrentlyOpen => _canvas.enabled;

    private void OnValidate()
    {
        if (_canvas == null) _canvas = GetComponent<Canvas>();
        _groups = GetComponentsInChildren<CanvasGroup>(true).ToList();
    }

    private void Awake()
    {
        _groups = GetComponentsInChildren<CanvasGroup>(true).ToList();
        for (int i = 0; i < _groups.Count; i++)
        {
            UIHelper.SetGroupActive(_groups[i], i == 0);
            _groups[i].gameObject.SetActive(true);
        }
    }

    public void OpenMenu()
    {
        _canvas.enabled = true;
        MouseControllerManager.InGame = false;
        SetFirstSelected(_firstSelected);
        MouseControllerManager.ActionsThisFrame++;
        SoundManager.MusicModifier.SetMuffled(true);
        Time.timeScale = 0;
        OpenGroup(0);
    }

    public void OpenGroup(int index)
    {
        index = Mathf.Clamp(index, 0, _groups.Count - 1);
        _currentGroup = index;
        for (int i = 0; i < _groups.Count; i++)
        {
            UIHelper.SetGroupActive(_groups[i], i == index);
        }
    }

    public void CloseMenu()
    {
        if (_closeReturnsToFirstGroup && _currentGroup != 0)
        {
            OpenGroup(0);
            return;
        }
        _canvas.enabled = false;
        MouseControllerManager.InGame = true;
        SetFirstSelected(null);
        MouseControllerManager.ActionsThisFrame++;
        SoundManager.MusicModifier.SetMuffled(false);
        Time.timeScale = 1;
    }

    public void SetFirstSelected(GameObject obj) => MouseControllerManager.UpdateSelected(obj);
}
