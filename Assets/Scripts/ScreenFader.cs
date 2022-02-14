using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaskableGraphic))]
public class ScreenFader : MonoBehaviour
{
    public float solidAlpha = 1;
    public float clearAlpha = 0;
    public float delay = 0f;
    public float timeToFade = 1f;
    private MaskableGraphic m_graphic;
    void Start()
    {
        m_graphic = GetComponent<MaskableGraphic>();
       // FadeOff();
    }

    IEnumerator FadeRoutine(float alpha)
    {
        yield return new WaitForSeconds(delay);
        m_graphic.CrossFadeAlpha(alpha,timeToFade,true); // true means that we will still fade even when game is paused
    }

    public void FadeOn()
    {
        StartCoroutine(FadeRoutine(solidAlpha));
    }
    public void FadeOff()
    {
        StartCoroutine(FadeRoutine(clearAlpha));
    }
}
