using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Language.Reader
{
    public class DropdownLanguagesTxt : MonoBehaviour
    {
        public string category = "native";
        public string[] id;
        public bool keepIfNotExist = true;

        private void Start()
        {
            for (var i = 0; i < GetComponent<Dropdown>().options.Capacity & i < id.Length; i++)
            {
                var txt = LangueAPI.Get(category, id[i], null);
                if (txt == null & !keepIfNotExist) txt = "<color=red>Language File Error</color>";
                GetComponent<Dropdown>().options[i].text = txt;
            }
        }
    }
}
