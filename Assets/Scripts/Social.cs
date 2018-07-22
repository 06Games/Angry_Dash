using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Social : MonoBehaviour
{
    public LoadingScreenControl LSC;
    public bool EditorContinue = true;

    public void NewStart()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Auth();
#elif !UNITY_EDITOR
        LSC.LoadScreen("Home");
#else

        if (EditorContinue)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String("googleServiceAuthenticating");
            transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            LSC.LoadScreen("Home");
        }
#endif
    }

#if UNITY_ANDROID
    bool mWaitingForAuth = false;
#endif
    public void Auth()
    {
#if UNITY_ANDROID
        if (InternetAPI.IsConnected())
        {
            transform.GetChild(0).gameObject.SetActive(true);
            if (!UnityEngine.Social.localUser.authenticated & !mWaitingForAuth)
            {
                mWaitingForAuth = true;
                transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String("googleServiceAuthenticating");
                transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                UnityEngine.Social.localUser.Authenticate((bool success) =>
                {
                    mWaitingForAuth = false;
                    if (success)
                    {
                        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Authenticated";
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
                        LSC.LoadScreen("Home");
                    }
                    else
                    {
                        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String("googleServiceAuthenticationFailed");
                        transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                    }
                });
            }
            else if (!mWaitingForAuth)
                LSC.LoadScreen("Home");
        }
        else LSC.LoadScreen("Home");
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
