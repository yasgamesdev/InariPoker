using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InariPoker
{
    public class PrefabManager
    {
        GameObject instance = null;

        public void LoadPrefab(string path)
        {
            instance = GameObject.Instantiate(Resources.Load(path)) as GameObject;
        }

        public void LoadPrefab(string path, Transform parent)
        {
            instance = GameObject.Instantiate(Resources.Load(path), parent) as GameObject;
        }

        public GameObject GetInstance()
        {
            return instance;
        }

        public void ReloadPrefab(string path)
        {
            GameObject.Destroy(instance);

            instance = GameObject.Instantiate(Resources.Load(path)) as GameObject;
        }

        public void Destroy()
        {
            if (instance != null)
            {
                GameObject.Destroy(instance);
            }
        }
    }
}
