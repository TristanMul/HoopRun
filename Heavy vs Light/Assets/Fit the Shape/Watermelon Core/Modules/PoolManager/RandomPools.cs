using UnityEngine;
using System.Collections.Generic;
using Watermelon.Core;

//Pool module v 1.4.4
// NOT FINISHED

namespace Watermelon
{
    [System.Serializable]
    public class RandomPools
    {
        private List<Pool> poolsList = new List<Pool>();

        private List<int> weightsList = new List<int>();
        private int weightsSum;

        public void AddPools(params RandomPoolSettings[] settings)
        {
            for (int i = 0; i < settings.Length; i++)
            {
                poolsList.Add(PoolManager.GetPoolByName(settings[i].name));
                weightsList.Add(settings[i].weight);
                weightsSum += settings[i].weight;
            }
        }

        public Pool GetRandomPool()
        {
            if (poolsList.IsNullOrEmpty())
            {
                Debug.Log("Random pools is not initialized.");
                return null;
            }

            int randomValue = Random.Range(1, weightsSum + 1);
            int currentValue = 0;

            for (int i = 0; i < weightsList.Count; i++)
            {
                currentValue += weightsList[i];

                if (randomValue <= currentValue)
                {
                    return poolsList[i];
                }
            }

            Debug.LogError("Random value(" + randomValue + ") is out of weights sum range.");
            return poolsList[0];
        }
    }

    [System.Serializable]
    public class RandomPoolSettings
    {
        public string name;
        public int weight;

        public RandomPoolSettings(string poolName, int poolWeight = 1)
        {
            if (poolName == null || poolName == string.Empty)
            {
                Debug.LogError("Pool name is empty. Please enter valid pool name.");
            }

            name = poolName;
            weight = poolWeight;
        }
    }
}