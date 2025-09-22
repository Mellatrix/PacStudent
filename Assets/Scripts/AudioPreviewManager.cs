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
        bgm.volume = 0.4f;
        sfx = gameObject.AddComponent<AudioSource>();

        bgmIndex = 0;
        sfxIndex = 0;
    }

    void Start()
    {
        PlayBgm(0);
        PlaySfx(0);
    }

    Coroutine bgmRoutine;
    public void PlayBgm(int next)
    {
        if (bgmClips == null || bgmClips.Length == 0)
        {
            Debug.LogWarning("No BGM clips assigned!");
            return;
        }
        bgmIndex = (bgmIndex + next) < 0 ? bgmClips.Length - 1 : (bgmIndex + next) % bgmClips.Length;
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);
        bgmRoutine = StartCoroutine(PlayClips(bgm, bgmClips[bgmIndex], bgmText));
        bgm.Play();
    }

    Coroutine sfxRoutine;
    public void PlaySfx(int next)
    {
        sfxIndex = (sfxIndex + next) < 0 ? sfxClips.Length - 1 : (sfxIndex + next) % sfxClips.Length;
        if (sfxRoutine != null)
            StopCoroutine(sfxRoutine);
        sfxRoutine = StartCoroutine(PlayClips(sfx, sfxClips[sfxIndex], sfxText));
        sfx.Play();
    }

    IEnumerator PlayClips(AudioSource source, AudioClipData clipData, TextMeshProUGUI text)
    {
        text.text = clipData.name;
        source.loop = clipData.loop;
        foreach (AudioClip clip in clipData.clips)
        {
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(clip.length);
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
