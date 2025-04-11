using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField] private MusicTrack _track;
    [SerializeField] private bool _playOnStart = true;
        
    public void Start()
    {
        if (_playOnStart) Play();
    }

    private void Play()
    {
        _track.Play();
    }
}