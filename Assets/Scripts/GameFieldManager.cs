using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StageNine;
using System.Collections.Generic;

public class GameFieldManager : MonoBehaviour
{
    //enums
     enum GameState
    {
        PREGAME,
        PLAY,
        MAIN_MENU,
        POSTSCREEN,

    };
    //subclasses

    //consts and static data

    public static GameFieldManager singleton;
    private const float PREGAME_WAIT_TIME = 1.5f;
    private const float PREGAME_FADE_TIME = 0.5f;
    private const float DIMMER_MAX_ALPHA = 0.7f;
    private const float INITIAL_TIME = 600f;
    private const int TIMER_FONT_SMALL = 40;
    private const int TIMER_FONT_BIGGER = 12;
    private const int TIMER_FONT_BIGGER_CRITICAL = 24;
    private const float TIMER_PULSE_SIZE_EXPONENT = 8.0f;
    private const float TIMER_PULSE_SIZE_EXPONENT_CRITICAL = 0.4f;
    private const float TIMER_PULSE_COLOR_EXPONENT = 0.4f;
    private const float CRITICAL_TIME_THRESHOLD = 10;
    private readonly Color TIMER_COLOR_NORMAL = Color.white;
    private readonly Color TIMER_COLOR_CRITICAL = Color.red;

    //public data
    public List<QuizItem> quizItems;

    //private data
    GameState gameState = GameState.PREGAME;
    [SerializeField] private float cameraMinX, cameraMinY, cameraMaxX, cameraMaxY, cameraHalfHeight, linearMaxSpeed;
    [SerializeField] private Canvas gameplayCanvas;
    [SerializeField] private Canvas interfaceCanvas;
    [SerializeField] private Canvas scoreCanvas;

    [SerializeField] private GameObject quizItemPrefab;

    private int _rightAnswerCount = 0;
    private int _wrongAnswerCount = 0;
    private float _timer = 0;
    private bool _timedMode = true;
    [SerializeField] private Text rightAnswersCounter;
    [SerializeField] private Text wrongAnswersCounter;
    [SerializeField] private Text timerDisplay;
    [SerializeField] private Image screenDimmer;
    [SerializeField] private Text screenCenterText;

    private float worldUnitsPerPixel;
    private string currentPrompt = null;
    private int rightAnswersInPlay = 0;
    private int wrongAnswersInPlay = 0;
    private int initialQuizItemCount = 30;
    private int promptIndex = 0;
    private int rightAnswerIndex = 0;
    private int wrongAnswerIndex = 0;
    private float chanceOfRightAnswer = .5f;

    //public properties

    public bool canTapQuizItems
    {
        // implement this
        get {
            if(gameState == GameState.PLAY)
            {
                return true;
            }
            return false;
             }
    }

    public int rightAnswerCount
    {
        get { return _rightAnswerCount; }
        set
        {
            _rightAnswerCount = value;
            rightAnswersCounter.text = "Right: " + _rightAnswerCount; 
        }
    }

    public int wrongAnswerCount
    {
        get { return _wrongAnswerCount; }
        set
        {
            _wrongAnswerCount = value;
            wrongAnswersCounter.text = "Wrong: " + _wrongAnswerCount;
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
        outputX = basePos.x;
        while (outputX < Camera.main.transform.position.x + cameraMinX)
        {
            outputX += (cameraMaxX - cameraMinX);
        }
        while (outputX > Camera.main.transform.position.x + cameraMaxX)
        {
            outputX -= (cameraMaxX - cameraMinX);
        }
        // now do it for Y
        outputY = basePos.y;
        while (outputY < Camera.main.transform.position.y + cameraMinY)
        {
            outputY += (cameraMaxY - cameraMinY);
        }
        while (outputY > Camera.main.transform.position.y + cameraMaxY)
        {
            outputY -= (cameraMaxY - cameraMinY);
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
        float xx;
        float yy;
        if (gameState == GameState.PLAY)
        {
            xx = Random.Range(cameraMinX, cameraMaxX);
            yy = Random.Range(2 * cameraHalfHeight, cameraMaxY - cameraMinY);
            yy += Camera.main.transform.position.y - cameraHalfHeight;
            // TODO: make this a little less messy, and maybe allow it to be within the currently illegal y range if it's in an offscreen x range?
        }
        else
        {
            xx = Random.Range(cameraMinX, cameraMaxX);
            yy = Random.Range(cameraMinY, cameraMaxY);
        }
        float vx = Random.Range(-linearMaxSpeed, linearMaxSpeed);
        float vy = Random.Range(-linearMaxSpeed, linearMaxSpeed);
        quizItem.Initialize(triviaPair, new Vector2(xx, yy), new Vector2(vx, vy));

        quizItems.Add(quizItem);
    }

    public void RemoveQuizItem(QuizItem toRemove)
    {
        if (QuizCorrectAnswer(toRemove.data))
        {
            ++rightAnswerCount;
            --rightAnswersInPlay;
        }
        else
        {
            ++wrongAnswerCount;
            --wrongAnswersInPlay;
        }
        if (_timedMode)
        {
            GenerateQuizItem();
        }
        quizItems.Remove(toRemove);
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

    private IEnumerator DisplayTopic()
    {
        screenDimmer.color = new Color(0f, 0f, 0f, DIMMER_MAX_ALPHA);
        screenCenterText.color = Color.white;
        screenCenterText.text = TriviaParser.singleton.categoryName;

        float time = 0f;
        while(time < PREGAME_WAIT_TIME)
        {
            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;
        while(time < PREGAME_FADE_TIME)
        {
            time += Time.deltaTime;
            float alpha = (PREGAME_FADE_TIME - time) / PREGAME_FADE_TIME;
            screenDimmer.color = new Color(0f, 0f, 0f, DIMMER_MAX_ALPHA * alpha);
            screenCenterText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        screenDimmer.color = new Color(0f, 0f, 0f, 0f);
        screenCenterText.color = new Color(1f, 1f, 1f, 0f);
        _timer = INITIAL_TIME;
        gameState = GameState.PLAY;
    }

    private void UpdateWorldUnitsPerPixel()
    {
        worldUnitsPerPixel = (Camera.main.ScreenToWorldPoint(new Vector3(1, 0)) - Camera.main.ScreenToWorldPoint(new Vector3(0, 0))).magnitude;
    }

    private void InitializeFields()
    {
        quizItems = new List<QuizItem>();
        UpdateWorldUnitsPerPixel();
    }

    private void GenerateQuizItem()
    {
        if (promptIndex >= TriviaParser.singleton.prompts.Count)
        {   // Out of right answers
            if (rightAnswersInPlay == 0 || wrongAnswerIndex >= TriviaParser.singleton.wrongAnswers.Count)
            {
                //  and we need a right answer
                ShowScoreScreen();
            }
            else
            {
                GenerateWrongAnswer();
            }
        }
        else if (rightAnswersInPlay == 0 || wrongAnswerIndex >= TriviaParser.singleton.wrongAnswers.Count || Random.Range(0f, 1f) >= chanceOfRightAnswer)
        {
            GenerateRightAnswer();
        }
        else
        {
            GenerateWrongAnswer();
        }
    }

    private void GenerateRightAnswer()
    {
        CreateQuizItem(TriviaParser.singleton.rightAnswers[TriviaParser.singleton.prompts[promptIndex]][rightAnswerIndex]);
        ++rightAnswerIndex;

        if (rightAnswerIndex >= TriviaParser.singleton.rightAnswers[TriviaParser.singleton.prompts[promptIndex]].Count)
        {
            rightAnswerIndex = 0;
            ++promptIndex;
        }
        ++rightAnswersInPlay;
    }

    private void GenerateWrongAnswer()
    {
        CreateQuizItem(TriviaParser.singleton.wrongAnswers[wrongAnswerIndex++]);
        ++wrongAnswersInPlay;
    }

    #endregion

    #region monobehaviors
    void Update()
    {
        if(gameState == GameState.PLAY)
        {
            if (_timedMode == true)
            {
                _timer -= Time.deltaTime;
                int seconds = Mathf.CeilToInt(_timer);
                float pulseAmount = _timer - seconds + 1;
                int minutes = seconds / 60;
                seconds -= minutes * 60;
                timerDisplay.text = minutes.ToString("0") + ":" + seconds.ToString("00");
                if (seconds > CRITICAL_TIME_THRESHOLD || minutes > 0)
                {
                    timerDisplay.fontSize = Mathf.FloorToInt(TIMER_FONT_SMALL + (TIMER_FONT_BIGGER * Mathf.Pow(pulseAmount, TIMER_PULSE_SIZE_EXPONENT)));
                }
                else
                {
                    timerDisplay.fontSize = Mathf.FloorToInt(TIMER_FONT_SMALL + (TIMER_FONT_BIGGER_CRITICAL * Mathf.Pow(pulseAmount, TIMER_PULSE_SIZE_EXPONENT_CRITICAL)));
                    pulseAmount = Mathf.Pow(pulseAmount, TIMER_PULSE_COLOR_EXPONENT);
                    timerDisplay.color = TIMER_COLOR_CRITICAL * pulseAmount + TIMER_COLOR_NORMAL * (1 - pulseAmount);
                }

                if(_timer <= 0f)
                {
                    ShowScoreScreen();
                }
            }
        }
    }

    private void ShowScoreScreen()
    {
        interfaceCanvas.gameObject.SetActive(false);
        gameState = GameState.POSTSCREEN;
        scoreCanvas.gameObject.SetActive(true);
    }

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
        scoreCanvas.gameObject.SetActive(false);

        TriviaParser.singleton.RandomizeAnswerLists();

        rightAnswersInPlay = 0;
        wrongAnswersInPlay = 0;
        promptIndex = 0;
        rightAnswerIndex = 0;
        wrongAnswerIndex = 0;
        for (int ii = 0; ii < initialQuizItemCount; ++ii)
        {
            GenerateQuizItem();
        }

        gameState = GameState.PREGAME;
        StartCoroutine(DisplayTopic());
    }
    #endregion
}