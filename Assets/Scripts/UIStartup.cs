using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenusControl : MonoBehaviour {

    public Canvas StartMenu;
    public Canvas InGameHUD;
    public Canvas PauseMenu;
    public Canvas PointTally;

	// Use this for initialization
	void Awake () {
        StartMenu.gameObject.SetActive(false);
        InGameHUD.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(false);
        PointTally.gameObject.SetActive(false);
	}

}
