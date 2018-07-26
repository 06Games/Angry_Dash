using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiscordClasses;
using System;
/*using DiscordRPC;
using DiscordRPC.Logging;*/

public class Discord : MonoBehaviour
{
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
    }*/

    /// <summary>
    /// Update the Discord RPC PResence
    /// </summary>
    /// <param name="state">Titre 1</param>
    /// <param name="detail">Titre 2 (null pour le désactiver)</param>
    /// <param name="lImage">Image 1 (null pour garder l'image actuelle)</param>
    /// <param name="sImage">Image 2 (null pour désactiver)</param>
    /// <param name="remainingTime">Temps restant avant la fin de la partie (-1 pour le désactiver, 0 pour actuelement)</param>
    /// <param name="startTime">Temps en seconde depuis le démarage de la partie (-1 pour le désactiver, 0 pour actuelement)</param>
    public static void Presence(string state, string detail = null, Img lImage = null, Img sImage = null, int remainingTime = -1, long startTime = -1)
    {
#if UNITY_STANDALONE
        GameObject go = GameObject.Find("Discord");
        if (go != null)
            go.GetComponent<Discord>().UpdatePresence(state, detail, lImage, sImage, remainingTime, startTime);
#endif
    }

#if UNITY_STANDALONE
    public DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
    public DiscordRpc.DiscordUser joinRequest;
    public UnityEngine.Events.UnityEvent onConnect;
    public UnityEngine.Events.UnityEvent onDisconnect;
    public DiscordJoinEvent onJoin;
    public DiscordJoinEvent onSpectate;
    public DiscordJoinRequestEvent onJoinRequest;

    DiscordRpc.EventHandlers handlers;

    void UpdatePresence(string detail, string state, Img lImage, Img sImage, int remainingTime, long startTime)
    {
        presence.state = state; //State
        presence.details = detail; //Detail

        //Large Image
        if (lImage != null)
        {
            presence.largeImageKey = lImage.key;
            presence.largeImageText = lImage.legende;
        }

        //Small Image
        if (sImage != null)
        {
            presence.smallImageKey = sImage.key;
            presence.smallImageText = sImage.legende;
        }
        else
        {
            presence.smallImageKey = null;
            presence.smallImageText = null;
        }

        //End Timestamp
        if (remainingTime >= 0)
            presence.endTimestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) + remainingTime;
        else presence.endTimestamp = 0;
        
        //Start Timestamp
        if (startTime == 0)
            presence.startTimestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        else if (startTime > 0)
            presence.startTimestamp = startTime;
        else if (startTime == -1)
            presence.startTimestamp = 0;

        DiscordRpc.UpdatePresence(presence); //Apply Changes
    }

    public void RequestRespond(bool accept)
    {
        if (accept)
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
        else DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
    }

    public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
    {
        Debug.Log(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
        onConnect.Invoke();
    }

    public void DisconnectedCallback(int errorCode, string message)
    {
        Debug.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
        onDisconnect.Invoke();
    }

    public void ErrorCallback(int errorCode, string message)
    {
        Debug.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
    }

    public void JoinCallback(string secret)
    {
        Debug.Log(string.Format("Discord: join ({0})", secret));
        onJoin.Invoke(secret);
    }

    public void SpectateCallback(string secret)
    {
        Debug.Log(string.Format("Discord: spectate ({0})", secret));
        onSpectate.Invoke(secret);
    }

    public void RequestCallback(ref DiscordRpc.DiscordUser request)
    {
        Debug.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
        joinRequest = request;
        onJoinRequest.Invoke(request);
    }

    void OnEnable()
    {
        Debug.Log("Discord API is starting");
        DontDestroyOnLoad(gameObject);

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
