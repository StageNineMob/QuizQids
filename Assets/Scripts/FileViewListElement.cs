using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FileViewListElement : MonoBehaviour {
    //enums

    //subclasses

    //consts and static data
    public const float MAXIMUM_TIME_DOUBLE_CLICK_DELAY = 0.35f;
    //public data
    public string fileName;
    public string categoryName;
    public int categoryNumber;
    //private data
    private float lastClick = 0f;

    [SerializeField]
    private Text displayText;
    //public properties
    public bool highlight
    {
        set
        {
            if (value)
            {
                GetComponent<Image>().color = Color.blue;
                displayText.color = Color.white;
            }
            else
            {
                GetComponent<Image>().color = Color.white;
                displayText.color = Color.black;
            }
        }
    }

    public string filename
    {
        get
        {
#if FILESYSTEM_CASE_INSENSITIVE
            return filenameText.text.ToLower();
#else
            return displayText.text;
#endif
        }
    }

    //methods
#region public methods

    /// <summary>
    /// Set the text to display.
    /// </summary>
    /// <param name="newName">filename provided with file type extension.</param>
    public void SetDisplayName(string newName)
    {
        displayText.text = newName;
    }

    public void RespondToClick()
    {
        //Case: Double-click
        if (Time.time < (lastClick + MAXIMUM_TIME_DOUBLE_CLICK_DELAY))
        {
            transform.parent.GetComponent<FileViewer>().RespondToDoubleClick(displayText.text);
        }
        else
        {
            ActivateSelf();
        }
        lastClick = Time.time;
    }

    public void ActivateSelf()
    {
        transform.parent.GetComponent<FileViewer>().SetInputField(displayText.text);
    }

#endregion

#region private methods

#endregion

#region monobehaviors


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

#endregion
}
