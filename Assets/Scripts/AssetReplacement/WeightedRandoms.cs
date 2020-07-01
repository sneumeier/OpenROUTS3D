using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AssetReplacement
{
    public static class WeightedRandoms
    {
        public static Random Rand = new Random(0);

        public static void InitRandomSeed()
        {
            Rand = new Random(0);
        }

        ///Picks an object of a Dictionary of Objects mapped to weights. Weights determine how probable it is.
        public static T Shuffle<T>(Dictionary<T, double> pairs, bool debug=false)
        {
            if (pairs.Count == 0)
            {
                return default(T);//Default of T should essentially always be null. In theory you could pass structs too though, e.g. if you pass an empty Vector3 list, you'll yield (0;0;0)
            }
            double sum = 0;
            foreach (var kvp in pairs)
            {
                if (kvp.Value < 0)
                {
                    continue;
                }
                sum += kvp.Value;
            }
            if (debug)
            {
                foreach (var kvp in pairs)
                {
                    UnityEngine.Debug.Log("Chance for " + kvp.Key.ToString() + " = " + ((kvp.Value / sum) * 100).ToString() + "%");
                }
            }
            double rand = Rand.NextDouble() * sum;
            double currentSum = 0;
            foreach (var kvp in pairs)
            {
                if (kvp.Value < 0)
                {
                    continue;
                }
                currentSum += kvp.Value;
                if (currentSum > rand)
                {
                    return kvp.Key;
                }
            }
            return pairs.Last().Key;
        }

    }
}
