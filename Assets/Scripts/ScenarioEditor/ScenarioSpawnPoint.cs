using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioSpawnPoint : ScenarioObject
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
            elem.Name = "ScenarioSpawnPoint";
            return elem;
        }
    }

}
