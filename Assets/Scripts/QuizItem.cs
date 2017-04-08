using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StageNine;

public class QuizItem : CameraDragger
{
    //enums
    private enum AnimationState
    {
        NORMAL = 0,
        CORRECT = 1,
        INCORRECT = 2,
    }

    //subclasses

    //consts and static data
    public const float CORRECT_FADE_TIME = .4f;
    public const float CORRECT_SCALE_AMOUNT = 1.3f;
    public const float INCORRECT_GRAVITY = 6500f;
    public const float DISTANCE_OFFSCREEN = 480;
    public const float INCORRECT_BUMP_VELOCITY = 1000f;
    public const float INCORRECT_ROTATION_SPEED = 18f;
    public const float INCORRECT_DARKEN_RATE = 1f;
    const float NUDGING_ACCEL = 1000f;
    const float NUDGING_CURVE_POWER = 0.3f;
    const float SPEED_DAMPENING_THRESHOLD = 350f;
    const float SPEED_DAMPENING_RATE = 0.5f;
    const float SPEED_BOOSTING_THRESHOLD = 100f;
    const float SPEED_BOOSTING_RATE = 0.8f;

    //public data
    public TriviaPair data;

    //private data
    [SerializeField] private Text answerText;
    private Vector2 linearVelocity;
    private bool overlapNudging;
    private bool speedDampening;
    private bool speedBoosting;

    private AnimationState animationState;
    float time;
    float twistRate;

    //public properties

    //methods
    #region public methods

    public override void GetTap()
    {
        var image = GetComponent<Image>();
        if(GameFieldManager.singleton.canTapQuizItems)
        {
            overlapNudging = false;
            speedDampening = false;
            speedBoosting = false;
            GameFieldManager.singleton.RemoveQuizItem(this);
            if(GameFieldManager.singleton.QuizCorrectAnswer(data))
            {
                image.color = Color.green;
                image.raycastTarget = false;
                AnswerCorrectVanishBegin();
                GameFieldManager.singleton.DisplayStreakCounter(Camera.main.WorldToScreenPoint(transform.position));
            }
            else
            {
                image.color = Color.red;
                image.raycastTarget = false;
                AnswerIncorrectVanishBegin();
            }
        }
    }

    public void Initialize(TriviaPair newData, Vector2 position, Vector2 velocity)
    {
        var image = GetComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = true;

        overlapNudging = true;
        speedDampening = true;
        speedBoosting = true;

        answerText.text = newData.value;
        answerText.color = Color.black;
        data = newData;
        linearVelocity = velocity;
        transform.localPosition = position;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        animationState = AnimationState.NORMAL;

        gameObject.SetActive(true);
    }

    #endregion

    #region private methods
    private void AnswerCorrectVanishBegin()
    {
        animationState = AnimationState.CORRECT;
        time = 0f;
        transform.SetAsLastSibling();
    }

    private void AnswerCorrectVanishUpdate()
    {
        if(time < CORRECT_FADE_TIME)
        {
            time += Time.deltaTime;
            float alpha = (CORRECT_FADE_TIME - time) / CORRECT_FADE_TIME;
            float scale = Mathf.Lerp(1f, CORRECT_SCALE_AMOUNT, time / CORRECT_FADE_TIME);
            //MAYBEDO: Lighten colors as they fade out? 
            answerText.color = new Color(0f, 0f, 0f, alpha);
            GetComponent<Image>().color = new Color(0f, 1f, 0f, alpha);
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            gameObject.SetActive(false);
            GameFieldManager.singleton.ReturnQuizItemToPool(this);
        }
    }

    private void AnswerIncorrectVanishBegin()
    {
        animationState = AnimationState.INCORRECT;
        time = 0f;
        transform.SetAsLastSibling();
        float randomAngle = Random.Range(-1f, 1f);
        linearVelocity += (Vector2.left * Mathf.Sin(randomAngle) + Vector2.up * Mathf.Cos(randomAngle)) * INCORRECT_BUMP_VELOCITY;
        twistRate = INCORRECT_ROTATION_SPEED;
        if (Random.Range(0, 2) == 0)
        {
            twistRate = -INCORRECT_ROTATION_SPEED;
        }
    }

    private void AnswerIncorrectVanishUpdate()
    {
        if(transform.position.y < Camera.main.transform.position.y + DISTANCE_OFFSCREEN)
        {

            time += Time.deltaTime;
            linearVelocity += Vector2.down * INCORRECT_GRAVITY * Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0, 0, twistRate * time);
            GetComponent<Image>().color = new Color(1f - time * INCORRECT_DARKEN_RATE, 0f, 0f);
        }
        else
        {
            gameObject.SetActive(false);
            GameFieldManager.singleton.ReturnQuizItemToPool(this);
        }
    }

    private bool IsColliding(RectTransform otherRect)
    {
        var ourRect = GetComponent<RectTransform>();
        var ourPos = ourRect.localPosition;
        var otherPos = otherRect.localPosition;
        var halfWidths = (ourRect.rect.width + otherRect.rect.width) * 0.5;
        var halfHeights = (ourRect.rect.height + otherRect.rect.height) * 0.5;
        return (Mathf.Abs(otherPos.x - ourPos.x) < halfWidths) && (Mathf.Abs(otherPos.y - ourPos.y) < halfHeights);
    }

    private float GetOverlapArea(RectTransform otherRect)
    {
        var ourRect = GetComponent<RectTransform>();
        var ourHalfWidth = ourRect.rect.width * 0.5f;
        var ourHalfHeight = ourRect.rect.height * 0.5f;
        var otherHalfWidth = otherRect.rect.width * 0.5f;
        var otherHalfHeight = otherRect.rect.height * 0.5f;
        return (Mathf.Min(ourRect.localPosition.x + ourHalfWidth, otherRect.localPosition.x + otherHalfWidth) - 
            Mathf.Max(ourRect.localPosition.x - ourHalfWidth, otherRect.localPosition.x - otherHalfWidth)) * 
            (Mathf.Min(ourRect.localPosition.y + ourHalfHeight, otherRect.localPosition.y + otherHalfHeight) - 
            Mathf.Max(ourRect.localPosition.y - ourHalfHeight, otherRect.localPosition.y - otherHalfHeight));
    }

    private float GetMaximumOverlapArea(Rect otherRect)
    {
        var ourRect = GetComponent<RectTransform>().rect;
        return Mathf.Min(ourRect.width, otherRect.width) *
            Mathf.Min(ourRect.height, otherRect.height);
    }
    #endregion

    #region monobehaviors
    void Update()
    {
        switch(animationState)
        {
            case AnimationState.CORRECT:
                AnswerCorrectVanishUpdate();
                break;
            case AnimationState.INCORRECT:
                AnswerIncorrectVanishUpdate();
                break;
        }
        transform.localPosition = GameFieldManager.singleton.ScreenWrapInBounds((Vector2)transform.localPosition + (linearVelocity * Time.deltaTime));
        if (overlapNudging && GameFieldManager.singleton.simulateCollisions)
        {
            // collision testing
#if DEBUG_DRAW
            GetComponent<Image>().color = Color.white;
#endif
            foreach(var item in GameFieldManager.singleton.activeQuizItems)
            {
                if(item != this)
                {
                    var otherRect = item.GetComponent<RectTransform>();
                    if (IsColliding(otherRect))
                    {
#if DEBUG_DRAW
                        // collision testing
                        GetComponent<Image>().color = Color.yellow;
#endif
                        var offsetVector = (Vector2)(transform.localPosition - item.transform.localPosition);
                        var magnitude = Mathf.Pow(GetOverlapArea(otherRect) / GetMaximumOverlapArea(otherRect.rect),NUDGING_CURVE_POWER);
                        linearVelocity += offsetVector.normalized * magnitude * Time.deltaTime * NUDGING_ACCEL * GameFieldManager.singleton.collisionBumpScale;
                    }
                }
            }
        }
        if(speedDampening)
        {
            if (linearVelocity.magnitude > SPEED_DAMPENING_THRESHOLD)
            {
                float overflow = linearVelocity.magnitude - SPEED_DAMPENING_THRESHOLD;
                linearVelocity *= (linearVelocity.magnitude - overflow * SPEED_DAMPENING_RATE * Time.deltaTime ) / linearVelocity.magnitude;
            }
        }
        if (speedBoosting)
        {
            if (linearVelocity.magnitude < SPEED_BOOSTING_THRESHOLD)
            {
                float overflow = linearVelocity.magnitude - SPEED_BOOSTING_THRESHOLD;
                linearVelocity *= (linearVelocity.magnitude - overflow * SPEED_BOOSTING_RATE * Time.deltaTime) / linearVelocity.magnitude;
            }
        }
    }
    #endregion
}
