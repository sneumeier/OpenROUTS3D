using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class TutorialQuestion:InterfaceQuestion
    {
        public string imagePath;
        public bool hasImage;

        public TutorialQuestion(string text, string identifier, string imagePath, bool hasImage)
            : base(text, identifier)
        {
            this.imagePath = imagePath;
            this.hasImage = hasImage;
        }

        public TutorialQuestion(XElement root):base("text","identifier")
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
                    case "ImagePath":
                        imagePath = elem.Value;
                        hasImage = true;
                        break;
                }
            }
        }

    }
}
