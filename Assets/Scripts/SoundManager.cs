using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : Singletone<SoundManager>
{
    public AudioClip[] musicClips;
    public AudioClip[] winClips;
    public AudioClip[] loseClips;
    public AudioClip[] bonusClips;
    [Range(0, 1)] public float musicVolume;
    [Range(0, 1)] public float fxVolume;
    public float lowPitch = 0.95f;
    public float highPitch = 1.05f;
    public string musicSourceName = "BackgroundMusic";
    private void Start()
    {
        PlayRandomMusic(true);
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f, bool IsPitchRandomized = true, bool selfDestruct = true)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX" + clip.name);
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            if (IsPitchRandomized)
            {
                source.pitch = Random.Range(lowPitch, highPitch);
            }
            source.Play();
            if (selfDestruct)
            {
                Destroy(go, clip.length);
            }
            
            return source;

        }

        return null;
    }

    public AudioSource PlayRandomClipAtPoint(AudioClip[] clip, Vector3 pos, float volume = 1f,bool isPitchRandomized = true, bool selfDestruct = true)
    {
        if (clip != null)
        {
            if (clip.Length != 0)
            {
                int index = Random.Range(0, clip.Length);
                if (clip[index] != null)
                {
                    AudioSource source = PlayClipAtPoint(clip[index], pos, volume,isPitchRandomized,selfDestruct );
                    return source;
                }
            }
        }

        return null;
    }

    public void PlayRandomMusic( bool dontDestroyOnLoad)
    {
        GameObject musicObject = GameObject.Find(musicSourceName);
 
        if (musicObject == null)
        {
            AudioSource source = PlayRandomClipAtPoint(musicClips, Vector3.zero, musicVolume, false, false);
            source.loop = true;
            source.gameObject.name = musicSourceName;
 
            if (dontDestroyOnLoad && source != null)
            {
                DontDestroyOnLoad(source.gameObject);
            }
        }
    } 
    public void PlayWinSound()
    {
        PlayRandomClipAtPoint(winClips, Vector3.zero, fxVolume);
    } 
    public void PlayLoseSound()
    {
        PlayRandomClipAtPoint(loseClips, Vector3.zero, fxVolume);
    } 
    public void PlayBonusSound()
    {
        PlayRandomClipAtPoint(bonusClips, Vector3.zero, fxVolume);
    } 
}
