using MessengerExtensions;
using UnityEngine;

public class SettingsApplicator : MonoBehaviour
{

    public GameObject[] objects;
    public string[] voids;

    void Start()
    {
        for (int i = 0; i < objects.Length; i++)
            objects[i].BroadcastToAll(voids[i]);
    }
}
