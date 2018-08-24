using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBounceSound : MonoBehaviour {

    public AudioClip bounceAgainstTable;
    public AudioClip bounceAgainstPlate;
    public AudioClip bounceAgainstWalls;

    private AudioSource source;
    private float lowPitchRange = .75f;
    private float highPitchRange = 1.5f;
    private float velocityToVolume = 0.2f;

    private string breakable = "Breakable";
    private string unbreakable = "Unbreakable";
    private string wall = "Wall";

	void Awake () {
        source = GetComponent<AudioSource>();
	}

    private void OnCollisionEnter(Collision col)
    {
        source.pitch = Random.Range(lowPitchRange, highPitchRange);
        float hitVol = col.relativeVelocity.magnitude * velocityToVolume;
        if (col.gameObject.CompareTag(breakable)) //is it breakable
        {
            source.PlayOneShot(bounceAgainstPlate, hitVol);
        }
        else if (col.gameObject.CompareTag(unbreakable))
        {
            source.PlayOneShot(bounceAgainstTable, hitVol);
        }
        else if (col.gameObject.CompareTag(wall))
        {
            source.PlayOneShot(bounceAgainstWalls, hitVol);
        }
    }
}
