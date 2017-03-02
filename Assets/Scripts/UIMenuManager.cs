using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UIMenuManager : MonoBehaviour {

    [SerializeField]
    private GameObject mainMenuPanel;
    [SerializeField]
    private GameObject playMenuPanel;
    [SerializeField]
    private GameObject logoText;

    [SerializeField]
    private Button playMultiChoiceButton;
    [SerializeField]
    private Button playTriviaSearchButton;
    [SerializeField]
    private FileViewer fileViewer;

    public static UIMenuManager singleton;

    public void DisableGameModeButtons()
    {
        playMultiChoiceButton.interactable = false;
        playTriviaSearchButton.interactable = false;
    }

    public void EnableGameModeButtons()
    {
        if ((TriviaParser.singleton.triviaMode & TriviaParser.TriviaMode.MULTIPLE_CHOICE) == TriviaParser.TriviaMode.MULTIPLE_CHOICE)
        {
            playMultiChoiceButton.interactable = true;
        }
        if ((TriviaParser.singleton.triviaMode & (TriviaParser.TriviaMode.SPECIFIC | TriviaParser.TriviaMode.GENERAL)) != TriviaParser.TriviaMode.NONE)
        {
            playTriviaSearchButton.interactable = true;
        }
    }

    public void PressedPlayButton()
    {
        mainMenuPanel.SetActive(false);
        logoText.SetActive(false);

        fileViewer.ClearList();
        fileViewer.PopulateList(TriviaParser.TRIVIA_DIRECTORY, TriviaParser.TRIVIA_FILE_EXT);
        DisableGameModeButtons();
        playMenuPanel.SetActive(true);
    }

    public void PressedPlayMultiChoiceButton()
    {
        // TODO: Confirm which category to play rather than forcing 0
        TriviaParser.singleton.LoadTrivia(0, TriviaParser.TriviaMode.MULTIPLE_CHOICE);
    }

    public void PressedPlayTriviaSearchButton()
    {
        // TODO: Confirm which category to play rather than forcing 0
        // TODO: Confirm general vs. specific
        var mode = TriviaParser.TriviaMode.GENERAL;
        if ((TriviaParser.singleton.triviaMode & TriviaParser.TriviaMode.SPECIFIC) == TriviaParser.TriviaMode.SPECIFIC)
        {
            mode = TriviaParser.TriviaMode.SPECIFIC;
        }
        TriviaParser.singleton.LoadTrivia(0, mode);
    }

    public void ResetMainMenu()
    {
        logoText.SetActive(true);
        mainMenuPanel.SetActive(true);
        playMenuPanel.SetActive(false);
    }

    public void HidePlayMenuPanel()
    {
        playMenuPanel.SetActive(false);
    }

    // Use this for initialization
    void Start () {
	
	}

    void Awake()
    {
        Debug.Log("[UIMenuManager:Awake]");
        if (singleton == null)
        {
            Debug.Log("Hello, world!");
            singleton = this;
            //InitializeFields();
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
