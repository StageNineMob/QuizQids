﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StageNine;
using System.Collections.Generic;

public class GameFieldManager : MonoBehaviour
{
    //enums
    public enum GameState
    {
        NONE,
        PREGAME,
        PLAY,
        PAUSE,
        MAIN_MENU,
        POSTSCREEN,
    };

    //subclasses
    [System.Serializable]
    public class UserSettings
    {
        public float sfxVolume;

        public UserSettings()
        {
            sfxVolume = 1f;
        }
    }
    
    //consts and static data
    public static GameFieldManager singleton;
    private const float PREGAME_WAIT_TIME = 1.5f;
    private const float PREGAME_FADE_TIME = 0.5f;
    private const float DIMMER_MAX_ALPHA = 0.7f;
    private const float INITIAL_TIME = 121f;
    private const int CENTER_FONT_SIZE = 60;
    private const int TIMER_FONT_SMALL = 50;
    private const int TIMER_FONT_BIGGER = 15;
    private const int TIMER_FONT_BIGGER_CRITICAL = 30;
    private const float TIMER_PULSE_SIZE_EXPONENT = 8.0f;
    private const float TIMER_PULSE_SIZE_EXPONENT_CRITICAL = 0.4f;
    private const float TIMER_PULSE_COLOR_EXPONENT = 0.4f;
    private const float CRITICAL_TIME_THRESHOLD = 10;
    private readonly Color TIMER_COLOR_NORMAL = Color.white;
    private readonly Color TIMER_COLOR_CRITICAL = Color.red;
    private const float PROMPT_CHANGE_GRACE_PERIOD = 2.5f;
    private const int PROMPT_FONT_END_SIZE = 250;
    private const int PROMPT_FONT_START_SIZE = 0;
    private const float PROMPT_PULSE_SIZE_EXPONENT = 2f;
    private const float PROMPT_PULSE_DURATION = 1.5f;
    private const float PROMPT_PULSE_DURATION_INVERSE = 1.0f/PROMPT_PULSE_DURATION;
    private const float MAX_PROMPT_DURATION = 10f;
    private const float DUMMY_BUMP_VELOCITY = 500f;
    private const float DUMMY_ROTATION_SPEED = 18f;
    private const float DUMMY_DARKEN_RATE = 1f;
    private const float DUMMY_GRAVITY = 3000f;
    private const float REVEAL_GLOW_TIME = 0.4f;
    private const float REVEAL_FADE_TIME = 0.25f;
    private const float MULTICHOICE_BUTTON_ZOOM_TIME = 0.1f;
    private const string SETTINGS_FILE_PATH = "UserSettings.qqs";
    
    //public data
    public List<QuizItem> quizItems;
    public bool quickplayEnabled;
    public UserSettings currentSettings;

    //private data
    GameState gameState = GameState.NONE;
    [SerializeField] private float cameraMinX, cameraMinY, cameraMaxX, cameraMaxY, cameraHalfHeight, linearMaxSpeed;
    [SerializeField] private Canvas gameplayCanvas;
    [SerializeField] private Canvas interfaceCanvas;
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private Canvas multiChoiceCanvas;

    [SerializeField] private Slider promptChangeTimer;
    [SerializeField] private GameObject quizItemPrefab;
    [SerializeField] private AudioSource soundEffectSource;
    [SerializeField] private AudioClip rightAnswerSound;
    [SerializeField] private float rightAnswerVolume;
    [SerializeField] private AudioClip wrongAnswerSound;
    [SerializeField] private float wrongAnswerVolume;
    [SerializeField] private AudioClip promptChangeSound;
    [SerializeField] private float promptChangeVolume;
    [SerializeField] private Text[] choiceButtonText;
    [SerializeField] private GameObject correctButtonDummy;
    [SerializeField] private GameObject incorrectButtonDummy;
    [SerializeField] private Text correctButtonDummyText;
    [SerializeField] private Text incorrectButtonDummyText;

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
    [SerializeField] private Button replayButton;
    [SerializeField] private Button endGameButton;
    

    private float worldUnitsPerPixel;
    private string currentPrompt = null, gracePrompt = null;
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
        get
        {
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
                ChangeState(GameState.PAUSE);
                break;
            case GameState.PAUSE:
                ChangeState(GameState.PLAY);
                break;
            default:
                break;
        }

    }
    public void PressedEndGameButton()
    {
        pauseButtonText.text = "Pause";
        ChangeState(GameState.MAIN_MENU);
    }

    public void PressedChoiceButton(int buttonIndex)
    {
        if (TriviaParser.singleton.rightAnswers[rightAnswerIndex].prompts.Contains(choiceButtonText[buttonIndex].text))
        {
            //like they got it right
            rightAnswerCount++;
            PlaySound(rightAnswerSound, rightAnswerVolume);
            StartCoroutine(RightAnswerDummyAnimation(buttonIndex));
        }
        else
        {
            //they got it rawng;
            wrongAnswerCount++;
            PlaySound(wrongAnswerSound, wrongAnswerVolume);
            StartCoroutine(WrongAnswerDummyAnimation(buttonIndex));
            for(int ii = 0; ii < choiceButtonText.Length; ii++)
            {
                if (TriviaParser.singleton.rightAnswers[rightAnswerIndex].prompts.Contains(choiceButtonText[ii].text))
                {
                    StartCoroutine(RevealAnswerDummyAnimation(ii));
                    break;
                }

            }
            //TODO: find right answer's index and have it play the "this was the right answer" animation
            //StartCoroutine(RightAnswerDummyAnimation(buttonIndex));
        }
        NextMultiChoiceItem();
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
            if (gracePrompt != null)
            {
                if (trivia.prompts.Contains(gracePrompt))
                    return true;
            }
        }
        else
        {
            if (trivia.prompts.Count > 0)
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
            PlaySound(rightAnswerSound, rightAnswerVolume);
        }
        else
        {
            ++wrongAnswerCount;
            --wrongAnswersInPlay;
            PlaySound(wrongAnswerSound, wrongAnswerVolume);
        }
        if (_timedMode)
        {
            GenerateQuizItem();
        }
        quizItems.Remove(toRemove);
    }

    public void ChangeVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp(v, 0, 1);
        //PlaySound(wrongAnswerSound, wrongAnswerVolume);
    }

    public void PlayAgainButton()
    {
        if (quickplayEnabled)
        {
            UIMenuManager.singleton.PressedQuickPlayButton();
        }
        else
        {
            ChangeState(GameState.PREGAME);
        }
    }

    public void ReplayButton()
    {
        ChangeState(GameState.PREGAME);
    }

    public void ChangeState(GameState newState)
    {
        GameState previousState = gameState;
        StateExit(gameState);
        gameState = newState;
        StateEnter(newState, previousState);
    }

    public void PlaySound(AudioClip sound, float volume)
    {
        soundEffectSource.PlayOneShot(sound, volume);
    }

    public void ResetApplicationSettings()
    {
        ChangeVolume(currentSettings.sfxVolume);
    }

    public void UpdateCurrentSettings()
    {
        currentSettings.sfxVolume = AudioListener.volume;
    }

    public void SaveCurrentSettings()
    {
        FileManager.singleton.Save(currentSettings, SETTINGS_FILE_PATH);
    }
    #endregion

    #region private methods
    private void PauseGame()
    {
        pauseButtonText.text = "Resume";
        screenDimmer.color = Color.black * DIMMER_MAX_ALPHA;
        screenCenterText.fontSize = CENTER_FONT_SIZE;
        screenCenterText.horizontalOverflow = HorizontalWrapMode.Wrap;
        screenCenterText.color = Color.white;

        promptDisplay.gameObject.SetActive(false);
        string text = TriviaParser.singleton.categoryName;
        if (TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.SPECIFIC)
        {
            text += System.Environment.NewLine + currentPrompt;
        }
        screenCenterText.text = text;
    }

    private void UnpauseGame()
    {
        pauseButtonText.text = "Pause";
        screenDimmer.color = Color.clear;
        screenCenterText.color = Color.clear;
        promptDisplay.gameObject.SetActive(true);
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

    private IEnumerator RightAnswerDummyAnimation(int buttonIndex)
    {
        int animationIndex = rightAnswerCount;
        var buttonText = choiceButtonText[buttonIndex];
        correctButtonDummy.SetActive(true);
        correctButtonDummy.transform.position = buttonText.transform.parent.position;
        correctButtonDummyText.text = buttonText.text;
        float time = 0f;

        while (time < QuizItem.CORRECT_FADE_TIME && animationIndex == rightAnswerCount)
        {
            time += Time.deltaTime;
            float alpha = (QuizItem.CORRECT_FADE_TIME - time) / QuizItem.CORRECT_FADE_TIME;
            float scale = Mathf.Lerp(1f, QuizItem.CORRECT_SCALE_AMOUNT, time / QuizItem.CORRECT_FADE_TIME);
            //MAYBEDO: Lighten colors as they fade out? 
            correctButtonDummyText.color = new Color(0f, 0f, 0f, alpha);
            correctButtonDummy.GetComponent<Image>().color = new Color(0f, 1f, 0f, alpha);
            correctButtonDummy.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (animationIndex == rightAnswerCount)
        {
            correctButtonDummy.gameObject.SetActive(false);
        }
    }

    private IEnumerator RevealAnswerDummyAnimation(int buttonIndex)
    {
        int animationIndex = rightAnswerCount + wrongAnswerCount;
        var buttonText = choiceButtonText[buttonIndex];
        correctButtonDummy.SetActive(true);
        correctButtonDummy.transform.position = buttonText.transform.parent.position;
        correctButtonDummyText.text = buttonText.text;
        float time = 0f;

        while (time < REVEAL_FADE_TIME + REVEAL_GLOW_TIME && animationIndex == rightAnswerCount + wrongAnswerCount)
        {
            time += Time.deltaTime;
            float alpha = 1;
            float white = 0;
            if (time < REVEAL_GLOW_TIME)
            {
                white = 1 - time / REVEAL_GLOW_TIME;
            }
            if (time > REVEAL_GLOW_TIME)
            {
                alpha = 1 - (time - REVEAL_GLOW_TIME) / REVEAL_FADE_TIME;
            }
            float scale = Mathf.Lerp(1f, QuizItem.CORRECT_SCALE_AMOUNT, time / (REVEAL_FADE_TIME + REVEAL_GLOW_TIME));
            //MAYBEDO: Lighten colors as they fade out? 
            correctButtonDummyText.color = new Color(0f, 0f, 0f, alpha);
            correctButtonDummy.GetComponent<Image>().color = new Color(white, 1f, white, alpha);
            correctButtonDummy.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (animationIndex == rightAnswerCount + wrongAnswerCount)
        {
            correctButtonDummy.gameObject.SetActive(false);
        }
    }

    private IEnumerator WrongAnswerDummyAnimation(int buttonIndex)
    {
        int animationIndex = wrongAnswerCount;
        var buttonText = choiceButtonText[buttonIndex];
        incorrectButtonDummy.SetActive(true);
        incorrectButtonDummy.transform.localPosition = buttonText.transform.parent.localPosition;
        incorrectButtonDummyText.text = buttonText.text;
        Vector2 linearVelocity = Vector2.zero;
        float time = 0f;

        float randomAngle = Random.Range(-1f, 1f);
        float twistRate = DUMMY_ROTATION_SPEED;
        if (Random.Range(0, 2) == 0)
        {
            twistRate = -DUMMY_ROTATION_SPEED;
        }
        linearVelocity += (Vector2.left * Mathf.Sin(randomAngle) + Vector2.up * Mathf.Cos(randomAngle)) * DUMMY_BUMP_VELOCITY;
        while (incorrectButtonDummy.transform.localPosition.y > -QuizItem.DISTANCE_OFFSCREEN && animationIndex == wrongAnswerCount)
        {
            //Debug.Log("[GameFieldManager:WrongAnswerDummyAnimation]");
            time += Time.deltaTime;
            linearVelocity += Vector2.down * DUMMY_GRAVITY * Time.deltaTime;
            incorrectButtonDummy.transform.localRotation = Quaternion.Euler(0, 0, twistRate * time);
            incorrectButtonDummy.GetComponent<Image>().color = new Color(1f - time * DUMMY_DARKEN_RATE, 0f, 0f);

            incorrectButtonDummy.transform.localPosition += (Vector3)(linearVelocity * Time.deltaTime);

            yield return null;
        }
        if (animationIndex == wrongAnswerCount)
        {
            incorrectButtonDummy.gameObject.SetActive(false);
        }
    }

    private IEnumerator DisplayTopic()
    {
        screenDimmer.color = new Color(0f, 0f, 0f, DIMMER_MAX_ALPHA);
        screenCenterText.color = Color.white;
        screenCenterText.text = TriviaParser.singleton.categoryName;
        screenCenterText.horizontalOverflow = HorizontalWrapMode.Wrap;
        screenCenterText.fontSize = CENTER_FONT_SIZE;

        promptChangeTimer.gameObject.SetActive(false);
        promptDisplay.gameObject.SetActive(false);

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
        ChangeState(GameState.PLAY);
        
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
                ChangeState(GameState.POSTSCREEN);
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
        
        TriviaParser.singleton.RandomizeAnswerLists();

        rightAnswerIndex = -1; // in multi choice, index is immediately incremented
        wrongAnswerIndex = 0;
        if (TriviaParser.singleton.triviaMode != TriviaParser.TriviaMode.MULTIPLE_CHOICE)
        {
            rightAnswerIndex = 0;
            rightAnswersInPlay = 0;
            wrongAnswersInPlay = 0;
            for (int ii = 0; ii < initialQuizItemCount; ++ii)
            {
                GenerateQuizItem();
            }
        }
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
        scoreCanvas.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(quickplayEnabled);
    }


    private void ClearGameData()
    {
        foreach (var item in quizItems)
        {
            Destroy(item.gameObject);
        }
        quizItems.Clear();
        currentPrompt = null;
        gracePrompt = null;
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
        screenCenterText.horizontalOverflow = HorizontalWrapMode.Overflow;
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
            else
            {
                timeRemaining = 0;
                gracePrompt = null;
            }
            yield return null;
        }
        if(gameState != GameState.PAUSE)
        {
            screenCenterText.color = Color.clear;
        }
    }

    private IEnumerator DisplayNewMultiChoice()
    {
        int animationIndex = rightAnswerCount + wrongAnswerCount;
        promptDisplay.text = currentPrompt;
        float time = 0f;
        while(time < MULTICHOICE_BUTTON_ZOOM_TIME && animationIndex == rightAnswerCount + wrongAnswerCount)
        {
            float zoom = time / MULTICHOICE_BUTTON_ZOOM_TIME;
            foreach(var button in choiceButtonText)
            {
                button.transform.parent.localScale = Vector3.one * zoom;
            }
            time += Time.deltaTime;
            yield return null;
        }
        if(animationIndex == rightAnswerCount + wrongAnswerCount)
        {
            foreach (var button in choiceButtonText)
            {
                button.transform.parent.localScale = Vector3.one;
            }
        }
    }

    private void TriviaSearchUpdate()
    {
        if (TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.SPECIFIC)
        {
            currentPromptDuration += Time.deltaTime;
            promptChangeTimer.value = 1 - currentPromptDuration / MAX_PROMPT_DURATION;
            if (gracePrompt != null && currentPromptDuration >= PROMPT_CHANGE_GRACE_PERIOD)
            {
                gracePrompt = null;
            }
            // also change prompt if nothing matching current prompt is in play
            if (currentPrompt == null || NothingMatches() || currentPromptDuration >= MAX_PROMPT_DURATION)
            {
                //this should be a random one, rather than always quiz item 0
                //Also want to make sure the prompt is never the same as it just was unless there's no other option
                currentPromptDuration = 0f;
                if (ChooseRandomPrompt())
                {
                    PlaySound(promptChangeSound, promptChangeVolume);
                    StartCoroutine(DisplayNewPrompt());
                }
            }
        }
    }

    private void MultiChoiceUpdate()
    {
        if(currentPrompt == null)
        {
            NextMultiChoiceItem();
        }
    }

    private void NextMultiChoiceItem()
    {
        ++rightAnswerIndex;
        if(rightAnswerIndex >= TriviaParser.singleton.rightAnswers.Count)
        {
            ChangeState(GameState.POSTSCREEN);
        }
        else
        {
            var pair = TriviaParser.singleton.rightAnswers[rightAnswerIndex];
            currentPrompt = pair.value;
            StartCoroutine(DisplayNewMultiChoice());
            var correctButton = Random.Range(0, choiceButtonText.Length);

            TriviaParser.Shuffle(pair.prompts);
            TriviaParser.singleton.RandomizePrompts();
            int promptIndex = 0;
            for(var buttonIndex = 0; buttonIndex < choiceButtonText.Length; ++buttonIndex)
            {
                if(buttonIndex == correctButton)
                {
                    choiceButtonText[buttonIndex].text = pair.prompts[0];
                }
                else
                {
                    while(pair.prompts.Contains(TriviaParser.singleton.prompts[promptIndex]) && promptIndex < TriviaParser.singleton.prompts.Count)
                    {
                        ++promptIndex;
                    }
                    if(promptIndex < TriviaParser.singleton.prompts.Count)
                    {
                        choiceButtonText[buttonIndex].text = TriviaParser.singleton.prompts[promptIndex];
                        ++promptIndex;
                    }
                    else
                    {
                        // TODO: How do we handle running out of prompts?
                        Debug.LogError("[GameFieldManager:NextMultiChoiceItem] Ran out of incorrect prompts!");
                    }
                }
            }
        }
    }

    private void TimerUpdate()
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

        if (_timer <= 0f)
        {
            ChangeState(GameState.POSTSCREEN);
        }
    }

    private void StateExit(GameState state)
    {
        switch (state)
        {
            case GameState.MAIN_MENU:
                mainMenuCanvas.gameObject.SetActive(false);
                break;
            case GameState.PAUSE:
                endGameButton.gameObject.SetActive(false);
                //gameplayCanvas.gameObject.SetActive(false);
                interfaceCanvas.gameObject.SetActive(false);
                break;
            case GameState.PLAY:
                pauseButtonText.text = "Pause";
                if (TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.MULTIPLE_CHOICE)
                {
                    multiChoiceCanvas.gameObject.SetActive(false);
                }
                gameplayCanvas.gameObject.SetActive(false);
                interfaceCanvas.gameObject.SetActive(false);
                break;
            case GameState.POSTSCREEN:
                scoreCanvas.gameObject.SetActive(false);
                gameplayCanvas.gameObject.SetActive(false);
                break;
            case GameState.PREGAME:
                gameplayCanvas.gameObject.SetActive(false);
                interfaceCanvas.gameObject.SetActive(false);
                _timer = INITIAL_TIME;

                if (TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.SPECIFIC)
                {
                    promptChangeTimer.gameObject.SetActive(true);
                    promptDisplay.gameObject.SetActive(true);
                }
                if (TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.MULTIPLE_CHOICE)
                {
                    //promptChangeTimer.gameObject.SetActive(true);
                    promptDisplay.gameObject.SetActive(true);
                }
                break;
            case GameState.NONE:
                gameplayCanvas.gameObject.SetActive(false);
                interfaceCanvas.gameObject.SetActive(false);
                mainMenuCanvas.gameObject.SetActive(false);
                multiChoiceCanvas.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void StateEnter(GameState state, GameState prevState)
    {
        switch (state)
        {
            case GameState.MAIN_MENU:
                UIMenuManager.singleton.ResetMainMenu();
                mainMenuCanvas.gameObject.SetActive(true);
                break;
            case GameState.PAUSE:
                if (prevState == GameState.PLAY)
                {
                    PauseGame();
                }
                //gameplayCanvas.gameObject.SetActive(true);
                interfaceCanvas.gameObject.SetActive(true);
                endGameButton.gameObject.SetActive(true);
                break;
            case GameState.PLAY:
                if (prevState == GameState.PAUSE)
                {
                    UnpauseGame();
                }
                if(TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.MULTIPLE_CHOICE)
                {
                    multiChoiceCanvas.gameObject.SetActive(true);
                }
                gameplayCanvas.gameObject.SetActive(true);
                interfaceCanvas.gameObject.SetActive(true);
                break;
            case GameState.POSTSCREEN:
                scoreCanvas.gameObject.SetActive(true);
                gameplayCanvas.gameObject.SetActive(true);
                ShowScoreScreen();
                break;
            case GameState.PREGAME:
                gameplayCanvas.gameObject.SetActive(true);
                interfaceCanvas.gameObject.SetActive(true);
                ClearGameData();
                GameSetup();
                break;
            case GameState.NONE:
            default:
                break;
        }
    }
    #endregion

    #region monobehaviors
    void Update()
    {
        if(gameState == GameState.PLAY)
        {
            if(TriviaParser.singleton.triviaMode == TriviaParser.TriviaMode.MULTIPLE_CHOICE)
            {
                MultiChoiceUpdate();
            }
            else
            {
                TriviaSearchUpdate();
            }
            if (_timedMode == true)
            {
                TimerUpdate();
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
                    gracePrompt = currentPrompt;
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
        if (FileManager.singleton.FileExists(SETTINGS_FILE_PATH))
        {
            currentSettings = FileManager.singleton.Load<UserSettings>(SETTINGS_FILE_PATH);
            if (null == currentSettings)
                currentSettings = new UserSettings();
        }
        else
        {
            currentSettings = new UserSettings();
            FileManager.singleton.Save(currentSettings, SETTINGS_FILE_PATH);
        }
        ResetApplicationSettings();

        UIMenuManager.singleton.HidePlayMenuPanel();

        ChangeState(GameState.MAIN_MENU);
        endGameButton.gameObject.SetActive(false);
    }

    #endregion
}