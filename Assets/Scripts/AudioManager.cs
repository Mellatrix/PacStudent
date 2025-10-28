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
        activeAudios.Add(PlaySource(name));
    }
    
    private AudioSource PlaySource(string name)
    {
        AudioClipData data = GetClipData(name);
        if (data == null) 
            return null;

        AudioClip clip = data.clips[Random.Range(0, data.clips.Length)];

        GameObject obj = new GameObject(name + " clip");
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = data.volume;
        source.loop = data.loop;

        source.Play();
        
        if (!data.loop)
            Destroy(obj, clip.length);

        return source;
    }

    private AudioSource GetSource(string name)
    {
        foreach (AudioSource source in activeAudios)
        {
            if (source.clip.name.Contains(name))
                return source;
        }
        
        return null;
    }
    private void Start()
    {
        
    }
}
