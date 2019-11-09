using AngryDash.Language;
using UnityEngine;
using UnityEngine.UI;

public class GetVersion : MonoBehaviour
{

    void Start()
    {
        GetComponent<Text>().text = LangueAPI.Get("native", "gameVersion", "v[0]", Base.GetVersion());
    }
}
