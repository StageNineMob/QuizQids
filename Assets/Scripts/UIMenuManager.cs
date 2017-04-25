using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using StageNine;

public class UIMenuManager : MonoBehaviour {
    //Consts and Static Data
    private const float MENU_SCROLL_DISTANCE_X = 800f;
    private const float MENU_SCROLL_DISTANCE_Y = 500f;
    private const float MENU_SCROLL_TIME_INVERSE = 1/0.25f;
    private readonly FloatCurveData MENU_SCROLL_CURVE = new FloatCurveData(0, 1, 0, 0);

    [SerializeField] private List<GameObject> toInitList;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject menuBlockingPanel;

    [SerializeField] private Button playMultiChoiceButton;
    [SerializeField] private Button playTriviaSearchButton;
    [SerializeField] private FileViewer fileViewer;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumeText;

    [SerializeField] private Slider animationTimeSlider;
    [SerializeField] private Text animationTimeText;

    //properties

    public int animationTimeSliderSetting
    {
        get { return (int)animationTimeSlider.value; }
        set { animationTimeSlider.value = value; }
    }

    public static UIMenuManager singleton;

    public void DisableGameModeButtons()
    {
        playMultiChoiceButton.interactable = false;
        playTriviaSearchButton.interactable = false;
    }

    public void EnableGameModeButtons()
    {
        if ((TriviaParser.singleton.categoryList[fileViewer.currentCategoryNumber].Value & TriviaParser.TriviaMode.MULTIPLE_CHOICE) == TriviaParser.TriviaMode.MULTIPLE_CHOICE)
        {
            playMultiChoiceButton.interactable = true;
        }
        if ((TriviaParser.singleton.categoryList[fileViewer.currentCategoryNumber].Value & (TriviaParser.TriviaMode.SPECIFIC | TriviaParser.TriviaMode.GENERAL)) != TriviaParser.TriviaMode.NONE)
        {
            playTriviaSearchButton.interactable = true;
        }
    }

    public void PressedPlayButton()
    {
        fileViewer.ClearList();
        fileViewer.PopulateList(TriviaParser.TRIVIA_DIRECTORY, TriviaParser.TRIVIA_FILE_EXT);
        DisableGameModeButtons();
        //forcibly call function for text has changed
        fileViewer.ResetFileViewer();
        StartCoroutine(MenuTransition(mainMenuPanel, playMenuPanel));
    }

    public void PressedPlayMultiChoiceButton()
    {
        TriviaParser.singleton.LoadTrivia(fileViewer.currentCategoryNumber, TriviaParser.TriviaMode.MULTIPLE_CHOICE);
        GameFieldManager.singleton.ChangeState(GameFieldManager.GameState.PREGAME);
        GameFieldManager.singleton.quickplayEnabled = false;
    }

    public void PressedPlayTriviaSearchButton()
    {
        // TODO: Confirm general vs. specific
        var mode = TriviaParser.TriviaMode.GENERAL;
        if ((TriviaParser.singleton.categoryList[fileViewer.currentCategoryNumber].Value & TriviaParser.TriviaMode.SPECIFIC) == TriviaParser.TriviaMode.SPECIFIC)
        {
            mode = TriviaParser.TriviaMode.SPECIFIC;
        }
        TriviaParser.singleton.LoadTrivia(fileViewer.currentCategoryNumber, mode);
        GameFieldManager.singleton.ChangeState(GameFieldManager.GameState.PREGAME);
        GameFieldManager.singleton.quickplayEnabled = false;
    }

    public void PressedQuickPlayButton()
    {
        fileViewer.SelectRandomElement();
        if (playMultiChoiceButton.interactable && playTriviaSearchButton.interactable)
        {
            //If we ever add another game mode, we're gonna wanna remember this 2. 
            //                               |
            //                               V
            int randomMode = Random.Range(0, 2);
            Debug.Log("[UIMenuManager:PressedQuickPlayButton] randomMode = " + randomMode);
            if (randomMode == 1)
            {
                PressedPlayTriviaSearchButton();
            }
            else
            {
                PressedPlayMultiChoiceButton();
            }
        }
        else
        {
            if (playMultiChoiceButton.interactable)
            {
                PressedPlayMultiChoiceButton();
            }
            else if (playTriviaSearchButton.interactable)
            {
                PressedPlayTriviaSearchButton();
            }
            else
            {
                Debug.Log("[UIMenuManager:PressedQuickPlayButton] trying to quickplay with no available game modes");
            }
        }
        GameFieldManager.singleton.quickplayEnabled = true;
    }

    public void PressedProfileButton()
    {
        TriviaParser.singleton.InitProfilerMode();
        GameFieldManager.singleton.ChangeState(GameFieldManager.GameState.PREGAME);
    }

    public void PressedOptionsButton()
    {
        volumeSlider.value = GameFieldManager.singleton.currentSettings.sfxVolume;
        StartCoroutine(MenuTransition(mainMenuPanel, optionsPanel));
    }

    public void PressedCancelOptionsButton()
    {
        GameFieldManager.singleton.ResetApplicationSettings();
        StartCoroutine(MenuTransition(optionsPanel, mainMenuPanel));
    }

    public void PressedConfirmOptionsButton()
    {
        GameFieldManager.singleton.UpdateCurrentSettings();
        GameFieldManager.singleton.SaveCurrentSettings();
        StartCoroutine(MenuTransition(optionsPanel, mainMenuPanel));
    }

    public void ChangeVolume()
    {
        GameFieldManager.singleton.ChangeVolume(volumeSlider.value);
        volumeText.text = Mathf.FloorToInt(volumeSlider.value * 100) + "%" ;
    }

    public void ChangeAnimationTime()
    {
        GameFieldManager.singleton.SetAnimationSpeed(Mathf.FloorToInt(animationTimeSlider.value));
    }

    public void AnimationTimeDisplayName(string newText)
    {
        animationTimeText.text = newText;
    }

    public void ResetMainMenu()
    {
        mainMenuPanel.SetActive(true);
        playMenuPanel.SetActive(false);
    }

    public void HidePlayMenuPanel()
    {
        StartCoroutine(MenuTransition(playMenuPanel, mainMenuPanel));
    }

    private Vector3 getRandomScrollDirection()
    {
        float randomNumber = Random.Range(0f, 1f);
        if (0.25 > randomNumber)
        {
            return new Vector3(MENU_SCROLL_DISTANCE_X, MENU_SCROLL_DISTANCE_Y * (randomNumber * 8 - 1));
        }
        else if (0.5 > randomNumber)
        {
            return new Vector3(-MENU_SCROLL_DISTANCE_X, MENU_SCROLL_DISTANCE_Y * (randomNumber * 8 - 3));
        }
        else if (0.75 > randomNumber)
        {
            return new Vector3(MENU_SCROLL_DISTANCE_X * (randomNumber * 8 - 5), MENU_SCROLL_DISTANCE_Y);
        }
        else
        {
            return new Vector3(MENU_SCROLL_DISTANCE_X * (randomNumber * 8 - 7), -MENU_SCROLL_DISTANCE_Y);
        }
    }

    private IEnumerator MenuTransition(GameObject oldPanel, GameObject newPanel)
    {
        Vector3 newPanelLocation = newPanel.transform.localPosition;
        Vector3 oldPanelLocation = oldPanel.transform.localPosition;
        Vector3 scrollDirection = getRandomScrollDirection();

        newPanel.SetActive(true);
        menuBlockingPanel.SetActive(true);
        float time = 0;

        while(time < 1)
        {
            float currentAmount = MENU_SCROLL_CURVE.GetFloatValue(time);
            oldPanel.transform.localPosition = oldPanelLocation + scrollDirection * currentAmount;
            newPanel.transform.localPosition = newPanelLocation + scrollDirection * (currentAmount - 1);
            time += Time.deltaTime * MENU_SCROLL_TIME_INVERSE;
            yield return null;
        }
        menuBlockingPanel.SetActive(false);
        oldPanel.SetActive(false);
        newPanel.transform.localPosition = newPanelLocation;
        oldPanel.transform.localPosition = oldPanelLocation;
    }

    // Use this for initialization
    void Start () {
        foreach (var item in toInitList)
        {
            item.SetActive(false);
        }

        optionsPanel.SetActive(false);
    }

    void Awake()
    {
        Debug.Log("[UIMenuManager:Awake]");
        if (singleton == null)
        {
            Debug.Log("Hello, world!");
            singleton = this;
            foreach (var item in toInitList)
            {
                item.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Goodbye, cruel world!");
            GameObject.Destroy(gameObject);
        }


    }


    // Update is called once per frame
    void Update () {
	
	}
}
