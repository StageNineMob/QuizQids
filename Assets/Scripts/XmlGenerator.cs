using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;


namespace StageNine
{
    public class XmlGenerator
    {
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

        public static void ParseLineSeparatedToXML(string filePath)
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
                return;
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
                    if(items.Length > 1)
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

            // TODO: DO SOMETHING USEFUL WITH THIS!!!
            Debug.Log(document.OuterXml);
        }
    }
}