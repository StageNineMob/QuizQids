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
        PAUSE,
        MAIN_MENU,
        POSTSCREEN,

    };
    //subclasses

    //consts and static data

    public static GameFieldManager singleton;
    private const float PREGAME_WAIT_TIME = 1.5f;
    private const float PREGAME_FADE_TIME = 0.5f;
    private const float DIMMER_MAX_ALPHA = 0.7f;
    private const float INITIAL_TIME = 120f;
    private const int TIMER_FONT_SMALL = 50;
    private const int TIMER_FONT_BIGGER = 15;
    private const int TIMER_FONT_BIGGER_CRITICAL = 30;
    private const float TIMER_PULSE_SIZE_EXPONENT = 8.0f;
    private const float TIMER_PULSE_SIZE_EXPONENT_CRITICAL = 0.4f;
    private const float TIMER_PULSE_COLOR_EXPONENT = 0.4f;
    private const float CRITICAL_TIME_THRESHOLD = 10;
    private readonly Color TIMER_COLOR_NORMAL = Color.white;
    private readonly Color TIMER_COLOR_CRITICAL = Color.red;
    private const float PROMPT_FONT_END_SIZE = 250;
    private const float PROMPT_FONT_START_SIZE = 0;
    private const float PROMPT_PULSE_SIZE_EXPONENT = 2f;
    private const float PROMPT_PULSE_DURATION = 1.5f;
    private const float PROMPT_PULSE_DURATION_INVERSE = 1.0f/PROMPT_PULSE_DURATION;
    private const float MAX_PROMPT_DURATION = 10f;

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
    [SerializeField] private Text promptDisplay;
    [SerializeField] private Image screenDimmer;
    [SerializeField] private Text screenCenterText;
    [SerializeField] private Text scoreUIRightText;
    [SerializeField] private Text scoreUIWrongText;
    [SerializeField] private Text scoreUIAccuracyText;
    [SerializeField] private Text scoreUIArbitraryText;
    [SerializeField] private Text pauseButtonText;


    private float worldUnitsPerPixel;
    private string currentPrompt = null;
    private int rightAnswersInPlay = 0;
    private int wrongAnswersInPlay = 0;
    private int initialQuizItemCount = 30;
    //private int promptIndex = 0;
    private int rightAnswerIndex = 0;
    private int wrongAnswerIndex = 0;
    private float chanceOfRightAnswer = .5f;

    private float currentPromptDuration = 0f;

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
    
    public void PressedPauseButton()
    {
        switch (gameState)
        {
            case GameState.PLAY:
                PauseGame();
                break;
            case GameState.PAUSE:
                UnpauseGame();
                break;
            default:
                break;
        }

    }

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
            if (trivia.prompts.Contains(currentPrompt))
                return true;
        }
        else
        {
            if (trivia.prompts.Contains("null"))
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

    public void PlayAgainButton()
    {
        ClearGameData();
        GameSetup();
    }

    #endregion

#region private methods
    private void PauseGame()
    {
        if(gameState == GameState.PLAY)
        {
            gameState = GameState.PAUSE;
            pauseButtonText.text = "Resume";
            foreach (var item in quizItems)
            {
                item.gameObject.SetActive(false);
            }
            screenDimmer.color = Color.black * DIMMER_MAX_ALPHA;
            screenCenterText.color = Color.white;
        }
    }

    private void UnpauseGame()
    {
        if (gameState == GameState.PAUSE)
        {
            gameState = GameState.PLAY;
            pauseButtonText.text = "Pause";
            foreach (var item in quizItems)
            {
                item.gameObject.SetActive(true);
            }
            screenDimmer.color = Color.clear;
            screenCenterText.color = Color.clear;
        }
    }

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
        if (rightAnswerIndex >= TriviaParser.singleton.rightAnswers.Count)
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
        CreateQuizItem(TriviaParser.singleton.rightAnswers[rightAnswerIndex]);
        ++rightAnswerIndex;

        //if (rightAnswerIndex >= TriviaParser.singleton.rightAnswers[TriviaParser.singleton.prompts[promptIndex]].Count)
        //{
        //    rightAnswerIndex = 0;
        //    ++promptIndex;
        //}
        ++rightAnswersInPlay;
    }

    private void GenerateWrongAnswer()
    {
        CreateQuizItem(TriviaParser.singleton.wrongAnswers[wrongAnswerIndex++]);
        ++wrongAnswersInPlay;
    }


    private void GameSetup()
    {
        scoreCanvas.gameObject.SetActive(false);
        interfaceCanvas.gameObject.SetActive(true);

        TriviaParser.singleton.RandomizeAnswerLists();

        rightAnswersInPlay = 0;
        wrongAnswersInPlay = 0;
        //promptIndex = 0;
        rightAnswerIndex = 0;
        wrongAnswerIndex = 0;
        for (int ii = 0; ii < initialQuizItemCount; ++ii)
        {
            GenerateQuizItem();
        }

        gameState = GameState.PREGAME;
        StartCoroutine(DisplayTopic());
    }

    private void ShowScoreScreen()
    {
        float elapsedTime = _timer;
        if (_timedMode)
        {
            elapsedTime = INITIAL_TIME - _timer;
        }

        scoreUIRightText.text = _rightAnswerCount.ToString();
        scoreUIWrongText.text = _wrongAnswerCount.ToString();
        float accuracy = 0f;
        if (_rightAnswerCount + _wrongAnswerCount > 0)
        {
            accuracy = (float)_rightAnswerCount / (_rightAnswerCount + _wrongAnswerCount);
        }
        string accuracyText = (accuracy * 100).ToString("##0") + "%";
        if (accuracy < 1 && accuracyText == "100%")
        {
            accuracyText = "99%";
        }
        scoreUIAccuracyText.text = accuracyText;
        scoreUIArbitraryText.text = (Mathf.FloorToInt(rightAnswerCount * accuracy / elapsedTime * 1000)).ToString();

        interfaceCanvas.gameObject.SetActive(false);
        gameState = GameState.POSTSCREEN;
        scoreCanvas.gameObject.SetActive(true);
    }


    private void ClearGameData()
    {
        foreach (var item in quizItems)
        {
            Destroy(item.gameObject);
        }
        quizItems.Clear();
        currentPrompt = null;
        rightAnswerCount = 0;
        wrongAnswerCount = 0;
        timerDisplay.color = TIMER_COLOR_NORMAL;
        timerDisplay.fontSize = TIMER_FONT_SMALL;
        int seconds = Mathf.CeilToInt(INITIAL_TIME);
        int minutes = seconds / 60;
        seconds -= minutes * 60;
        timerDisplay.text = minutes.ToString("0") + ":" + seconds.ToString("00");
    }

    private IEnumerator DisplayNewPrompt()
    {
        promptDisplay.text = currentPrompt;
        screenCenterText.text = currentPrompt;
        float timeRemaining = PROMPT_PULSE_DURATION;

        screenCenterText.color = Color.white;
        while(timeRemaining > 0)
        {
            if(!(gameState == GameState.PAUSE))
            {
                float pulseAmount = Mathf.Pow(timeRemaining * PROMPT_PULSE_DURATION_INVERSE, PROMPT_PULSE_SIZE_EXPONENT);
                float fontSize = Mathf.Lerp(PROMPT_FONT_END_SIZE, PROMPT_FONT_START_SIZE, pulseAmount);

                screenCenterText.fontSize = (int)fontSize;
                screenCenterText.color = new Color (1,1,1,timeRemaining * PROMPT_PULSE_DURATION_INVERSE);
                timeRemaining -= Time.deltaTime;
            }
            yield return null;
        }
        screenCenterText.color = Color.clear;

    }
    #endregion

    #region monobehaviors
    void Update()
    {
        if(gameState == GameState.PLAY)
        {
            if(TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.SPECIFIC)
            {
                currentPromptDuration += Time.deltaTime;
                // also change prompt if nothing matching current prompt is in play
                if(currentPrompt == null || NothingMatches() || currentPromptDuration >= MAX_PROMPT_DURATION)
                {
                    //this should be a random one, rather than always quiz item 0
                    //Also want to make sure the prompt is never the same as it just was unless there's no other option
                    currentPromptDuration = 0f;
                    if(ChooseRandomPrompt())
                    {
                        StartCoroutine(DisplayNewPrompt());
                    }
                }
            }
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

    private bool ChooseRandomPrompt()
    {
        string lastPrompt = currentPrompt;
        TriviaParser.Shuffle(quizItems);
        foreach (var item in quizItems)
        {
            foreach (var prompt in item.data.prompts)
            {
                if (prompt != lastPrompt)
                {
                    currentPrompt = prompt;
                    return true;
                }
            }
        }
        // if we never found a new prompt, return false
        return false;
    }

    private bool NothingMatches()
    {
        foreach(var item in quizItems)
        {
            if (QuizCorrectAnswer(item.data))
                return false;
        }
        return true;
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
        ClearGameData();
        GameSetup();
    }

    #endregion
}