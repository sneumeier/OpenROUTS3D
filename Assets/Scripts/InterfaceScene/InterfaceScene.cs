using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class InterfaceScene : AutomatedScene
    {
        public InterfaceQuestion[] questions;

        public InterfaceScene(InterfaceQuestion[] iqs)
        {
            questions = iqs;
        }

        public InterfaceScene(XElement root)
        {
            List<InterfaceQuestion> questionList = new List<InterfaceQuestion>();
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "TutorialElement":
                        questionList.Add(new TutorialQuestion(elem));
                        break;
                    case "RadioElement":
                        questionList.Add(new RadioQuestion(elem));
                        break;
                    case "FreeTextElement":
                        questionList.Add(new FreeTextQuestion(elem));
                        break;
                    case "BarElement":
                        questionList.Add(new BarQuestion(elem));
                        break;

                }
            }
            questions = questionList.ToArray();
        }
    }
}
