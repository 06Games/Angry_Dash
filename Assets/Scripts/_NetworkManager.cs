using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class _NetworkManager : MonoBehaviour {

    public GameObject Selection;

	public void StartHost()
    {
        GetComponent<NetworkManager>().StartHost();
        Selection.SetActive(false);
    }
    public void Join()
    {
        GetComponent<NetworkManager>().StartClient();
        Selection.SetActive(false);
    }

    public void Disconnect()
    {
        GetComponent<NetworkManager>().StopHost();
        GetComponent<NetworkManager>().StopClient();
        Selection.SetActive(true);
    }
}
