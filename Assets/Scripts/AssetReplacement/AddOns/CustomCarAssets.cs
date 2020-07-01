using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class CustomCarAssets: MonoBehaviour, IAssetPackPreInit
    {
        public List<SumoVehicle> carAssets = new List<SumoVehicle>();
        public List<double> weights = new List<double>();

        public void PreInit()
        {
            Debug.Log("Adding custom vehicles");
            int i = 0;
            foreach (SumoVehicle car in carAssets)
            {
                double weight = 1;
                if (i < weights.Count)
                {
                    weight = weights[i];
                }
                PrefabProvider.customCars.Add(car, weight);
                i++;
            }
        }
    }
}
