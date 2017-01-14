using UnityEngine;
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

    [SerializeField] private float cameraMinX, cameraMinY, cameraMaxX, cameraMaxY;
    [SerializeField] private Canvas gameplayCanvas;
    
    [SerializeField] private GameObject quizItemPrefab;

    private float worldUnitsPerPixel;
    private string currentPrompt = null;

    //public properties

    public bool canTapQuizItems
    {
        // implement this
        get { return true; }
    }

    //methods
    #region public methods
    public void CameraPan(Vector2 pan)
    {
        Camera.main.transform.position += (Vector3)(pan * worldUnitsPerPixel);
        CameraWrap();
        // cameraInterrupt = true;
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
        quizItem.Initialize(triviaPair, Vector2.zero);
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

    #endregion


}
