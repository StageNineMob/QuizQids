
using System.Collections.Generic;

namespace StageNine
{
    public class TriviaPair
    {
        public List<string> prompts;
        public string value;

        public TriviaPair()
        {
            prompts = new List<string>();
        }

        //for profiler objects;
        public TriviaPair(int id)
        {
            value = id.ToString();
            prompts = new List<string>();
        }
    }
}
