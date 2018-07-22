using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiscordRPC;
using DiscordRPC.Logging;

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

    public DiscordRpcClient client;

    void Initialize()
    {
        client = new DiscordRpcClient("470264480786284544", null, true, -1, new DiscordRPC.IO.NativeNamedPipeClient()); //Create a discord client
        client.Logger = new ConsoleLogger() { Level = LogLevel.Warning }; //Set the logger

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
}
