﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class PointsTallyBehavior : MonoBehaviour {

    Sequence pointsTallySequence;

    [Range(0.1f, 2.0f)]
    public float durb4_GameOverAppears;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayObjectsBrokenTitle;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayCountObjectsBroken;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayMultiplier;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayTotalScoreTitle;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayCountTotalScore;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayClickToContinue;

    public Text GameOverTitle;
    public Text ObjectsBrokenTitle;
    public Text TotalScoreTitle;
    public Text multiplier;
    public Text ObjectsBrokenCount;
    public Text TotalScoreCount;
    public Text ClickToContinue;

    private bool clickToContinueDisplayed;
    private int totalScore;
    private int objectsBroken;
    private const int LMB = 0;
    private const int RMB = 1;

    public void Start()
    {
        totalScore = 0;
        objectsBroken = 0;
        clickToContinueDisplayed = false;
    }

    public void Update()
    {
        if (!clickToContinueDisplayed)
        {
            return;
        }
        if (Input.GetMouseButtonUp(LMB) || Input.GetMouseButtonUp(RMB))
        {

            ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.ReturnToStartMenu());
        }
    }

    public void BeginTallySequence(int objectsBroken, int totalScore)
    {
        clickToContinueDisplayed = false;
        print(objectsBroken + " " + totalScore);
        TotalScoreCount.text = totalScore.ToString();
        ObjectsBrokenCount.text = objectsBroken.ToString();
        Disassemble();

        pointsTallySequence = DOTween.Sequence();

        pointsTallySequence.AppendInterval(durb4_GameOverAppears)
                           .AppendCallback(DisplayGameOver)
                           .AppendInterval(durb4_DisplayObjectsBrokenTitle)
                           .AppendCallback(DisplayObjectsBrokenTitle)
                           .AppendInterval(durb4_DisplayCountObjectsBroken)
                           .AppendCallback(DisplayTotalObjectsBroken)
                           .AppendInterval(durb4_DisplayMultiplier)
                           .AppendCallback(DisplayMultiplier)
                           .AppendInterval(durb4_DisplayTotalScoreTitle)
                           .AppendCallback(DisplayTotalScoreTitle)
                           .AppendInterval(durb4_DisplayCountTotalScore)
                           .AppendCallback(DisplayTotalScore)
                           .AppendInterval(durb4_DisplayClickToContinue)
                           .AppendCallback(DisplayClickToContinue);
    }

    private void DisplayGameOver()
    {
        GameOverTitle.gameObject.SetActive(true);
    }

    //Displays the title first, then counts the number of objects broken
    private void DisplayObjectsBrokenTitle()
    {
        ObjectsBrokenTitle.gameObject.SetActive(true);
    }

    private void DisplayTotalObjectsBroken()
    {
        ObjectsBrokenCount.gameObject.SetActive(true);
    }

    private void DisplayMultiplier()
    {
        multiplier.gameObject.SetActive(true);
    }

    private void DisplayTotalScoreTitle()
    {
        TotalScoreTitle.gameObject.SetActive(true);
    }

    private void DisplayTotalScore()
    {
        TotalScoreCount.gameObject.SetActive(true);
    }

    private void DisplayClickToContinue()
    {
        ClickToContinue.gameObject.SetActive(true);
        clickToContinueDisplayed = true;
    }

    //returns every UI component to an inactive state for the next setup
    public void Disassemble()
    {
        GameOverTitle.gameObject.SetActive(false);
        ObjectsBrokenTitle.gameObject.SetActive(false);
        TotalScoreTitle.gameObject.SetActive(false);
        multiplier.gameObject.SetActive(false);
        ObjectsBrokenCount.gameObject.SetActive(false);
        TotalScoreCount.gameObject.SetActive(false);
        ClickToContinue.gameObject.SetActive(false);
    }
}
