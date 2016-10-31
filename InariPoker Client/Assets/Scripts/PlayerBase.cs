using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InariPoker
{
    public class PlayerBase
    {
        public Vector2 anchorPos { get; protected set; }
        public int[] smallCards = new int[8];
        protected GameObject prefab;

        public void AddCard(int cardtype)
        {
            smallCards[cardtype]++;
            prefab.GetComponent<PlayerPrefabScript>().SetSmallCard(smallCards);
        }

        public void SetTurnCoin(bool active)
        {
            prefab.GetComponent<PlayerPrefabScript>().SetTurnCoin(active);
        }

        public void CreateBalloon(string text)
        {
            prefab.GetComponent<PlayerPrefabScript>().CreateBalloon(text);
        }

        public void ClearBalloon()
        {
            prefab.GetComponent<PlayerPrefabScript>().ClearBalloon();
        }

        public void SetCorrect()
        {
            prefab.GetComponent<PlayerPrefabScript>().SetCorrect();
        }

        public void SetWrong()
        {
            prefab.GetComponent<PlayerPrefabScript>().SetWrong();
        }

        public bool CardOver4()
        {
            foreach (int sum in smallCards)
            {
                if(sum >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetDefeat()
        {
            prefab.GetComponent<PlayerPrefabScript>().SetDefeat();
        }

        public void Destroy()
        {
            GameObject.Destroy(prefab);
        }

        public void SetRoar(int index)
        {
            prefab.GetComponent<PlayerPrefabScript>().SetRoar(index);
        }
    }
}
