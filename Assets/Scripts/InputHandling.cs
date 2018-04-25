using UnityEngine;
using System.Collections;

public class InputHandling : MonoBehaviour {

    private float currentTimeSpeed;
    
	// Use this for initialization
	void Start () 
	{
        currentTimeSpeed = Time.timeScale;
	}

	// Update is called once per frame
	void Update () 
	{
	    if(Input.GetKeyUp(KeyCode.UpArrow) && Time.timeScale < 100f)
        {
            Time.timeScale += .1f;
            currentTimeSpeed = Time.timeScale;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) && Time.timeScale >= 0f)
        {
            Time.timeScale -= .1f;
            currentTimeSpeed = Time.timeScale;
        }
        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            ResetTimeScale();
        }
    }

    void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
        currentTimeSpeed = Time.timeScale;
    }
}
