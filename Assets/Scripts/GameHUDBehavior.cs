using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDBehavior : MonoBehaviour, IGameHUDEvent {

    public Text BallSpeed;
    public Text CurrentScore;
    public GameObject UpdateList;
    public GameObject Update;

    private float mpsToMPH = 2.23694f;
    private GameObject[] listOfUpdates;
    private int listOfUpdatesCounter;
    private int updatesListed;

    // Use this for initialization
    void Start () {
        CurrentScore.text = "0";
        BallSpeed.text = "0 mph";
        listOfUpdates = new GameObject[8];
        for(int i = 0; i < listOfUpdates.Length; i++)
        {
            listOfUpdates[i] = GameObject.Instantiate(Update) as GameObject;
            listOfUpdates[i].SetActive(false);
        }
        listOfUpdatesCounter = 0;
        updatesListed = 0;
	}

    //Adds new text obj to the bottom of the scrollview housing new message
    //scrolls the list down one space to make room for new object
    /*public void AddNewMessage(string message)
    {
        print("Entered AddNewMessage. List of updates counter = " + listOfUpdatesCounter);
        //1) put the message into the next text object
        listOfUpdates[listOfUpdatesCounter].GetComponent<Text>().text = message;
        //2) place the message at the bottom of the children
        listOfUpdates[listOfUpdatesCounter].SetActive(true);
        listOfUpdates[listOfUpdatesCounter].transform.SetParent(UpdateList.gameObject.transform, false);
        listOfUpdates[listOfUpdatesCounter].transform.SetAsLastSibling();
        listOfUpdatesCounter = (listOfUpdatesCounter++) % listOfUpdates.Length;
        //3) Remove an entry in the list if we have used the maximum
        updatesListed++;
        if(updatesListed > 7)
        {
            GameObject oldestUpdate = UpdateList.transform.GetChild(0).gameObject;
            oldestUpdate.transform.parent = null;
            oldestUpdate.SetActive(false);
            updatesListed = 7;
        }
    }*/

    public void UpdateScore(int newScore)
    {
        //print("Receiving " + newScore + " and updating score!");
        CurrentScore.text = newScore.ToString();
    }

    public void UpdateSpeed(float metersPerSecond)
    {
        float milesPerHour = metersPerSecond * mpsToMPH;
        BallSpeed.text = string.Format("##.# mph", milesPerHour);
    }
}
