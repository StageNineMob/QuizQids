using UnityEngine;
using UnityEngine.EventSystems;

public class ConfirmationSound : MonoBehaviour, IPointerUpHandler
{
    private bool valueHasChanged = false;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume;

    public void SetValueHasChanged()
    {
        valueHasChanged = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(valueHasChanged)
        {
            GameFieldManager.singleton.PlaySound(clip, volume);
            valueHasChanged = false;
        }
    }

    // Use this for initialization
    void Start ()
    {
        valueHasChanged = false;
	}
}
