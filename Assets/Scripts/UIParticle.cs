using UnityEngine;
using StageNine;

public class UIParticle : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 acceleration;
    private float age;
    private float duration;
    private float inverseDuration;
    private static readonly FloatCurveData sizeOverTime = new FloatCurveData(1, 0, 0, -2);

    public void OurInit(Vector3 newPos, Vector3 newVel, Vector3 newAcc, float newDur)
    {
        transform.localPosition = newPos;
        velocity = newVel;
        acceleration = newAcc;
        duration = newDur;
        inverseDuration = 1 / duration;
        age = 0f;
        gameObject.SetActive(true);
    }

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        age += Time.deltaTime;
        if(age >= duration)
        {
            gameObject.SetActive(false);
            UIParticleManager.singleton.ReturnParticle(this);
        }
        else
        {
            velocity += acceleration * Time.deltaTime;
            transform.localPosition += velocity * Time.deltaTime;
            transform.localScale = Vector3.one * sizeOverTime.GetFloatValue(age * inverseDuration);
        }
    }
}
