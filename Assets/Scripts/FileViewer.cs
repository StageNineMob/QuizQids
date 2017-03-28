using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class FileViewer : MonoBehaviour
{
    //enums

    //subclasses

    //consts and static data
    const float SCROLLBAR_TOP = 1f;
    const int NO_ELEMENT = -1;
    const float SCROLL_FACTOR = 1000f;

    //public data
    public int currentCategoryNumber;
    //private data
    private List<FileViewListElement> elements;
    private string _directory, _extension;
    [SerializeField]
    private GameObject listElementPrefab;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private InputField _inputField;
    private float _scrollAreaMinHeight;
    private int _currentlySelectedElement = NO_ELEMENT;
    [SerializeField] private Image metadataThumbnail;
    [SerializeField] private Text metadataText;

    private UnityAction _doubleClickCallback;
    private bool shouldReset = false;
    private bool shouldResize = false;

    //public properties
    public UnityAction doubleClickCallback
    {
        set
        {
            _doubleClickCallback = value;
        }
    }

    //methods
    #region public methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extension"></param>
    public void PopulateList(string directory, string extension, bool displayCategories = true)
    {
        _directory = directory;
        _extension = extension;

        FileManager.singleton.EnsureDirectoryExists(directory);
        List<string> fileNames = FileManager.singleton.FilenamesInDirectory(directory);
        foreach (string fileName in fileNames)
        {
            if (FileManager.FileExtensionIs(fileName, extension))
            {
                if (displayCategories)
                {
                    TriviaParser.singleton.LoadTriviaMetadata(directory + fileName, false);
                    for(int ii = 0; ii < TriviaParser.singleton.categoryList.Count; ++ii)
                    {
                        FileViewListElement element = Instantiate(listElementPrefab).GetComponent<FileViewListElement>();
                        if (element == null)
                        {
                            Debug.LogError("wut");
                        }
                        element.fileName = fileName;
                        element.categoryName = TriviaParser.singleton.categoryList[ii].Key;
                        element.categoryNumber = ii;
                        element.SetDisplayName(element.categoryName);
                        element.transform.SetParent(transform);
                        element.transform.localScale = Vector3.one;

                        elements.Add(element);
                    }
                }
                else
                {
                    FileViewListElement element = Instantiate(listElementPrefab).GetComponent<FileViewListElement>();
                    if (element == null)
                    {
                        Debug.LogError("wut");
                    }
                    element.fileName = fileName;
                    // MAYBEDO: set dummy values for element categoryname/number?
                    element.SetDisplayName(fileName);
                    element.transform.SetParent(transform);
                    element.transform.localScale = Vector3.one;

                    elements.Add(element);
                }
            }
        }
        shouldResize = true;
    }

    public void NextInList()
    {
        if (_currentlySelectedElement != NO_ELEMENT)
        {
            elements[(_currentlySelectedElement + 1) % elements.Count].ActivateSelf();
        }
    }

    public void PreviousInList()
    {
        if (_currentlySelectedElement != NO_ELEMENT)
        {
            elements[(_currentlySelectedElement + elements.Count - 1) % elements.Count].ActivateSelf();
        }
    }


    public void Scroll(float axisValue)
    {
        if(GetComponent<RectTransform>().rect.height > _scrollAreaMinHeight)
        {
            scrollbar.value += axisValue * SCROLL_FACTOR / (GetComponent<RectTransform>().rect.height - _scrollAreaMinHeight);
            if(scrollbar.value > 1f)
            {
                scrollbar.value = 1f;
            }
            if (scrollbar.value < 0f)
            {
                scrollbar.value = 0f;
            }
        }
    }

    public void ClearList()
    {
        foreach(var element in elements)
        {
            Destroy(element.gameObject);
        }
        elements.Clear();
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, _scrollAreaMinHeight);
        transform.localPosition = Vector3.zero;
    }

    public void SetInputField(string inputText)
    {
        _inputField.text = inputText;
        // TODO: 
        //      :| Figure out a way to highlight the name in the text field without...
        //      B| ...brakinstuff
        _inputField.MoveTextStart(false);
        _inputField.MoveTextEnd(true);
    }

    public void RespondToDoubleClick(string fileName)
    {
        if (_inputField.text == fileName)
        {
            if(_doubleClickCallback != null)
            {
                _doubleClickCallback();
            }
        }
        else
        {
            SetInputField(fileName);
        }
    }

    public void HighlightDisplayName(string displayName)
    {
        //ClearMetadata();
#if  FILESYSTEM_CASE_INSENSITIVE
        fileName = fileName.ToLower();
#endif
        if (_currentlySelectedElement != NO_ELEMENT)
        {
            elements[_currentlySelectedElement].highlight = false;
            UIMenuManager.singleton.DisableGameModeButtons();
        }
        _currentlySelectedElement = NO_ELEMENT;
        currentCategoryNumber = NO_ELEMENT;

        //if (!FileManager.FileExtensionIs(displayName, _extension))
        //    displayName += _extension;
        for (int ii = 0; ii < elements.Count; ii++)
        {
            if (elements[ii].categoryName == displayName)
            {
                _currentlySelectedElement = ii;
                elements[ii].highlight = true;
                JumpToElementIndex(ii);
                currentCategoryNumber = elements[ii].categoryNumber;
                ParseMetadata(elements[ii].fileName);
                return;
            }
        }
    }

    public void SelectRandomElement()
    {
        int randomIndex = Random.Range(0, elements.Count);
        SetInputField(elements[randomIndex].categoryName);
    }

    public void JumpToElementIndex(int index)
    {
        var elementYPos = transform.localPosition.y + elements[index].transform.localPosition.y;
        var elementHeight = elements[index].GetComponent<RectTransform>().rect.height;
        var limit = (_scrollAreaMinHeight - elementHeight) * .5f;
        if (elementYPos > limit)
        {
            transform.localPosition += new Vector3(0, limit - elementYPos);
        }
        else if (elementYPos < -limit)
        {
            transform.localPosition += new Vector3(0, -limit - elementYPos);
        }
    }

    public void ResetFileViewer()
    {
        shouldReset = true;
    }

    //public void ClearMetadata()
    //{
    //    metadataText.text = "";
    //    metadataThumbnail.color = Color.clear;
    //    //TODO: replace the sprite with our "IMAGE NOT FOUND" texture;
    //    metadataThumbnail.sprite = null;
    //}

    #endregion

    #region private methods

    //private void DecodeMetadata(string filename)
    //{
    //    string textToPrint = "";
    //    switch(_extension)
    //    {
    //        case MapManager.PICTURE_FILE_EXTENSION:
    //            {
    //                metadataThumbnail.color = Color.white;

    //                if (MapManager.singleton.LoadMap(filename, MapManager.PREVIEW_MODE))
    //                {
    //                    //textToPrint += MapManager.singleton.previewMapName + Environment.NewLine;
    //                    //textToPrint += Environment.NewLine + "Point Limit: " + MapManager.singleton.previewArmyPointLimit;
    //                    metadataText.text = textToPrint;

    //                    var previewTexture = MapManager.singleton.MapPreview(MapManager.PREVIEW_MODE);
    //                    metadataThumbnail.sprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), Vector2.one * 0.5f);
    //                }
    //                else
    //                {
    //                    // MAYBEDO: blow up?
    //                    Debug.LogError("[FileViewer:DecodeMetadata] Failed to load map");
    //                }
    //                break;
    //            }
    //        default:
    //            // MAYBEDO: blow up?
    //            break;
    //    }
    //}

    private static string FormatFloatTime(float floatTime)
    {
        int seconds = Mathf.FloorToInt(floatTime % 60);
        int minutes = Mathf.FloorToInt(floatTime / 60);
        string minutesSeconds = minutes.ToString() + ":" + seconds.ToString("00");
        return minutesSeconds;
    }

    private void ParseMetadata(string fileName)
    {
        TriviaParser.singleton.LoadTriviaMetadata(_directory + fileName, false);
        UIMenuManager.singleton.EnableGameModeButtons();
    }

    private void ResizeFileViewer()
    {
        // Resize scroll area to fit text and move to top of list
        float listHeight = 0f;
        foreach (var element in elements)
        {
            listHeight += element.GetComponent<RectTransform>().rect.height;
            Debug.Log("Height: " + listHeight);
        }
        if (listHeight > _scrollAreaMinHeight)
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, listHeight);
            scrollbar.value = SCROLLBAR_TOP;
            transform.localPosition = new Vector3(0, (GetComponent<RectTransform>().rect.height - _scrollAreaMinHeight) * -0.5f);
        }
    }

    #endregion

    #region monobehaviors

    // Use this for initialization
    void Awake()
    {
        elements = new List<FileViewListElement>();
        _scrollAreaMinHeight = GetComponent<RectTransform>().rect.height;
        shouldReset = false;
    }

    void Update()
    {
        if (shouldResize)
        {
            ResizeFileViewer();
            shouldResize = false;
        }
        if (shouldReset)
        {
            HighlightDisplayName(_inputField.text);
            shouldReset = false;
        }
    }
    #endregion
}
