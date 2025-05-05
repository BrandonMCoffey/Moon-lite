using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerController : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private float _startVolume = 0.8f;
    [SerializeField] private string _masterVolume = "MasterVolume";
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private string _musicVolume = "MusicVolume";
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private string _ambientVolume = "AmbientVolume";
    [SerializeField] private Slider _ambientSlider;
    [SerializeField] private string _sfxVolume = "SfxVolume";
    [SerializeField] private Slider _sfxSlider;

    private JsonPlayerPrefs _saveData;
    private float _saveTimer = 0;

    private void Awake()
    {
        _saveData = new JsonPlayerPrefs("AudioSettings");
    }

    private void Start()
    {
        float volume = _saveData.GetFloat(_masterVolume, _masterSlider != null ? _startVolume : 1f);
        SetMasterVolume(volume);
        if (_masterSlider) _masterSlider.value = volume;

        volume = _saveData.GetFloat(_musicVolume, _startVolume);
        SetMusicVolume(volume);
        if (_musicSlider) _musicSlider.value = volume;

        volume = _saveData.GetFloat(_ambientVolume, _startVolume);
        SetAmbientVolume(volume);
        if (_ambientSlider) _ambientSlider.value = volume;

        volume = _saveData.GetFloat(_sfxVolume, _startVolume);
        SetSfxVolume(volume);
        if (_sfxSlider) _sfxSlider.value = volume;
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

    public void SetMasterVolume(float volume) => SetVolume(_masterVolume, volume);
    public void SetMusicVolume(float volume) => SetVolume(_musicVolume, volume);
    public void SetAmbientVolume(float volume) => SetVolume(_ambientVolume, volume);
    public void SetSfxVolume(float volume) => SetVolume(_sfxVolume, volume);
    public void SetVolume(string id, float volume)
    {
        if (_mixer != null) _mixer.SetFloat(id, Convert(volume));
        _saveData.SetFloat(id, volume);
        _saveTimer = 0.5f; // Prevent saving every frame when dragging slider
    }

    private static float Convert(float volume) => volume == 0 ? -80 : Mathf.Log10(volume) * 20;
}