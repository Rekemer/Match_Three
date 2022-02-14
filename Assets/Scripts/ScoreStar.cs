using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour
{
    public AudioClip sound;
    public Image star;
    public ParticlePlayer starFX;
    public bool activated = false;
    public float delay = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        SetActive(false);
    }

    IEnumerator Test()
    {
        yield return new WaitForSeconds(3f);
        Activate();
    }
    public void SetActive(bool state)
    {
        if (star != null)
        {
            star.gameObject.SetActive(state);
        }
    }

    public void Activate()
    {
        if (activated) return;
        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        activated = true;
        yield return new WaitForSeconds(delay);
        if (starFX != null)
        {
            starFX.Play();
        }

        if (SoundManager.Instance != null && sound != null)
        {
            SoundManager.Instance.PlayClipAtPoint(sound, Vector3.zero, SoundManager.Instance.fxVolume);
        }
        SetActive(true);
    }
}
