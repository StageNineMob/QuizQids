using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;


namespace StageNine
{
    public class XmlGenerator
    {
        private class SongInfo
        {
            public HashSet<string> decades;
            public HashSet<string> artists;
            public SongInfo(string decade, string artist)
            {
                decades = new HashSet<string>();
                artists = new HashSet<string>();
                Add(decade, artist);
            }

            public void Add(string decade, string artist)
            {
                decades.Add(decade);
                artists.Add(artist);
            }
        }
        public static XmlElement GenerateAnswer(XmlDocument document, string name, string[] categories, string[][] prompts)
        {
            var answer = document.CreateElement("Answer");
            var nameAttribute = document.CreateAttribute("Name");
            nameAttribute.Value = name;

            answer.Attributes.Append(nameAttribute);

            //for loop for categories
            for(int category = 0; category < categories.Length; ++category)
            {
                var categoryElement = document.CreateElement("Category");
                nameAttribute = document.CreateAttribute("Name");
                nameAttribute.Value = categories[category];
                categoryElement.Attributes.Append(nameAttribute);

                for(int prompt = 0; prompt < prompts[category].Length; ++prompt)
                {
                    var promptElement = document.CreateElement("Prompt");
                    nameAttribute = document.CreateAttribute("Name");
                    nameAttribute.Value = prompts[category][prompt];
                    promptElement.Attributes.Append(nameAttribute);
                    categoryElement.AppendChild(promptElement);
                }
                answer.AppendChild(categoryElement);
            }
            return answer;
        }

        public static string ParseLineSeparatedToXML(string filePath)
        {
            string text = "";
            try
            {
                TextAsset tAsset = Resources.Load(filePath) as TextAsset;
                text = tAsset.text;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[XmlGenerator:ParseLineSeparatedToXML] File Read Error");
                return null;
            }
            text = text.Replace("\r", "");
            var lines = text.Split('\n');
            int lineNum = 0;
            
            XmlDocument document = new XmlDocument();
            List<string> categoryNames = new List<string>();
            var rootElement = document.CreateElement("Root");
            var categoriesElement = document.CreateElement("Categories");
            for (; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
            {
                var categoryElement = document.CreateElement("Category");
                var nameAttribute = document.CreateAttribute("Name");
                nameAttribute.Value = lines[lineNum];
                categoryNames.Add(lines[lineNum]);
                categoryElement.Attributes.Append(nameAttribute);
                // TODO: modes should be attached at this step
                categoriesElement.AppendChild(categoryElement);
            }
            rootElement.AppendChild(categoriesElement);
            string[] categoriesArray = categoryNames.ToArray();
            var allAnswers = document.CreateElement("AllAnswers");

            for (++lineNum; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
            {
                string[] items = lines[lineNum].Split(';');
                // TODO: handle multiple categories here
                if (items.Length >= 2)
                {
                    string name = items[0];
                    List<string> prompts = new List<string>();
                    for(int prompt = 1; prompt < items.Length; ++prompt)
                    {
                        prompts.Add(items[prompt]);
                    }
                    if (items.Length > 1)
                    {
                        prompts.Add("null");
                    }
                    string[][] promptsArray = new string[1][];
                    promptsArray[0] = prompts.ToArray();
                    allAnswers.AppendChild(GenerateAnswer(document, items[0], categoriesArray, promptsArray));
                }
            }
            for (++lineNum; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
            {
                // TODO: handle multiple categories here
                string name = lines[lineNum];
                string[][] promptsArray = new string[1][];
                promptsArray[0] = new string[0];
                allAnswers.AppendChild(GenerateAnswer(document, name, categoriesArray, promptsArray));
            }
            rootElement.AppendChild(allAnswers);
            document.AppendChild(rootElement);

            return document.OuterXml;
        }

        public static string ParseChartToppersToXML(string filePath)
        {
            const string BY_ARTIST = "Hot 100 Chart Toppers by Artist";
            const string BY_DECADE = "Hot 100 Chart Toppers by Decade";
            string text = "";
            try
            {
                TextAsset tAsset = Resources.Load(filePath) as TextAsset;
                text = tAsset.text;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[XmlGenerator:ParseLineSeparatedToXML] File Read Error");
                return null;
            }
            text = text.Replace("\r", "");
            var lines = text.Split('\n');

            XmlDocument document = new XmlDocument();
            List<string> categoryNames = new List<string>();
            var rootElement = document.CreateElement("Root");
            var categoriesElement = document.CreateElement("Categories");

            var categoryElement = document.CreateElement("Category");
            var nameAttribute = document.CreateAttribute("Name");
            nameAttribute.Value = BY_ARTIST;
            categoryNames.Add(BY_ARTIST);
            categoryElement.Attributes.Append(nameAttribute);
            categoriesElement.AppendChild(categoryElement);

            categoryElement = document.CreateElement("Category");
            nameAttribute = document.CreateAttribute("Name");
            nameAttribute.Value = BY_DECADE;
            categoryNames.Add(BY_DECADE);
            categoryElement.Attributes.Append(nameAttribute);
            categoriesElement.AppendChild(categoryElement);

            rootElement.AppendChild(categoriesElement);
            string[] categoriesArray = categoryNames.ToArray();
            var allAnswers = document.CreateElement("AllAnswers");

            string currentDecade = "";
            Dictionary<string, SongInfo> songs = new Dictionary<string, SongInfo>();

            for (int lineNum = 0; lineNum < lines.Length && lines[lineNum] != ""; ++lineNum)
            {
                string[] items = lines[lineNum].Split('\t');
                if (items.Length < 3)
                {
                    if(items[0][items[0].Length-1] == 's')
                    {
                        currentDecade = items[0];
                    }
                }
                else
                {
                    string name = items[1];
                    string artist = items[2];
                    if(songs.ContainsKey(name))
                    {
                        songs[name].Add(currentDecade, artist);
                    }
                    else
                    {
                        songs.Add(name, new SongInfo(currentDecade, artist));
                    }
                }
            }
            foreach (var song in songs)
            {
                string[][] promptsArray = new string[categoriesArray.Length][];
                for (int catIndex = 0; catIndex < categoriesArray.Length; ++catIndex)
                {
                    if(categoriesArray[catIndex] == BY_DECADE)
                    {
                        promptsArray[catIndex] = song.Value.decades.ToArray();
                    }
                    else if (categoriesArray[catIndex] == BY_ARTIST)
                    {
                        promptsArray[catIndex] = song.Value.artists.ToArray();
                    }
                }
                allAnswers.AppendChild(GenerateAnswer(document, song.Key, categoriesArray, promptsArray));
            }
            rootElement.AppendChild(allAnswers);
            document.AppendChild(rootElement);

            return document.OuterXml;
        }

    }
}