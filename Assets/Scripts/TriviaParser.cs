using UnityEngine;
using System.Collections.Generic;
using System.IO;
using StageNine;
using Random = UnityEngine.Random;

public class TriviaParser : MonoBehaviour {

    //enums

    //subclasses

    //consts and static data

    public static TriviaParser singleton;

    //public data

    //private data
    string categoryName;
    private Dictionary<string, List<TriviaPair>> _rightAnswers;
    private List<TriviaPair> _wrongAnswers;
    private List<string> _prompts;

    //public properties
    public Dictionary<string, List<TriviaPair>> rightAnswers
    {
        get { return _rightAnswers; }
    }

    public List<TriviaPair> wrongAnswers
    {
        get { return _wrongAnswers; }
    }

    public List<string> prompts
    {
        get { return _prompts; }
    }

    //methods
    #region public methods

    public void LoadTrivia(string filePath)
    {
        //confirm that filePath exists?
        StreamReader reader = File.OpenText(filePath);
        string line;
        _rightAnswers = new Dictionary<string, List<TriviaPair>>();
        _wrongAnswers = new List<TriviaPair>();
        _prompts = new List<string>();
        while ((line = reader.ReadLine()) != null && line != "")
        {
            categoryName = line;
        }
        while ((line = reader.ReadLine()) != null && line != "")
        {
            string[] items = line.Split(';');
            if(items.Length >= 2)
            {
                TriviaPair pair = new TriviaPair();
                pair.value = items[0];
                pair.prompt = items[1];
                if(_rightAnswers.ContainsKey(pair.prompt))
                {
                    _rightAnswers[pair.prompt].Add(pair);
                }
                else
                {
                    List<TriviaPair> triviaPairList = new List<TriviaPair>();
                    triviaPairList.Add(pair);
                    _rightAnswers.Add(pair.prompt, triviaPairList);
                    _prompts.Add(pair.prompt);
                }
            }
            else
            {
                // wig out?
            }
        }
        while ((line = reader.ReadLine()) != null && line != "")
        {
            TriviaPair triviaPair = new TriviaPair();
            triviaPair.prompt = null;
            triviaPair.value = line;
            _wrongAnswers.Add(triviaPair);
        }
    }

    public void RandomizeAnswerLists()
    {
        Shuffle(_wrongAnswers);
        Shuffle(_prompts);
        foreach(var prompt in _prompts)
        {
            Shuffle(_rightAnswers[prompt]);
        }
    }


    #endregion

    #region private methods
    private void InitializeFields()
    {
        LoadTrivia("C:/Users/Starbuck/Desktop/state capitals and fakes.txt");
    }

    private void Shuffle<T>(IList<T> list)
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