using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [Header("AudioSource")]
    public AudioSource gamePlayAudio;
    [Header("SoundEffect")]
    public AudioSource correctAudio;
    public AudioSource wrongAudio;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlaySound(AudioSource soundToPlay)
    {
        if (!soundToPlay.isPlaying)
            soundToPlay.Play();
    }

    public void StopSound(AudioSource soundToStop)
    {
        if(soundToStop.isPlaying)
            soundToStop.Stop();
    }

    public void PlaySoundAudioClip(AudioClip clip)
    {
        gamePlayAudio.clip = clip;

        if(!gamePlayAudio.isPlaying)
        {
            gamePlayAudio.Play();
            gamePlayAudio.loop = true;
        }
    }
}
