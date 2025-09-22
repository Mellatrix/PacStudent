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

        Debug.Log("yuhh" + bgmClips.Length);
        bgmIndex = 0;
        sfxIndex = 0;
    }

    void Start()
    {
        PlayBgm(0);
        PlaySfx(0);
    }

    public void PlayBgm(int next)
    {
        if (bgmClips == null || bgmClips.Length == 0)
        {
            Debug.LogWarning("No BGM clips assigned!");
            return;
        }
        bgmIndex = (bgmIndex + next) < 0 ? bgmClips.Length - 1 : (bgmIndex + next) % bgmClips.Length;
        bgm.clip = bgmClips[bgmIndex].clip;
        bgm.loop = bgmClips[bgmIndex].loop;
        bgmText.text = bgmClips[bgmIndex].name;
        bgm.Play();
    }

    public void PlaySfx(int next)
    {
        sfxIndex = (sfxIndex + next) < 0 ? sfxClips.Length - 1 : (sfxIndex + next) % sfxClips.Length;
        sfx.clip = sfxClips[sfxIndex].clip;
        sfx.loop = sfxClips[sfxIndex].loop;
        sfxText.text = sfxClips[sfxIndex].name;
        sfx.Play();
    }
}

[Serializable]
public class AudioClipData
{
    public string name;
    public AudioClip clip;
    public bool loop;
}
