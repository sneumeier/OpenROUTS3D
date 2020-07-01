using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioPylon:ScenarioObject
    {
        public override ScenarioObject Deserialize(XElement elem)
        {
            base.Deserialize(elem);
            return this;
        }

        public override XElement Serialize()
        {
            XElement elem = base.Serialize();
            XElement subelem;
            elem.Name = "ScenarioPylon";

            return elem;
        }
    }

}
