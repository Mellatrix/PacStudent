using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioPreviewManager : MonoBehaviour
{
    private AudioSource bgm, sfx;
    [SerializeField] private AudioClipData[] bgmClips, sfxClips;
    int bgmIndex, sfxIndex;
    [SerializeField] private TextMeshProUGUI bgmText, sfxText;

    private void Awake()
    {
        bgm = gameObject.AddComponent<AudioSource>();
        bgm.volume = 0.2f;
        sfx = gameObject.AddComponent<AudioSource>();

        bgmIndex = 0;
        sfxIndex = 0;
    }

    void Start()
    {
        PlayBgm(0);
        PlaySfx(0);
    }

    private Coroutine bgmRoutine;
    public void PlayBgm(int next)
    {
        if (bgmClips == null || bgmClips.Length == 0)
        {
            return;
        }
        bgmIndex = (bgmIndex + next) < 0 ? bgmClips.Length - 1 : (bgmIndex + next) % bgmClips.Length;
        
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);
        bgmRoutine = StartCoroutine(PlayClip(bgm, bgmClips[bgmIndex], bgmText));

        bgm.Play();
    }

    private Coroutine sfxRoutine;
    public void PlaySfx(int next)
    {
        sfxIndex = (sfxIndex + next) < 0 ? sfxClips.Length - 1 : (sfxIndex + next) % sfxClips.Length;
        if (sfxRoutine != null)
            StopCoroutine(sfxRoutine);

        sfxRoutine = StartCoroutine(PlayClip(sfx, sfxClips[sfxIndex], sfxText));
        
        sfx.Play();
    }

    IEnumerator PlayClip(AudioSource source, AudioClipData data, TextMeshProUGUI text)
    {
        text.text = data.name;
        source.loop = data.loop;
        float t = 0f;
        foreach (AudioClip clip in data.clips)
        {
            source.clip = clip;
            source.Play();
            while (t < clip.length)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}

[Serializable]
public class AudioClipData
{
    public string name;
    public AudioClip[] clips;
    public bool loop;
}
