using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.AssetReplacement
{
    public class AddonButton:MonoBehaviour
    {
        public Image icon;
        public bool active = true;
        public AssetPack pack;

        public void Start()
        {
            if (!active)
            {
                icon.color = Color.grey;
                pack.requestLoad = false;
            }
            else
            {
                icon.color = Color.white;
                pack.requestLoad = true;
            }
        }

        public void ToggleActive() {
            active = !active;
            if (!active)
            {
                icon.color = Color.grey;
                pack.requestLoad = false;
            }
            else {
                icon.color = Color.white;
                pack.requestLoad = true;
            }
        }

    }
}
