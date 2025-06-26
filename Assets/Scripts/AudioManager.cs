using System.Collections.Generic;
using AddressableAsyncInstances;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const int MusicChannel = 0;
    private const int SfxChannel = 1;
    private const float MusicVolume = 1;
    private const float SfxVolume = 1;
    
    //audio data
    private static AudioClip[] hitAudios;
    private static AudioClip[] selectAudios;
    private static AudioClip buffAudio;
    private static AudioClip debuffAudio;
    
    private static AudioManager instance;
    private List<AudioSource> sources;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Setup()
    {
        instance = new GameObject("AudioManager").AddComponent<AudioManager>();
        
        instance.sources = new(2);
        instance.sources.Add(instance.gameObject.AddComponent<AudioSource>());
        instance.sources.Add(instance.gameObject.AddComponent<AudioSource>());
        instance.sources[MusicChannel].volume = MusicVolume;
        instance.sources[SfxChannel].volume = SfxVolume;
        
        selectAudios = new AudioClip[2];
        for (int i = 0; i < selectAudios.Length; i++)
        {
            int id = i;
            AAAsset<AudioClip>.LoadAsset($"Select{id + 1}",
                clip => { selectAudios[id] = clip; });
        }

        hitAudios = new AudioClip[4];
        for (int i = 0; i < hitAudios.Length; i++)
        {
            int id = i;
            AAAsset<AudioClip>.LoadAsset($"Attack_Hit{id + 1}",
                clip => { hitAudios[id] = clip; });
        }

        AAAsset<AudioClip>.LoadAsset("Buff", clip => buffAudio = clip);
        AAAsset<AudioClip>.LoadAsset("Debuff", clip => debuffAudio = clip);
    }

    public static void PlayHitAudio()
    {
        if (ReferenceEquals(instance, null)) return;
        int id = Random.Range(0, hitAudios.Length);
        instance.sources[SfxChannel].PlayOneShot(hitAudios[id]);
    }
    
    public static void PlaySelectAudio()
    {
        if (ReferenceEquals(instance, null)) return;
        int id = Random.Range(0, selectAudios.Length);
        instance.sources[SfxChannel].clip = selectAudios[id];
        instance.sources[SfxChannel].Play();
    }

    public static void PlayStatusChangeAudio(bool buff)
    {
        if (ReferenceEquals(instance, null)) return;
        instance.sources[SfxChannel].PlayOneShot(buff ? buffAudio : debuffAudio);
    }
}
