using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InariPoker
{
    class BigCardScript : MonoBehaviour
    {
        int index;

        public void Init(int index)
        {
            this.index = index;
        }

        public void CardClick()
        {
            Players.SelectBigCard(index);
        }
    }
}
