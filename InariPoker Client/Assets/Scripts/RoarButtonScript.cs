using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace InariPoker
{
    class RoarButtonScript : MonoBehaviour
    {
        [SerializeField]
        Text text;
        int index;

        public void Init(int index, string text)
        {
            this.index = index;
            this.text.text = text;
        }

        public void ButtonClick()
        {
            GameObject.Find("Network").GetComponent<NetworkScript>().RoarButtonClick(index);
        }
    }
}
