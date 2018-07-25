using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiscordClasses;
/*using DiscordRPC;
using DiscordRPC.Logging;*/

public class Discord : MonoBehaviour
{
    /*
    [Header("Details")]
    public string inputDetails;
    public string inputState;

    [Header("Time")]
    public bool inputStartTime;
    public float inputEndTime;

    [Header("Images")]
    public string inputLargeKey;
    public string inputLargeTooltip;
    public string inputSmallKey;
    public string inputSmallTooltip;

    public DiscordPresence presence;
    */

    /*public DiscordRpcClient client;

    void Initialize()
    {
        client = new DiscordRpcClient("470264480786284544", null, true, -1, new DiscordRPC.IO.NativeNamedPipeClient()); //Create a discord client
        client.Logger = new DebugLogger() { Level = LogLevel.Info }; //Set the logger

        //Subscribe to events
        client.OnPresenceUpdate += (sender, e) =>
        {
            Debug.LogFormat("Received Update! {0}", e.Presence);
        };

        client.Initialize(); //Connect to the RPC

        //Set the rich presence
        client.SetPresence(new RichPresence()
        {
            Details = "Example Project",
            State = "csharp example",
            Assets = new Assets()
            {
                LargeImageKey = "default"
            }
        });
    }

    void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    void Update()
    {
        client.Invoke(); //Invoke all the events, such as OnPresenceUpdate
    }

    void OnDisable()
    {
        client.Dispose();
    }

    /*public void SendPresence()
    {
        UpdatePresence();
        DiscordManager.instance.SetPresence(presence);
    }

    public void UpdateFields(DiscordPresence presence)
    {
        this.presence = presence;
        inputState = presence.state;
        inputDetails = presence.details;


        inputLargeTooltip = presence.largeAsset.tooltip;
        inputLargeKey = presence.largeAsset.image;

        inputSmallTooltip = presence.smallAsset.tooltip;
        inputSmallKey = presence.smallAsset.image;
    }

    public void UpdatePresence()
    {
        presence.state = inputState;
        presence.details = inputDetails;
        presence.startTime = inputStartTime ? new DiscordTimestamp(Time.realtimeSinceStartup) : DiscordTimestamp.Invalid;

        presence.largeAsset = new DiscordAsset()
        {
            image = inputLargeKey,
            tooltip = inputLargeTooltip
        };
        presence.smallAsset = new DiscordAsset()
        {
            image = inputSmallKey,
            tooltip = inputSmallTooltip
        };

        presence.endTime = inputEndTime > 0 ? new DiscordTimestamp(Time.realtimeSinceStartup + inputEndTime) : DiscordTimestamp.Invalid;
    }*/

    /// <summary>
    /// Update the Discord RPC PResence
    /// </summary>
    /// <param name="state">Titre 1</param>
    /// <param name="detail">Titre 2</param>
    /// <param name="lImage">Image 1</param>
    /// <param name="sImage">Image 2 (null pour désactiver)</param>
    /// <param name="remainingTime">Temps restant avant la fin de la partie (-1 pour le désactiver)</param>
    /// <param name="startTime">Temps en seconde depuis le démarage de la partie (-1 pour actuelement, 0 pour le désactiver)</param>
    public static void Presence(string state, string detail, Img lImage, Img sImage = null, int remainingTime = -1, long startTime = 0)
    {
#if UNITY_STANDALONE
        GameObject go = GameObject.Find("Discord");
        if (go != null)
            go.GetComponent<Discord>().UpdatePresence(state, detail, lImage, sImage, remainingTime, startTime);
#endif
    }

#if UNITY_STANDALONE
    public DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
    public int callbackCalls;
    public DiscordRpc.DiscordUser joinRequest;
    public UnityEngine.Events.UnityEvent onConnect;
    public UnityEngine.Events.UnityEvent onDisconnect;
    public UnityEngine.Events.UnityEvent hasResponded;
    public DiscordJoinEvent onJoin;
    public DiscordJoinEvent onSpectate;
    public DiscordJoinRequestEvent onJoinRequest;

    DiscordRpc.EventHandlers handlers;

    void UpdatePresence(string state, string detail, Img lImage, Img sImage, int remainingTime, long startTime)
    {
        if (lImage != null)
        {
            presence.largeImageKey = lImage.key;
            presence.largeImageText = lImage.legende;
        }
        if (sImage != null)
        {
            presence.smallImageKey = sImage.key;
            presence.smallImageText = sImage.legende;
        }
        presence.state = state;
        presence.details = detail;
        if (startTime == -1)
            presence.startTimestamp = System.DateTime.UtcNow.Second;
        else if(startTime >= 0)
            presence.startTimestamp = startTime;
        if (remainingTime >= 0)
            presence.endTimestamp = System.DateTime.UtcNow.Second + remainingTime;
        else presence.endTimestamp = 0;
        DiscordRpc.UpdatePresence(presence);
    }

    public void RequestRespond(bool accept)
    {
        if (accept)
        {
            Debug.Log("Discord: responding yes to Ask to Join request");
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
            hasResponded.Invoke();
        }
        else
        {
            Debug.Log("Discord: responding no to Ask to Join request");
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
            hasResponded.Invoke();
        }
    }

    public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
        onConnect.Invoke();
    }

    public void DisconnectedCallback(int errorCode, string message)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
        onDisconnect.Invoke();
    }

    public void ErrorCallback(int errorCode, string message)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
    }

    public void JoinCallback(string secret)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: join ({0})", secret));
        onJoin.Invoke(secret);
    }

    public void SpectateCallback(string secret)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: spectate ({0})", secret));
        onSpectate.Invoke(secret);
    }

    public void RequestCallback(ref DiscordRpc.DiscordUser request)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
        joinRequest = request;
        onJoinRequest.Invoke(request);
    }

    void OnEnable()
    {
        Debug.Log("Discord API is starting");
        DontDestroyOnLoad(gameObject);
        callbackCalls = 0;

        handlers = new DiscordRpc.EventHandlers();
        handlers.readyCallback = ReadyCallback;
        handlers.disconnectedCallback += DisconnectedCallback;
        handlers.errorCallback += ErrorCallback;
        handlers.joinCallback += JoinCallback;
        handlers.spectateCallback += SpectateCallback;
        handlers.requestCallback += RequestCallback;
        DiscordRpc.Initialize("470264480786284544", ref handlers, true, "");
        Presence("Starting the game", "", new Img("default"));
    }

    void OnDisable()
    {
        DiscordRpc.Shutdown();
    }

    void Update()
    {
        DiscordRpc.RunCallbacks();
    }
#endif
}

namespace DiscordClasses {
    public class Img
    {
        /// <summary>
        /// Créer une image d'illustration
        /// </summary>
        /// <param name="_key">Le nom de l'image</param>
        public Img(string _key) { key = _key; legende = ""; }
        /// <summary>
        /// Créer une image d'illustration
        /// </summary>
        /// <param name="_key">Le nom de l'image</param>
        /// <param name="_legende">La légende de l'image (s'affiche au survole)</param>
        public Img(string _key, string _legende) { key = _key; legende = _legende; }
        public string key;
        public string legende;
    }
}
