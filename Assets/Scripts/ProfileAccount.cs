using _06Games.Account;
using UnityEngine;

public class ProfileAccount : MonoBehaviour
{
    public void Advanced() { Application.OpenURL("https://06games.ddns.net/accounts/"); }
    public void Disconnect() { API.Disconnect(); }
}
