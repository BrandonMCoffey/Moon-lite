using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicModifier : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private string _lowpassCutoff = "MusicLowpassCutoff";
    [SerializeField] private float _lowpassNormal = 10000;
    [SerializeField] private float _lowpassMuffled = 500;

    private void Awake()
    {
        SetMuffled(false);
    }

    public void SetMuffled(bool muffle)
    {
        if (_mixer != null) _mixer.SetFloat(_lowpassCutoff, muffle ? _lowpassMuffled : _lowpassNormal);
    }
}
