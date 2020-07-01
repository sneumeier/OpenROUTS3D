using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.CarScripts
{
    public class LoggedCrash
    {
        public float timestamp;
        public string collisionObjectName = "";
        public Vector3 impulse;
        public List<Vector3> impactPoints = new List<Vector3>();
        public Vector3 relativeVelocity;

        public XElement Serialize()
        {
            XElement elem = new XElement("LoggedCrash");

            XElement subelem;

            subelem = new XElement("Timestamp");
            subelem.Value = timestamp.ToString();
            elem.Add(subelem);

            subelem = new XElement("CollisionObjectName");
            subelem.Value = collisionObjectName;
            elem.Add(subelem);

            subelem = new XElement("Impulse");
            subelem.Value = impulse.ToString();
            elem.Add(subelem);

            subelem = new XElement("RelativeVelocity");
            subelem.Value = relativeVelocity.ToString();
            elem.Add(subelem);

            subelem = new XElement("ImpactPoints");
            foreach (Vector3 v3 in impactPoints)
            {
                XElement impactElem = new XElement("ImpactPoint");
                impactElem.Value = v3.ToString();
                subelem.Add(impactElem);
            }
            elem.Add(subelem);

            return elem;

        }

    }
}
