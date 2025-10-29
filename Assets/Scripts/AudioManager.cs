using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    [SerializeField] private AudioClipData[] audioDatas;
    List<AudioSource> activeAudios = new List<AudioSource>();

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
    
    private AudioSource PlaySource(string name, bool randomize = false)
    {
        AudioClipData data = GetClipData(name);
        if (data == null) 
            return null;

        AudioClip clip = data.clips[Random.Range(0, data.clips.Length)];

        GameObject obj = new GameObject(name + " clip");
        AudioSource source = obj.AddComponent<AudioSource>();
        
        source.pitch = randomize? Random.Range(0.8f, 1.2f) : 1;
        source.clip = clip;
        source.volume = randomize? Random.Range(data.volume - 0.55f, data.volume + 0.25f): data.volume;
        source.loop = data.loop;
        
        if (source.loop)
            activeAudios.Add(source);

        source.Play();
        
        if (!data.loop)
            Destroy(obj, clip.length);

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
        Destroy(GetSource(name)?.gameObject);
    }
    
    private void Start()
    {
        
    }
}
