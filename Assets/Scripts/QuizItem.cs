using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StageNine;

public class QuizItem : CameraDragger
{
    //enums

    //subclasses

    //consts and static data
    const float widthFactor = 0.6f, heightFactor = 1.1f;

    //public data
    
    //private data
    TriviaPair data;
    [SerializeField] Text answerText;
    Vector2 linearVelocity;
    //public properties

    //methods
    #region public methods

    public override void GetTap()
    {
        var image = GetComponent<Image>();
        if(GameFieldManager.singleton.canTapQuizItems)
        {
            if(GameFieldManager.singleton.QuizCorrectAnswer(data))
            {
                image.color = Color.green;
                image.raycastTarget = false;
                GameFieldManager.singleton.rightAnswers++;
                // TODO: begin correct answer animation
            }
            else
            {
                image.color = Color.red;
                image.raycastTarget = false;
                GameFieldManager.singleton.wrongAnswers++;
                // TODO: begin wrong answer animation
            }
        }
    }

    public void Initialize(TriviaPair newData, Vector2 position, Vector2 velocity)
    {
        var image = GetComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = true;

        answerText.text = newData.value;
        data = newData;
        linearVelocity = velocity;
        transform.localPosition = position;
        GetComponent<RectTransform>().sizeDelta = new Vector2(answerText.fontSize * newData.value.Length * widthFactor, answerText.fontSize * heightFactor);
    }

    #endregion

    #region private methods

    #endregion

    #region monobehaviors
    void Update()
    {
        transform.localPosition = GameFieldManager.singleton.ScreenWrapInBounds((Vector2)transform.localPosition + (linearVelocity * Time.deltaTime));
    }
    #endregion

}
