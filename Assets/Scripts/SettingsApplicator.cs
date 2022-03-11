using MessengerExtensions;
using UnityEngine;

public class SettingsApplicator : MonoBehaviour
{

    public GameObject[] objects;
    public string[] voids;

    private void Start()
    {
        for (var i = 0; i < objects.Length; i++)
            objects[i].BroadcastToAll(voids[i]);
    }
}
