using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class FreeTextQuestion:InterfaceQuestion
    {
        public bool isNumber;
        public bool required;

        public FreeTextQuestion(string text, string identifier, bool isNumber, bool required)
            : base(text, identifier)
        {
            this.isNumber = isNumber;
            this.required = required;
        }

        public FreeTextQuestion(XElement root)
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
                    case "IsNumber":
                        isNumber = bool.Parse(elem.Value);
                        break;
                    case "Required":
                        required = bool.Parse(elem.Value);
                        break;
                }
            }
        }

    }
}
