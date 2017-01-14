using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public abstract class CameraDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    //enums

    //subclasses

    public class TouchData
    {
        public int pointerId;
        public Vector2 screenPos;
        public bool isDragging;

        public TouchData(int newId, Vector2 newPos, bool newDrag = false)
        {
            pointerId = newId;
            screenPos = newPos;
            isDragging = newDrag;
        }
    }

    //consts and static data

    private const int DRAG_THRESHOLD =
#if UNITY_IOS || UNITY_ANDROID
        10;
#else
        5;
#endif

    //public data

    //private data

    private Dictionary<int, TouchData> touches;

    //public properties

    //methods
    #region public methods

    public void OnBeginDrag(PointerEventData eventData)
    {
        touches[eventData.pointerId].isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var touch = touches[eventData.pointerId];
        GameFieldManager.singleton.CameraPan(touch.screenPos - eventData.position);
        touch.screenPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // UNUSED
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        int newId = eventData.pointerId;
        Vector2 newPos = eventData.position;
        touches.Add(newId, new TouchData(newId, newPos));
        Debug.Log("[CameraDragger:OnPointerDown] You clicked something, PointerID: " + newId + ", Position: " + newPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var touch = touches[eventData.pointerId];

        if (!touch.isDragging)
        {
            GetTap();
        }
        touches.Remove(eventData.pointerId);
    }

    public abstract void GetTap();

    #endregion

    #region private methods

    #endregion

    #region monobehaviors

    // Use this for initialization
    void Start ()
    {
        touches = new Dictionary<int, TouchData>();
	}

    #endregion
}
