using UnityEngine;
using System.Collections.Generic;

public class UIParticleManager : MonoBehaviour
{
    public static UIParticleManager singleton;

    private Stack<UIParticle> inactiveParticles;
    [SerializeField] private GameObject particlePrefab;

    public void MakeParticles(Transform particleParent, Vector3 initialPosition, Vector3 initialPositionVariation, Vector3 initialVelocity, Vector3 initialVelocityVariation, Vector3 acceleration, float initialDuration, float initialDurationVariation, int count)
    {
        for(int ii = 0; ii < count; ii++)
        {
            UIParticle particle = GetParticle();
            particle.transform.SetParent(particleParent);
            Vector3 position = initialPosition + new Vector3(initialPositionVariation.x * Random.Range(-1f, 1f), initialPositionVariation.y * Random.Range(-1f, 1f), initialPositionVariation.z * Random.Range(-1f, 1f));
            Vector3 velocity = initialVelocity + new Vector3(initialVelocityVariation.x * Random.Range(-1f, 1f), initialVelocityVariation.y * Random.Range(-1f, 1f), initialVelocityVariation.z * Random.Range(-1f, 1f));
            float duration = initialDuration + initialDurationVariation * Random.Range(-1f, 1f);
            particle.OurInit(position, velocity, acceleration, duration);
        }
    }

    public UIParticle GetParticle()
    {
        if(inactiveParticles.Count < 1)
        {
            GameObject newParticle = Instantiate(particlePrefab) as GameObject;
            return newParticle.GetComponent<UIParticle>();
        }
        else
        {
            return inactiveParticles.Pop();
        }
    }

    public void ReturnParticle(UIParticle particle)
    {
        inactiveParticles.Push(particle);
    }

	// Use this for initialization
	void Start ()
    {
	
	}
	

	void Awake () {
        Debug.Log("[UIParticleManager:Awake]");
        if (singleton == null)
        {
            Debug.Log("Hello, world!");
            singleton = this;
            inactiveParticles = new Stack<UIParticle>();
        }
        else
        {
            Debug.Log("Goodbye, cruel world!");
            GameObject.Destroy(gameObject);
        }
    }
}
