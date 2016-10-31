using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace InariPoker
{
    class SelectCardScript : MonoBehaviour
    {
        int cardtype;

        public void Init(int cardtype)
        {
            this.cardtype = cardtype;
            GetComponent<Image>().sprite = Resources.Load<Sprite>("0" + cardtype.ToString());
        }

        public void CardClick()
        {
            Players.DecideCard(cardtype);
        }
    }
}
