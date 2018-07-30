using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStartup : MonoBehaviour {

    public Canvas StartMenu;
    public Canvas GameHUD;
    public Canvas PauseMenu;
    public Canvas PointsTally;
    public Canvas HighScores;

    void Awake()
    {
        StartMenu.gameObject.SetActive(false);
        GameHUD.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(false);
        PointsTally.gameObject.SetActive(false);
        HighScores.gameObject.SetActive(false);
    }

    
}
