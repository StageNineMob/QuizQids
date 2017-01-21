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
    const float CORRECT_FADE_TIME = .4f;
    const float CORRECT_SCALE_AMOUNT = 1.3f;

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
                StartCoroutine(AnswerCorrectVanish());
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

    private IEnumerator AnswerCorrectVanish()
    {
        float time = 0f;
        transform.SetAsLastSibling();
        while (time < CORRECT_FADE_TIME)
        {
            time += Time.deltaTime;
            float alpha = (CORRECT_FADE_TIME - time) / CORRECT_FADE_TIME;
            float scale = Mathf.Lerp(1f, CORRECT_SCALE_AMOUNT, time / CORRECT_FADE_TIME);
            //MAYBEDO: Lighten colors as they fade out? 
            answerText.color = new Color(0f, 0f, 0f, alpha);
            GetComponent<Image>().color = new Color(0f, 1f, 0f, alpha);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        gameObject.SetActive(false);  // TODO return to object pool
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
