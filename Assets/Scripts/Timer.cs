using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timeLeft;
    public Image clock;
     int _maxTime = 60 ;
    public bool isPaused;
    public float flashTimeLimit = 10f;
    public AudioClip beepSound;
    public Color flashColor = Color.red;
    public float flashInterval = 1f;
    private IEnumerator routine;
    public void Init( int maxTime = 60)
    {
        _maxTime = maxTime;
        if (timeLeft != null)
        {
            timeLeft.text = _maxTime.ToString();
        }

        if (clock != null)
        {
            clock.type = Image.Type.Filled;
            clock.fillOrigin = (int)Image.Origin360.Top;
            clock.fillMethod = Image.FillMethod.Radial360;
        }
    }

    public void UpdateTimer(int time)
    {
        if (isPaused) return;
        if (clock != null)
        {
            clock.fillAmount = (float) time / (float) _maxTime;
        }

        if (time < flashTimeLimit)
        {
            routine = FlashRoutine(clock, flashColor, flashInterval);
            StartCoroutine(routine);
            if (SoundManager.Instance != null && beepSound != null)
            {
                SoundManager.Instance.PlayClipAtPoint(beepSound, Vector3.zero, SoundManager.Instance.fxVolume, false);
            }
        }

        if (timeLeft != null)
        {
            timeLeft.text = time.ToString();
        }
    }

    IEnumerator FlashRoutine(Image image, Color targetColour, float interval)
    {
        if (image != null)
        {
            Color originalColor = image.color;
            image.CrossFadeColor(targetColour,interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);
            image.CrossFadeColor(originalColor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);
        }
        yield return null;
    }
    public void FadeOff()
    {
        StopCoroutine(routine);
        ScreenFader[] screenFaders = GetComponentsInChildren<ScreenFader>();
        foreach (ScreenFader screenFader in screenFaders)
        {
            screenFader.FadeOff();
        }
    }
}
