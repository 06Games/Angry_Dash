using AngryDash.Language;
using GooglePlayGames;
using Tools;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Linq;
using GooglePlayGames;
#endif

public class Social : MonoBehaviour
{
    readonly string scene = "Load";

#if UNITY_EDITOR
    static bool editorContinue = true;
#endif

    public void NewStart()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "gameServices.authentication", "Authentication...");
        transform.GetChild(0).GetChild(1).gameObject.SetActive(false);

        Button quit = transform.GetChild(1).GetChild(1).GetComponent<Button>();
        quit.onClick.RemoveAllListeners();
        quit.onClick.AddListener(() =>
        {
            Account ac = GameObject.Find("Account").GetComponent<Account>();
            ac.complete += () => LoadingScreenControl.LoadScreen(scene);
            ac.Initialize();
            gameObject.SetActive(false);
        });
        Auth((error) =>
        {
            if (error)
            {
                transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "gameServices.authentication.failed.google", "Authentication failed.\nGoogle Play service failed to start");
                transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "gameServices.authentication.success", "Authenticated");
                Achievement("CgkI9r-go54eEAIQAg", true, (bool s) => { }); //Achievement 'Welcome'

                Account ac = GameObject.Find("Account").GetComponent<Account>();
                ac.complete += () => LoadingScreenControl.LoadScreen(scene);
                ac.Initialize();
                gameObject.SetActive(false);
            }
        });
    }

    static bool mWaitingForAuth = false;
    /// <summary>Authentificate to the game service</summary>
    /// <param name="onComplete">The function to call at the end of the operation</param>
    public static void Auth(System.Action<bool> onComplete)
    {
        if (!InternetAPI.IsConnected()) { onComplete.Invoke(false); return; }
        if (UnityEngine.Social.localUser.authenticated) { onComplete.Invoke(true); return; }
        if (!mWaitingForAuth)
        {
            mWaitingForAuth = true;
#if UNITY_ANDROID
            var config = new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .RequestEmail()
            //.RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true; // Bug fix
            PlayGamesPlatform.Activate(); // Activate the Google Play Games platform
#endif
            try
            {
                UnityEngine.Social.localUser.Authenticate((bool success) =>
                {
                    mWaitingForAuth = false;
#if UNITY_EDITOR
                if (!editorContinue && UnityEngine.Social.localUser.userName == "Lerpz") success = false;
#endif
                if (success) Logging.Log("Successfully connected to the Game Services. Welcome " + UnityEngine.Social.localUser.userName);
                    else Logging.Log("Authentication failed.", LogType.Warning);
                    onComplete.Invoke(!success);
                });
            }
            catch(System.Exception e) { Debug.LogError(e); onComplete.Invoke(false); }
        }
    }

    #region Call
    /// <summary>Check if the user is authenticated</summary>
    /// <param name="callback">The api to call at the end of the check</param>
    static void Check(System.Action callback, System.Action errorCallback)
    {
        if (!InternetAPI.IsConnected()) errorCallback.Invoke();
        else if (mWaitingForAuth) errorCallback.Invoke();
        else if (!UnityEngine.Social.localUser.authenticated)
        {
            Auth((error) =>
            {
                if (error) errorCallback.Invoke();
                else callback.Invoke();
            });
        }
        else callback.Invoke();
    }


    /***************
     * Achievement *
     ***************/

    /// <summary>Reveal/Unlock an achievement</summary>
    /// <param name="id">ID of the achievement</param>
    /// <param name="unlock">If true, unlock the achievement else reveal it</param>
    /// <param name="callback">The function to call at the end of the operation</param>
    public static void Achievement(string id, bool unlock, System.Action<bool> callback)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Social.ReportProgress(id, unlock ? 100F : 0F, callback);
#endif
        }, () => callback.Invoke(false));
    }
    /// <summary>Modify an achievement</summary>
    /// <param name="id">ID of the achievement</param>
    /// <param name="progress">The progress of the achievement (in %)</param>
    /// <param name="callback">The function to call at the end of the operation</param>
    public static void Achievement(string id, double progress, System.Action<bool> callback)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Social.LoadAchievements((data) =>
            {
                var aData = data.FirstOrDefault(d => d.id == id);
                double Progress = progress;
                if (aData == null || aData.percentCompleted < progress) UnityEngine.Social.ReportProgress(id, progress, callback);
            });
#endif
        }, () => callback.Invoke(false));
    }


    /***************
     * Leaderboard *
     ***************/

    /// <summary>Modify a score</summary>
    /// <param name="id">ID of the leaderboard</param>
    /// <param name="score">The player's score</param>
    /// <param name="callback">The function to call at the end of the operation</param>
    public static void Leaderboard(string id, long score, System.Action<bool> callback)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Social.ReportScore(score, id, callback);
#endif
        }, () => callback.Invoke(false));
    }
    /// <summary>Modify a score</summary>
    /// <param name="id">ID of the leaderboard</param>
    /// <param name="score">The player's score</param>
    /// <param name="tag">Metadata tag</param>
    /// <param name="callback">The function to call at the end of the operation</param>
    public static void Leaderboard(string id, long score, string tag, System.Action<bool> callback)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.ReportScore(score, id, tag, callback);
#endif
        }, () => callback.Invoke(false));
    }


    /**********
     * Events *
     **********/

    /// <summary>Increment an event</summary>
    /// <param name="id">ID of the event</param>
    /// <param name="toAdd">The value to add the current score</param>
    public static void IncrementEvent(string id, ulong toAdd)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.Events.IncrementEvent(id, (uint)toAdd);
#endif
        }, () => { });
    }
    /// <summary>Modify an event</summary>
    /// <param name="id">ID of the event</param>
    /// <param name="value">The value of the event</param>
    public static void Event(string id, ulong value)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.Events.FetchEvent(GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork, id, (a, d) =>
            {
                long toAdd = (long)value - (long)d.CurrentCount;
                if (toAdd > 0) PlayGamesPlatform.Instance.Events.IncrementEvent(id, (uint)toAdd);
            });
#endif
        }, () => { });
    }
    /// <summary>Modify an event</summary>
    /// <param name="id">ID of the event</param>
    /// <param name="callback">The function to call after receiving the value (bool: error, ulong: value)</param>
    public static void GetEvent(string id, System.Action<bool, ulong> callback)
    {
        Check(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.Events.FetchEvent(GooglePlayGames.BasicApi.DataSource.ReadNetworkOnly, id, (a, d) => callback.Invoke(a >= 0, d == null ? 0: d.CurrentCount));
#else
            callback.Invoke(true, 0);
#endif
        }, () => callback.Invoke(false, 0));
    }
    #endregion
}
