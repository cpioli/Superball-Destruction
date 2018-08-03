using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IGameHUDEvent : IEventSystemHandler {

    //void AddNewMessage(string message);
    void UpdateScore(int newScore);
    void UpdateSpeed(float metersPerSecond);
}
