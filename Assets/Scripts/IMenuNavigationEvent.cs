using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IMenuNavigationEvent : IEventSystemHandler {

    void ReturnToMainMenu();
    void UploadGameHUD();
    void PausedGame();
    void TallyTotalPoints();
    void UploadHighScores();
}
