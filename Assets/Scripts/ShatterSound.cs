using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterSound : MonoBehaviour {

    public AudioClip shatterSound;

    private AudioSource source;
    private float lowPitchRange = .75f;
    private float highPitchRange = 1.5f;
    private float velocityToVolume = 0.2f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision col)
    {
        source.pitch = Random.Range(lowPitchRange, highPitchRange);
        float hitVol = col.relativeVelocity.magnitude * velocityToVolume;
        source.PlayOneShot(shatterSound, hitVol);
    }
}
