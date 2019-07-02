using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Language.Reader
{
    public class LangueTxt : MonoBehaviour
    {
        public string category = "native";
        public string id;
        public string[] arg = new string[0];

        void Start() { GetComponent<Text>().text = LangueAPI.Get(category, id, GetComponent<Text>().text, arg); }
    }
}
