using UnityEngine;

public class MainCam : MonoBehaviour
{
    public GameObject Player;
    public bool OnPlayer = true;

    void Update()
    {
        if (Player != null & OnPlayer)
            transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + (Screen.height * 0.25F), -10);
    }
}
