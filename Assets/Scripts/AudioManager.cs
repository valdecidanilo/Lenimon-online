using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private float MusicVolume = 1;
    private float SfxVolume = 1;
    
    //audio data
    [SerializeField] private AudioClip[] hitAudios;
    [SerializeField] private AudioClip[] selectAudios;
    [SerializeField] private AudioClip buffAudio;
    [SerializeField] private AudioClip debuffAudio;

    public AudioClip battleMusic;
    public AudioClip winSfx;
    public AudioClip gameMusic;
    public AudioClip lobyMusic;
    
    public static AudioManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            Instance.musicSource.volume = MusicVolume;
            Instance.sfxSource.volume = SfxVolume;
        }
        else if(Instance != this)
            Destroy(gameObject);
    }

    public void PlayHitAudio()
    {
        if (ReferenceEquals(Instance, null)) return;
        int id = Random.Range(0, hitAudios.Length-1);
        Instance.sfxSource.PlayOneShot(hitAudios[id]);
    }

    public void PlayBattle()
    {
        if (ReferenceEquals(Instance, null)) return;
        musicSource.clip = battleMusic;
        musicSource.Play();
    }
    public void PlayLobby()
    {
        if (ReferenceEquals(Instance, null)) return;
        musicSource.clip = lobyMusic;
        musicSource.Play();
    }

    public void PlayWin()
    {
        musicSource.Stop();
        musicSource.PlayOneShot(winSfx);
    }
    public void PlayGame()
    {
        if (ReferenceEquals(Instance, null)) return;
        musicSource.clip = gameMusic;
        musicSource.Play();
    }
    public void PlaySelectAudio()
    {
        if (ReferenceEquals(Instance, null)) return;
        int id = Random.Range(0, selectAudios.Length-1);
        sfxSource.clip = selectAudios[id];
        sfxSource.Play();
    }

    public void PlayStatusChangeAudio(bool buff)
    {
        if (ReferenceEquals(Instance, null)) return;
        sfxSource.PlayOneShot(buff ? buffAudio : debuffAudio);
    }
}
