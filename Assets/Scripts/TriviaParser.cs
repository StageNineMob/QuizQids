using UnityEngine;
using System.Collections.Generic;
using System.IO;
using StageNine;
using Random = UnityEngine.Random;
using System.Xml;


public class TriviaParser : MonoBehaviour {

    //enums
    public enum TriviaMode
    {
        NONE = 0,
        GENERAL = 1,
        SPECIFIC = 2,
        MULTIPLE_CHOICE = 4,
    };
    //subclasses

    //consts and static data
    public const string GENERAL_MODE_NAME = "General";
    public const string SPECIFIC_MODE_NAME = "Specific";
    public const string MULTIPLE_CHOICE_MODE_NAME = "Multiple Choice";

    public const string TRIVIA_DIRECTORY = "/Trivia/";
    public const string TRIVIA_FILE_EXT = ".xml";

    public static TriviaParser singleton;

    //public data
    public List<KeyValuePair<string, TriviaMode>> categoryList;

    //private data
    string _categoryName;
    private TriviaMode _triviaMode;
    //private Dictionary<string, List<TriviaPair>> _rightAnswers;
    private List<TriviaPair> _rightAnswers;
    private List<TriviaPair> _wrongAnswers;
    private List<string> _prompts;
    private XmlDocument triviaDocument;

    //public properties
    public string categoryName
    {
        get { return _categoryName; }
    }

    //public Dictionary<string, List<TriviaPair>> rightAnswers
    public List<TriviaPair> rightAnswers
    {
        get { return _rightAnswers; }
    }

    public List<TriviaPair> wrongAnswers
    {
        get { return _wrongAnswers; }
    }

    public TriviaMode triviaMode
    {
        get { return _triviaMode; }
    }

    public List<string> prompts
    {
        get { return _prompts; }
    }

    //methods
    #region public methods
    // this should go somewhere better!
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = Random.Range(0, n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //public XElement GetWrongAnswers()
    //{
    //    from answer in _wrongAnswers
    //    let fields = str
    //    select new XElement("Answer",
    //        new XAttribute("name", fields),
    //        new XElement("Category",
    //            new XAttribute("name", lines1[0])
    //        )
    //    )

    //    //XElement wrongAnswers = new XElement("AllAnswers",);
    //    return null;
    //}

    public void LoadTrivia(int categoryNumber, TriviaMode mode)
    {
        //File.WriteAllText(@"C:\Users\Starbuck\Desktop\Robot Masters.xml", XmlGenerator.ParseLineSeparatedToXML("Trivia/robot masters"));

        //confirm that filePath exists?
        
        _triviaMode = mode;
        _rightAnswers = new List<TriviaPair>();
        _wrongAnswers = new List<TriviaPair>();
        if (mode == TriviaMode.MULTIPLE_CHOICE)
        {
            _prompts = new List<string>();
        }
             
        _categoryName = categoryList[categoryNumber].Key;

        var nodeList = triviaDocument.SelectNodes("/Root/AllAnswers/Answer");
        foreach(XmlElement node in nodeList)
        {
            var categoryNodes = node.SelectNodes("Category");
            foreach(XmlElement categoryNode in categoryNodes)
            {
                //if this answer is supposed to appear for this category...
                if (categoryNode.Attributes["Name"].Value == _categoryName)
                {
                    TriviaPair tp = new TriviaPair();
                    tp.value = node.Attributes["Name"].Value;

                    //get all the corresponding prompts...
                    var promptNodes = categoryNode.SelectNodes("Prompt");
                    foreach(XmlElement promptNode in promptNodes)
                    {
                        tp.prompts.Add(promptNode.Attributes["Name"].Value);

                        //if we're in specific mode, add prompts as we find them
                        //if (mode == TriviaMode.SPECIFIC && promptNode.Attributes["Name"].Value != "null")
                        //{
                        //    if(!_prompts.Contains(promptNode.Attributes["Name"].Value))
                        //    {
                        //        _prompts.Add(promptNode.Attributes["Name"].Value);
                        //    }
                        //}
                    }
                    //pick our prompt from something on screen
                    // if there is at least one prompt for which this is correct
                    // it is a right answer
                    if(tp.prompts.Count > 0)
                    {
                        // TODO: Add support for SPECIFIC mode and for
                        // one answer being correct for multiple prompts
                        //if (mode == TriviaMode.GENERAL)
                        //{
                        //    if (!_rightAnswers.ContainsKey("null"))
                        //    {
                        //        _rightAnswers.Add("null", new List<TriviaPair>());
                        //    }
                        //    _rightAnswers["null"].Add(tp);
                        //}
                        if(mode == TriviaMode.MULTIPLE_CHOICE)
                        {
                            foreach(var prompt in tp.prompts)
                            {
                                if (!_prompts.Contains(prompt))
                                {
                                    _prompts.Add(prompt);
                                }
                            }
                        }
                        _rightAnswers.Add(tp);
                    }
                    else
                    {
                        _wrongAnswers.Add(tp);
                    }
                }
            }
        }
    }

    //public void OldLoadTrivia(string filePath)
    //{
    //    System.IO.File.WriteAllText(@"C:\Users\Starbuck\Desktop\US Cities.xml", XmlGenerator.ParseLineSeparatedToXML(filePath));

    //    //confirm that filePath exists?
    //    string text = "";
    //    try
    //    {
    //        TextAsset tAsset = Resources.Load(filePath) as TextAsset;
    //        text = tAsset.text;
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError("[TriviaParser:LoadTrivia] R");
    //    }
    //    text = text.Replace("\r", "");
    //    var lines = text.Split('\n');
    //    int lineNum = 0;

    //    _rightAnswers = new Dictionary<string, List<TriviaPair>>();
    //    _wrongAnswers = new List<TriviaPair>();
    //    _prompts = new List<string>();
    //    for(; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
    //    {
    //        _categoryName = lines[lineNum];
    //    }
    //    for (++lineNum; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
    //    {
    //        string[] items = lines[lineNum].Split(';');
    //        if (items.Length >= 2)
    //        {
    //            TriviaPair pair = new TriviaPair();
    //            pair.value = items[0];
    //            pair.prompt = items[1];
    //            if (_rightAnswers.ContainsKey(pair.prompt))
    //            {
    //                _rightAnswers[pair.prompt].Add(pair);
    //            }
    //            else
    //            {
    //                List<TriviaPair> triviaPairList = new List<TriviaPair>();
    //                triviaPairList.Add(pair);
    //                _rightAnswers.Add(pair.prompt, triviaPairList);
    //                _prompts.Add(pair.prompt);
    //            }
    //        }
    //        else
    //        {
    //            // wig out?
    //        }
    //    }
    //    for (++lineNum; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
    //    {
    //        TriviaPair triviaPair = new TriviaPair();
    //        triviaPair.prompt = null;
    //        triviaPair.value = lines[lineNum];
    //        _wrongAnswers.Add(triviaPair);
    //    }
    //}

    public void LoadTriviaMetadata(string filePath, bool resources = true)
    {
        string text = "";

        Debug.Log("[TriviaParser:LoadTriviaMetadata] Application.PersistentDataPath " + Application.persistentDataPath);

        try
        {
            if (resources)
            {
                TextAsset tAsset = Resources.Load(filePath) as TextAsset;
                text = tAsset.text;
            }
            else
            {
                text = File.ReadAllText(Application.persistentDataPath + filePath);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[TriviaParser:LoadTrivia] FileReadError: " + ex.Message);
            return;
        }

        triviaDocument = new XmlDocument();
        triviaDocument.LoadXml(text);

        var nodeList = triviaDocument.SelectNodes("/Root/Categories/Category");
        categoryList = new List<KeyValuePair<string, TriviaMode>>();
        foreach (XmlElement node in nodeList)
        {
            TriviaMode mode = TriviaMode.NONE;
            var modeNodes = node.SelectNodes("TriviaMode");
            foreach (XmlElement modeNode in modeNodes)
            {
                if (modeNode.Attributes["Name"].Value == GENERAL_MODE_NAME)
                {
                    mode |= TriviaMode.GENERAL;
                }
                if (modeNode.Attributes["Name"].Value == SPECIFIC_MODE_NAME)
                {
                    mode |= TriviaMode.SPECIFIC;
                }
                if (modeNode.Attributes["Name"].Value == MULTIPLE_CHOICE_MODE_NAME)
                {
                    mode |= TriviaMode.MULTIPLE_CHOICE;
                }
            }
            // TODO: Add support for selecting triviamodes and categories separately
            _triviaMode = mode;
            categoryList.Add(new KeyValuePair<string, TriviaMode>(node.Attributes["Name"].Value, mode));
        }
    }

    public void RandomizeAnswerLists()
    {
        Shuffle(_wrongAnswers);
        Shuffle(_rightAnswers);
    }

    public void RandomizePrompts()
    {
        Shuffle(_prompts);
    }

    #endregion

    #region private methods
    private void InitializeFields()
    {
        //LoadTriviaMetadata("/Trivia/Robot Masters.xml", false);
        //LoadTrivia(0, TriviaMode.MULTIPLE_CHOICE);
    }
    #endregion

    // Use this for initialization
    #region monobehaviors
    void Awake()
    {
        Debug.Log("[TriviaParser:Awake]");
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