using AngryDash.Language;
using UnityEngine;
using UnityEngine.UI;
using Tools;
using GooglePlayGames;

public class Social : MonoBehaviour
{
    public LoadingScreenControl LSC;
    public bool EditorContinue = true;
    readonly string scene = "Load";

    public void NewStart()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "googleServiceAuthenticating", "Authenticating...");
        transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        Auth((error, e) =>
        {
            if ((bool)error)
            {
                transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "googleServiceAuthenticationFailed", "Authentication failed.\nGoogle Play service failed to start");
                transform.GetChild(0).GetChild(1).gameObject.SetActive(true);

                Button quit = transform.GetChild(1).GetChild(1).GetComponent<Button>();
                quit.onClick.RemoveAllListeners();
                quit.onClick.AddListener(() => LSC.LoadScreen(scene));
            }
            else
            {
                transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Authenticated";
                Achievement("CgkI9r-go54eEAIQAg", true, (bool s) => { Debug.Log("Deblocage de bienvenue: " + s); }); //Débloque le succès Bienvenue

                LSC.LoadScreen(scene);
            }
        });
    }

    static bool mWaitingForAuth = false;
    public static void Auth(BetterEventHandler onComplete)
    {
        if (!InternetAPI.IsConnected()) { onComplete.Invoke(false, new BetterEventArgs()); return; }
        if (UnityEngine.Social.localUser.authenticated) { onComplete.Invoke(true, new BetterEventArgs()); return; }
        if (!mWaitingForAuth)
        {
            mWaitingForAuth = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            var config = new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .RequestEmail()
            //.RequestServerAuthCode(false)
            //.RequestIdToken()
            .Build();

            //PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true; // recommended for debugging
            PlayGamesPlatform.Activate(); // Activate the Google Play Games platform
#endif
            UnityEngine.Social.localUser.Authenticate((bool success) =>
            {
                mWaitingForAuth = false;
                if (success) Debug.Log("Welcome " + UnityEngine.Social.localUser.userName);
                else Debug.LogWarning("Authentication failed.");
                onComplete.Invoke(!success, new BetterEventArgs());
            });
        }
    }

    static void Check(BetterEventHandler callback)
    {
        if (mWaitingForAuth) Debug.Log("Auth not finished");
        else if (!UnityEngine.Social.localUser.authenticated) Auth((error, e) => {
            if (!(bool)error) callback.Invoke(null, null);
        });
        else callback.Invoke(null, null);
    }
    public static void Achievement(string id, bool unlock, System.Action<bool> callback)
    {
        Check((s, e) =>
        {
            UnityEngine.Social.ReportProgress(id, unlock ? 100F : 0F, callback);
        });
    }
    public static void Achievement(string id, int progress, System.Action<bool> callback)
    {
        Check((s, e) =>
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(id, progress, callback);
#endif
        });
    }
}
