using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Social : MonoBehaviour
{

    void Start()
    {
#if UNITY_EDITOR
        transform.GetChild(0).gameObject.SetActive(false);
#elif UNITY_ANDROID
        if(!BaseControl.CheckTex())
            Auth();
#endif
    }

    bool mWaitingForAuth = false;
    public void Auth()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        if (!UnityEngine.Social.localUser.authenticated & !mWaitingForAuth)
        {
            mWaitingForAuth = true;
            transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Authenticating...";
            transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            UnityEngine.Social.localUser.Authenticate((bool success) =>
            {
                mWaitingForAuth = false;
                if (success)
                {
                    transform.GetChild(0).gameObject.SetActive(false);
                    PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                    .EnableSavedGames()
                    .RequestEmail()
                    .RequestServerAuthCode(false)
                    .RequestIdToken()
                    .Build();

                    PlayGamesPlatform.InitializeInstance(config);
                    PlayGamesPlatform.DebugLogEnabled = true; // recommended for debugging
                    PlayGamesPlatform.Activate(); // Activate the Google Play Games platform
                    UnityEngine.Social.ReportProgress("CgkI9r-go54eEAIQAg", 100.0f, (bool s) => { }); //Débloque le succès Bienvenue
                }
                else
                {
                    transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Authentication failed.\nGoogle Play service failed to start";
                    transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                }
            });
        }
        else if (!mWaitingForAuth)
            transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
