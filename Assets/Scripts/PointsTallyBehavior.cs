using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class PointsTallyBehavior : MonoBehaviour {

    enum countingType
    {
        NONE,
        OBJECTS,
        SCORE
    };
    countingType whatsCountingNow;
    Sequence pointsTallySequence;

    [Range(0.1f, 2.0f)]
    public float durb4_GameOverAppears;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayObjectsBrokenTitle;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayCountObjectsBroken;
    [Range(0.1f, 2.0f)]
    public float durof_CountObjectsBroken;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayMultiplier;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayTotalScoreTitle;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayCountTotalScore;
    [Range(0.1f, 2.0f)]
    public float durof_CountTotalScore;
    [Range(0.1f, 2.0f)]
    public float durb4_DisplayClickToContinue;

    public Text GameOverTitle;
    public Text ObjectsBrokenTitle;
    public Text TotalScoreTitle;
    public Text multiplier;
    public Text ObjectsBrokenCount;
    public Text TotalScoreCount;
    public Text ClickToContinue;

    private int objectsBroken, totalScore;
    private float objectsBrokenLerper, objectsBrokenLerpDuration, totalScoreLerper, totalScoreLerpDuration;
    private bool clickToContinueDisplayed;
    private const int LMB = 0;
    private const int RMB = 1;

    public void Start()
    {
        totalScore = 0;
        objectsBroken = 0;
        

        clickToContinueDisplayed = false;
        Disassemble();
        whatsCountingNow = countingType.NONE;
    }

    public void Update()
    {
        switch(whatsCountingNow)
        {
            case countingType.NONE:
                CheckIfPlayerContinues();
                return;

            case countingType.OBJECTS:
                LerpTheObjectsCounter();
                return;

            case countingType.SCORE:
                LerpTheScoreCounter();
                return;
        }


    }

    private void CheckIfPlayerContinues()
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
        this.totalScore = totalScore;
        totalScoreLerper = 0f;
        totalScoreLerpDuration = 0f;
        TotalScoreCount.text = totalScore.ToString();
        this.objectsBroken = objectsBroken;
        objectsBrokenLerper = 0f;
        objectsBrokenLerpDuration = 0f;
        ObjectsBrokenCount.text = objectsBroken.ToString();

        print(totalScore + " " + objectsBroken);

        pointsTallySequence = DOTween.Sequence();

        pointsTallySequence.AppendInterval(durb4_GameOverAppears)
                           .AppendCallback(DisplayGameOver)
                           .AppendInterval(durb4_DisplayObjectsBrokenTitle)
                           .AppendCallback(DisplayObjectsBrokenTitle)
                           .AppendInterval(durb4_DisplayCountObjectsBroken)
                           .AppendCallback(CountObjectsBroken)
                           .AppendInterval(durb4_DisplayMultiplier + durof_CountObjectsBroken)
                           .AppendCallback(DisplayMultiplier)
                           .AppendInterval(durb4_DisplayTotalScoreTitle)
                           .AppendCallback(DisplayTotalScoreTitle)
                           .AppendInterval(durb4_DisplayCountTotalScore)
                           .AppendCallback(CountTotalScore)
                           .AppendInterval(durb4_DisplayClickToContinue + durof_CountTotalScore)
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

    private void CountObjectsBroken()
    {
        ObjectsBrokenCount.gameObject.SetActive(true);
    }

    private void LerpTheObjectsCounter()
    {
        objectsBrokenLerpDuration += Time.deltaTime;
        if (objectsBrokenLerpDuration > durof_CountObjectsBroken)
        {
            objectsBrokenLerpDuration = durof_CountObjectsBroken;
            whatsCountingNow = countingType.NONE;
        }
        print(objectsBroken);
        float timeToLerp = objectsBrokenLerpDuration / durof_CountObjectsBroken;
        int currentCount = (int)Mathf.Lerp(0, objectsBroken, timeToLerp);
        print(currentCount);
        ObjectsBrokenCount.text = currentCount.ToString();
    }

    private void DisplayMultiplier()
    {
        multiplier.gameObject.SetActive(true);
    }

    private void DisplayTotalScoreTitle()
    {
        TotalScoreTitle.gameObject.SetActive(true);
    }

    private void LerpTheScoreCounter()
    {
        totalScoreLerpDuration += Time.deltaTime;
        if(totalScoreLerpDuration > durof_CountTotalScore)
        {
            totalScoreLerpDuration = durof_CountObjectsBroken;
            whatsCountingNow = countingType.NONE;
        }
        float timeToLerp = totalScoreLerpDuration / durof_CountTotalScore;
        int currentScore = (int)Mathf.Lerp(0, totalScore, timeToLerp);
        print(currentScore);
        TotalScoreCount.text = currentScore.ToString();
    }

    private void CountTotalScore()
    {
        TotalScoreCount.gameObject.SetActive(true);
        whatsCountingNow = countingType.SCORE;
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
        ObjectsBrokenCount.text = "0";
        ObjectsBrokenCount.gameObject.SetActive(false);
        TotalScoreCount.text = "0";
        TotalScoreCount.gameObject.SetActive(false);
        ClickToContinue.gameObject.SetActive(false);
    }
}
