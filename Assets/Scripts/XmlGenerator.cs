using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using System.Linq;

namespace StageNine
{
    public class XmlGenerator
    {
        public static void ParseLineSeparatedToXML(string filePath)
        {
            string text1 = "", text2 = "", text3 = "";
            try
            {
                TextAsset tAsset = Resources.Load(filePath + " scsv1") as TextAsset;
                text1 = tAsset.text;
            }
            catch (System.Exception e)
            {
                return;
            }
            try
            {
                TextAsset tAsset = Resources.Load(filePath + " scsv2") as TextAsset;
                text2 = tAsset.text;
            }
            catch (System.Exception e)
            {
                return;
            }
            try
            {
                TextAsset tAsset = Resources.Load(filePath + " scsv3") as TextAsset;
                text3 = tAsset.text;
            }
            catch (System.Exception e)
            {
                return;
            }

            //string[] source = File.ReadAllLines("cust.csv");
            text1 = text1.Replace("\r", "");
            var lines1 = text1.Split('\n');
            text2 = text2.Replace("\r", "");
            var lines2 = text2.Split('\n');
            text3 = text3.Replace("\r", "");
            var lines3 = text3.Split('\n');

            XElement trivia = new XElement("Root",
                new XElement("Categories",
                    from str in lines1
                    let fields = str
                    select new XElement("Category",
                        new XAttribute("name", fields),
                        new XElement("Mode",
                            new XAttribute("name", "General")
                        ),
                        new XElement("Mode",
                            new XAttribute("name", "Specific")
                        )
                    )
                ),
                new XElement("AllAnswers",
                    from str in lines2
                    let fields = str.Split(';')
                    select new XElement("Answer",
                        new XAttribute("name", fields[0]),
                        new XElement("Category",
                            new XAttribute("name", lines1[0]),
                            new XElement("Prompt",
                                new XAttribute("name", "null")
                            ),
                            new XElement("Prompt",
                                new XAttribute("name", fields[1])
                            )
                        )
                    ),
                    from str in lines3
                    let fields = str
                    select new XElement("Answer",
                        new XAttribute("name", fields),
                        new XElement("Category",
                            new XAttribute("name", lines1[0])
                        )
                    )
                )
            );

            


            Debug.Log(trivia);
        }
    }
}