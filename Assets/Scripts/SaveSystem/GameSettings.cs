using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private string _saveName = "GameSettings";

    [Header("Graphics Quality")]
    [SerializeField] private TMP_Dropdown _graphicsQualityDropdown;
    [SerializeField] private int _defaultGraphicsQuality = 2;
    [SerializeField] private List<string> _graphicsQualityOptions = new List<string> { "Low", "Medium", "High", "Ultra" };

    [Header("Display Mode")]
    [SerializeField] private TMP_Dropdown _displayModeDropdown;
    [SerializeField] private int _defaultDisplayMode = 1;
    [SerializeField] private List<string> _displayModeOptions = new List<string> { "Fullscreen", "Windowed", "Borderless" };

    [Header("Frame Rate")]
    [SerializeField] private Toggle _vSyncToggle;
    [SerializeField] private bool _defaultVSync = true;
    [SerializeField] private TMP_Dropdown _frameRateCapDropdown;
    [SerializeField] private int _defaultFrameRateCapOption = 2;
    [SerializeField] private List<int> _frameRateCapOptions = new List<int> { 30, 60, 120, 240 };

    [Header("Brightness")]
    [SerializeField] private Slider _brightnessSlider;
    [SerializeField] private float _defaultBrightness = 0.5f;

    private const string GraphicsQuality = "GraphicsQuality"; // High, Medium, Low
    private const string DisplayMode = "DisplayMode"; // Fullscreen, Windowed, Borderless
    private const string VSync = "VSync"; // True/False
    private const string FrameRateCap = "FrameRateCap"; // 30, 60, 120, 240, etc
    private const string Brightness = "Brightness"; // 1 - 100
    // Todo: Antialiasing, Shadows, Texture Quality, etc

    private JsonPlayerPrefs _saveData;
    private float _saveTimer = 0;

    private void OnValidate()
    {
        if (_graphicsQualityDropdown != null)
        {
            _graphicsQualityDropdown.ClearOptions();
            _graphicsQualityDropdown.AddOptions(_graphicsQualityOptions);
        }
        if (_displayModeDropdown != null)
        {
            _displayModeDropdown.ClearOptions();
            _displayModeDropdown.AddOptions(_displayModeOptions);
        }
        if (_frameRateCapDropdown != null)
        {
            _frameRateCapDropdown.ClearOptions();
            _frameRateCapDropdown.AddOptions(_frameRateCapOptions.Select(option => option.ToString()).ToList());
            _frameRateCapDropdown.AddOptions(new List<string> { "Unlimited" });
        }
    }

    private void Awake()
    {
        _saveData = new JsonPlayerPrefs(_saveName);
    }

    private void Start()
    {
        int graphicsQuality = _saveData.GetInt(GraphicsQuality, _defaultGraphicsQuality);
        SetGraphicsQuality(graphicsQuality);
        if (_graphicsQualityDropdown != null)
        {
            _graphicsQualityDropdown.ClearOptions();
            _graphicsQualityDropdown.AddOptions(_graphicsQualityOptions);
            _graphicsQualityDropdown.value = graphicsQuality;
        }

        int displayMode = _saveData.GetInt(DisplayMode, _defaultDisplayMode);
        SetDisplayMode(displayMode);
        if (_displayModeDropdown != null)
        {
            _displayModeDropdown.ClearOptions();
            _displayModeDropdown.AddOptions(_displayModeOptions);
            _displayModeDropdown.value = displayMode;
        }

        bool vSync = _saveData.GetBool(VSync, _defaultVSync);
        SetVSync(vSync);
        if (_vSyncToggle != null) _vSyncToggle.isOn = vSync;

        int frameRateCap = _saveData.GetInt(FrameRateCap, _defaultFrameRateCapOption);
        SetFrameRateCap(frameRateCap);
        if (_frameRateCapDropdown != null)
        {
            _frameRateCapDropdown.ClearOptions();
            _frameRateCapDropdown.AddOptions(_frameRateCapOptions.Select(option => option.ToString()).ToList());
            _frameRateCapDropdown.AddOptions(new List<string> { "Unlimited" });
            _frameRateCapDropdown.value = frameRateCap;
        }

        float brightness = _saveData.GetFloat(Brightness, _defaultBrightness);
        SetBrightness(brightness);
        if (_brightnessSlider) _brightnessSlider.value = brightness;
    }

    private void Update()
    {
        if (_saveTimer > 0)
        {
            _saveTimer -= Time.deltaTime;
            if (_saveTimer <= 0)
            {
                _saveData.Save();
                _saveTimer = 0;
            }
        }
    }

    public void SetGraphicsQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
        _saveData.SetInt(GraphicsQuality, quality);
        _saveTimer = 0.5f;
    }

    public void SetDisplayMode(int displayMode)
    {
        switch (displayMode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
        _saveData.SetInt(DisplayMode, displayMode);
        _saveTimer = 0.5f;
    }

    public void SetVSync(bool vSync)
    {
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        _saveData.SetBool(VSync, vSync);
        _saveTimer = 0.5f;
    }

    public void SetFrameRateCap(int frameRateCap)
    {
        Application.targetFrameRate = frameRateCap == _frameRateCapOptions.Count ? -1 : _frameRateCapOptions[frameRateCap];
        _saveData.SetInt(FrameRateCap, frameRateCap);
        _saveTimer = 0.5f;
    }

    public void SetBrightness(float brightnses)
    {
        _saveData.SetFloat(Brightness, brightnses);
        _saveTimer = 0.5f;
    }
}
