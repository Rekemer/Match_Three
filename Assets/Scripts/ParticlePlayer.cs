using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;
    public float lifeTime = 1;
    public bool destroyImmediatelly;

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        if (destroyImmediatelly) Destroy(gameObject, lifeTime);
    }

    public void Play()
    {
        foreach (ParticleSystem particle in allParticles)
        {
            particle.Stop();
            particle.Play();
        }

        Destroy(gameObject, lifeTime);
    }
}