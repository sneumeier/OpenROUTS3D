using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class InterfaceQuestion
    {
        public string text;
        public string identifier;
        public string answer = "";
        public List<string> multipleAnswers = new List<string>();

        public InterfaceQuestion(string text, string identifier)
        {
            this.text = text;
            this.identifier = identifier;
        }

        public void ResetAnswers()
        {
            answer = "";
            multipleAnswers.Clear();
        }

        public void SaveAnswers()
        {
            if (!AutorunDeserializer.answers.ContainsKey(identifier))
            {
                AutorunDeserializer.answers.Add(identifier, new List<string>());
            }
            AutorunDeserializer.answers[identifier].Clear();
            AutorunDeserializer.answers[identifier].Add(answer);
            foreach (string answ in multipleAnswers)
            {
                AutorunDeserializer.answers[identifier].Add(answ);
            }
        }
    }
}
