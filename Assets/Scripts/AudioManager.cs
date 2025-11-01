using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    void Awake()
    {
        if (instance == null && instance != this)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        
    }
    
    [SerializeField] private AudioClipData[] audioDatas;
    List<AudioSource> activeAudios = new List<AudioSource>();

    void Start()
    {
        ScenesManager.instance.BeforeSceneLoaded += DestroyLevelMusic;
    }

    public void DestroyLevelMusic()
    {
        for (int i = activeAudios.Count - 1; i >= 0; i--)
        {
            StopAudio(activeAudios[i].name);
        }
    }
    
    private AudioClipData GetClipData(string name)
    {
        foreach (AudioClipData data in audioDatas)
        {
            if (data.name == name)
                return data;
        }
        return null;
    }

    public void PlayAudio(string name)
    {
        PlaySource(name);
    }

    public void PlayAudioRandom(string name)
    {
        PlaySource(name, true);
    }

    public void PlayAudioIncreasing(string name, float duration)
    {
        StartCoroutine(IncreaseVolumeOverTime(PlaySource(name), duration));
    }

    IEnumerator IncreaseVolumeOverTime(AudioSource source, float duration)
    {
        float targetVolume = source.volume;
        source.volume = 0;
        float t = 0;

        while (t < duration)
        {
            if (source == null) yield break;
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0, targetVolume, t/duration);
            yield return null;
        }
        
        if (source == null) yield break;
        source.volume = targetVolume;
    }

    public void ReplaceAudio(string currAudio, string newAudio)
    {
        AudioSource sourceToStop = GetSource(currAudio);
        float time = 0;
        if (sourceToStop != null)
        {
            time = sourceToStop.time;
            activeAudios.Remove(sourceToStop);
            Destroy(sourceToStop.gameObject);
        }
        
        AudioSource newSource = PlaySource(newAudio);
        newSource.time = time;
    }
    
    private AudioSource PlaySource(string name, bool randomize = false)
    {
        AudioClipData data = GetClipData(name);
        if (data == null) 
            return null;

        AudioClip clip = data.clips[Random.Range(0, data.clips.Length)];

        GameObject obj = new GameObject(name + " clip");
        AudioSource source = obj.AddComponent<AudioSource>();
        DontDestroyOnLoad(obj);
        
        source.pitch = randomize? Random.Range(0.8f, 1.2f) : 1;
        source.clip = clip;
        source.volume = randomize? Random.Range(data.volume - 0.55f, data.volume + 0.25f): data.volume;
        source.loop = data.loop;
        
        if (source.loop || data.name.StartsWith("game") || data.name.EndsWith("BGM"))
            activeAudios.Add(source);
        else
            Destroy(obj, clip.length);
        
        source.Play();
        
        return source;
    }

    public AudioSource GetSource(string name)
    {
        foreach (AudioSource source in activeAudios)
        {
            if (source == null) return null;
            if (source.name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                return source;
        }
        
        return null;
    }

    public void StopAudio(string name)
    {
        AudioSource source = GetSource(name);
        if (source == null) 
            return;
        activeAudios.Remove(source);
        Destroy(source.gameObject);
    }
    
}
