using UnityEngine;

public class ArgsMenuManager : MonoBehaviour {

    public LoadingScreenControl LS;
    public CreatorManager CM;

	void Start () {
        string[] args = LS.GetArgs();
        if(args == null) return;
        else if (args.Length < 2) return;

        if(args[0] == name)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            for (int i = 0; i < CM.GO.Length; i++)
            {
                if (CM.GO[i].name == args[1])
                    CM.array = i;
            }
        }
    }
	
	void Update () {
		
	}
}
