﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StageNine;

public class GameFieldManager : MonoBehaviour
{
    //enums

    //subclasses

    //consts and static data

    public static GameFieldManager singleton;

    //public data

    //private data

    [SerializeField] private float cameraMinX, cameraMinY, cameraMaxX, cameraMaxY, linearMaxSpeed;
    [SerializeField] private Canvas gameplayCanvas;

    [SerializeField] private GameObject quizItemPrefab;

    private int _rightAnswers = 0;
    private int _wrongAnswers = 0;
    [SerializeField] private Text rightAnswersCounter;
    [SerializeField] private Text wrongAnswersCounter;

    private float worldUnitsPerPixel;
    private string currentPrompt = null;

    //public properties

    public bool canTapQuizItems
    {
        // implement this
        get { return true; }
    }

    public int rightAnswers
    {
        get { return _rightAnswers; }
        set
        {
            _rightAnswers = value;
            rightAnswersCounter.text = "Right: " + _rightAnswers; 
        }
    }

    public int wrongAnswers
    {
        get { return _wrongAnswers; }
        set
        {
            _wrongAnswers = value;
            wrongAnswersCounter.text = "Wrong: " + _wrongAnswers;
        }
    }

    //methods
    #region public methods
    public void CameraPan(Vector2 pan)
    {
        Camera.main.transform.position += (Vector3)(pan * worldUnitsPerPixel);
        CameraWrap();
        // cameraInterrupt = true;
    }

    public Vector3 ScreenWrapInBounds(Vector2 basePos)
    {
        float outputX, outputY;
        if (basePos.x < cameraMinX)
        {
            outputX = basePos.x + (cameraMaxX - cameraMinX);
        }
        else if (basePos.x > cameraMaxX)
        {
            outputX = basePos.x - (cameraMaxX - cameraMinX);
        }
        else
        {
            outputX = basePos.x;
        }
        // now do it for Y
        if (basePos.y < cameraMinY)
        {
            outputY = basePos.y + (cameraMaxY - cameraMinY);
        }
        else if (basePos.y > cameraMaxY)
        {
            outputY = basePos.y - (cameraMaxY - cameraMinY);
        }
        else
        {
            outputY = basePos.y;
        }
        return new Vector3(outputX, outputY);
    }

    public bool QuizCorrectAnswer(TriviaPair trivia)
    {
        if (currentPrompt != null)
        {
            if (trivia.prompt == currentPrompt)
                return true;
        }
        else
        {
            if (trivia.prompt != null)
                return true;
        }
        return false;
    }

    public void CreateQuizItem(TriviaPair triviaPair)
    {
        QuizItem quizItem = Instantiate(quizItemPrefab).GetComponent<QuizItem>();
        quizItem.transform.SetParent(gameplayCanvas.transform);
        float xx = Random.Range(cameraMinX, cameraMaxX);
        float yy = Random.Range(cameraMinY, cameraMaxY);
        float vx = Random.Range(-linearMaxSpeed, linearMaxSpeed);
        float vy = Random.Range(-linearMaxSpeed, linearMaxSpeed);
        quizItem.Initialize(triviaPair, new Vector2(xx, yy), new Vector2(vx, vy));
    }

    #endregion

    #region private methods
    private void CameraWrap()
    {
        if(Camera.main.transform.position.x > cameraMaxX)
        {
            Camera.main.transform.position += Vector3.left * (cameraMaxX - cameraMinX);
        }
        if (Camera.main.transform.position.x < cameraMinX)
        {
            Camera.main.transform.position += Vector3.right * (cameraMaxX - cameraMinX);
        }
        if (Camera.main.transform.position.y > cameraMaxY)
        {
            Camera.main.transform.position += Vector3.down * (cameraMaxY - cameraMinY);
        }
        if (Camera.main.transform.position.y < cameraMinY)
        {
            Camera.main.transform.position += Vector3.up * (cameraMaxY - cameraMinY);
        }
    }

    private void UpdateWorldUnitsPerPixel()
    {
        worldUnitsPerPixel = (Camera.main.ScreenToWorldPoint(new Vector3(1, 0)) - Camera.main.ScreenToWorldPoint(new Vector3(0, 0))).magnitude;
    }

    private void InitializeFields()
    {
        UpdateWorldUnitsPerPixel();
    }

    #endregion

    #region monobehaviors
    void Awake()
    {
        Debug.Log("[GameFieldManager:Awake]");
        if (singleton == null)
        {
            Debug.Log("Hello, world!");
            singleton = this;
            InitializeFields();
        }
        else
        {
            Debug.Log("Goodbye, cruel world!");
            GameObject.Destroy(gameObject);
        }
    }


    void Start()
    {
        int rightAnswerCount = 15;
        int wrongAnswerCount = 15;

        TriviaParser.singleton.RandomizeAnswerLists();

        int promptNumber = 0;
        int answerNumber = 0;

        for (int ii = 0; ii < rightAnswerCount; ++ii)
        {
            CreateQuizItem(TriviaParser.singleton.rightAnswers[TriviaParser.singleton.prompts[promptNumber]][answerNumber]);
            ++answerNumber;
            if(answerNumber >= TriviaParser.singleton.rightAnswers[TriviaParser.singleton.prompts[promptNumber]].Count)
            {
                answerNumber = 0;
                ++promptNumber;
            }
        }

        for (int ii = 0; ii < wrongAnswerCount; ++ii)
        {
            CreateQuizItem(TriviaParser.singleton.wrongAnswers[ii]);
        }
    }
    #endregion


}
