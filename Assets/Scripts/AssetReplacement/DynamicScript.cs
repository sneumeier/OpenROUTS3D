using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public class DynamicScript:MonoBehaviour
    {
        #region Properties and Fields
        public TextAsset scriptAsset;
        public bool isComponent;
        public string className;

        private Assembly _assembly = null;
        public Assembly scriptAssembly
        { 
            get
            {
                if(_assembly==null)
                {
                    _assembly = LoadScript();
                }
                return _assembly;
            }
        }
        #endregion Properties and Fields
        #region Constructor

        #endregion Constructor
        #region Methods
        public Assembly LoadScript()
        {
            return Assembly.Load(scriptAsset.text);
        }

        public void Awake()
        {
            if (isComponent)
            {
                Type reflectedType = scriptAssembly.GetType(className);
                //Invoke gameobject.AddComponent with generic attribute of the reflected type of our assembly
            }
        }


        #endregion Methods
    }
}
