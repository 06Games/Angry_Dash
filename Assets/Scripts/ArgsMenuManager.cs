using System.Linq;
using UnityEngine;

public class ArgsMenuManager : MonoBehaviour
{
    public MenuManager[] CM;

    private void Start()
    {
        if (CM.Length < transform.childCount) CM = CM.Concat(new MenuManager[transform.childCount - CM.Length]).ToArray();

        var args = SceneManager.args;
        if (args.Length < 2) return;

        for (var p = 0; p < transform.childCount; p++)
        {
            if (CM[p] == null) CM[p] = transform.GetChild(p).gameObject.GetComponentInChildren<MenuManager>();

            if (CM[p] != null)
            {
                if (args[0] == transform.GetChild(p).name)
                {
                    transform.GetChild(p).gameObject.SetActive(true);
                    for (var i = 0; i < CM[p].GO.Length; i++)
                    {
                        if (CM[p].GO[i].name == args[1])
                            CM[p].array = i;
                    }

                    p = transform.childCount;
                }
            }
        }
    }

    private void Update()
    {

    }
}
