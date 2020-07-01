using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class RadioQuestion:InterfaceQuestion
    {

        public List<string> radioOptions = new List<string>();
        public bool multiselect = false;

        public RadioQuestion(string text, string identifier, List<string> options)
            : base(text, identifier)
        {
            radioOptions = options;
        }

        public RadioQuestion(XElement root)
            : base("text", "identifier")
        {
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "Text":
                        text = elem.Value;
                        break;
                    case "Identifier":
                        identifier = elem.Value;
                        break;
                    case "RadioOption":
                        radioOptions.Add(elem.Value);
                        break;
                    case "Multiselect":
                        multiselect = bool.Parse(elem.Value);
                        break;
                }
            }
        }


    }
}
