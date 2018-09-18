using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatorManager : MonoBehaviour {

    public GameObject[] GO;
    public int array;

	void Update () {
        for(int i = 0; i < GO.Length; i++)
        {
            if(i == array)
                GO[i].SetActive(true);
            else GO[i].SetActive(false);
        }
	}

    public void ChangArray(int a) {
        array = a;
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void Array(int a) { array = a; }
}
