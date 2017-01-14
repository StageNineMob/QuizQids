using UnityEngine;
using System.Collections.Generic;
using System.IO;
using StageNine;

public class TriviaParser : MonoBehaviour {

    //enums

    //subclasses

    //consts and static data

    public static TriviaParser singleton;

    //public data

    //private data
    string categoryName;
    Dictionary<string, List<TriviaPair>> rightAnswers;
    List<TriviaPair> wrongAnswers;
    List<string> prompts;

    //public properties

    //methods
    #region public methods

        public void LoadTrivia(string filePath)
    {
        //confirm that filePath exists?
        StreamReader reader = File.OpenText(filePath);
        string line;
        rightAnswers = new Dictionary<string, List<TriviaPair>>();
        wrongAnswers = new List<TriviaPair>();
        prompts = new List<string>();
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
                if(rightAnswers.ContainsKey(pair.prompt))
                {
                    rightAnswers[pair.prompt].Add(pair);
                }
                else
                {
                    List<TriviaPair> triviaPairList = new List<TriviaPair>();
                    triviaPairList.Add(pair);
                    rightAnswers.Add(pair.prompt, triviaPairList);
                    prompts.Add(pair.prompt);
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
            wrongAnswers.Add(triviaPair);
        }
        Debug.Log("[TriviaParser:LoadTrivia] prompts: " + prompts[0] + " rightAnswers: " + rightAnswers[prompts[0]][0].value + " wrongAnswers: "
             + wrongAnswers[0].value);
        GameFieldManager.singleton.CreateQuizItem(wrongAnswers[0]);
    }

    #endregion

    #region private methods
    private void InitializeFields()
    {
        LoadTrivia("C:/Users/Starbuck/Desktop/state capitals and fakes.txt");
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