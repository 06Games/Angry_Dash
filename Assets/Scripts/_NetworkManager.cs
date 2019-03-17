using Level;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

#pragma warning disable CS0618 // En attendant la nouvelle API

public class MyMsgBase : MessageBase
{
    public string map;
};
public class ServInfo : MessageBase
{
    public byte[] icon;
    public string Name;
    public int player;
    public int maxPlayer;
};
public class MsgID
{
    public const short AskForServerInfo = 500;
    public const short GetServerInfo = 501;
    public const short AskForServerMap = 502;
    public const short GetServerMap = 503;
}


    public class ServRequest : MessageBase{};

public class _NetworkManager : NetworkBehaviour
{
    public GameObject Selection;
    public GameObject Items;

    public LoadingScreenControl LSC;

    NetworkManager NM;

    string adress = "localhost";
    int port = 7777;

    private void Start()
    {
        NM = GetComponent<NetworkManager>();

        string[] launchArgs = LSC.GetArgs();
        if (launchArgs == null) launchArgs = new string[0];
        if (launchArgs.Length > 0)
        {
            if (launchArgs[0] == "Connect")
            {
                string[] param = launchArgs[1].Split(new string[] { ":" }, StringSplitOptions.None);
                adress = param[0];
                port = int.Parse(param[1]);
            }

            Join();
        }
        else return;

        Client.RegisterHandler(MsgID.GetServerMap, OnConnected);
    }

    public void Join()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        NM.networkAddress = adress;
        NM.networkPort = port;
        NetworkClient NC = NM.StartClient();
        Selection.SetActive(true);
        Selection.transform.GetChild(0).gameObject.SetActive(true);
        NC.RegisterHandler(MsgType.Disconnect, Error);
        NC.RegisterHandler(MsgType.Error, Error);
        StartData();
        mapRequested = false;

        Discord.Presence(LangueAPI.String("native", "discordServer_title"), "", new DiscordClasses.Img("default", LangueAPI.StringWithArgument("native", "discordServer_caption", new string[] { adress, port.ToString() })), null, -1, 0);
        History.LvlPlayed(adress + ":" + port, "S", "");
    }

    void Error(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        Transform ErrorPanel = Selection.transform.GetChild(1);
        string title = "";
        string subtitle = "";

        if (netMsg.msgType == 33)
        {
            title = LangueAPI.String("native", "ServerErrorNotResponding_title");
            subtitle = LangueAPI.String("native", "ServerErrorNotResponding_subtitle");
        }
        else
        {
            title = LangueAPI.String("native", "ServerErrorUnknown_title");
            subtitle = LangueAPI.StringWithArgument("native", "ServerErrorUnknown_subtitle", netMsg.msgType);
        }

        ErrorPanel.GetChild(0).GetComponent<Text>().text = title;
        ErrorPanel.GetChild(1).GetComponent<Text>().text = subtitle;

        Selection.transform.GetChild(0).gameObject.SetActive(false);
        Selection.SetActive(true);
        ErrorPanel.gameObject.SetActive(true);
    }

    public void Disconnect()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        NM.StopHost();
        NM.StopClient();
        Selection.SetActive(false);
        isSetup = false;

        for (int i = 0; i < Items.transform.childCount; i++)
            Destroy(Items.transform.GetChild(i).gameObject);

        Discord.Presence(LangueAPI.String("native", "discordHome_title"), "", new DiscordClasses.Img("default"));
        LSC.LoadScreen("Home", new string[] { "Play", "Community Servers" });
    }

    private bool isSetup = false;
    private NetworkClient Client;

    public void StartData()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (!isSetup)
            SetupClient();
    }
    public MyMsgBase m_Message;
    public void SetupClient()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        NetworkManager net = NM;
        Client = net.client;
        Client.RegisterHandler(MsgID.GetServerMap, OnConnected);

        isSetup = true;
    }

    bool mapRequested = true;
    private void Update()
    {
        if (Items == null)
            Items = GameObject.Find("Main Camera").GetComponent<LevelPlayer>().SummonPlace.gameObject;

        if (!mapRequested & NM.IsClientConnected())
        {
            Client.Send(MsgID.AskForServerMap, new ServRequest());
            mapRequested = true;
        }
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

        MyMsgBase msg = netMsg.ReadMessage<MyMsgBase>();
        GameObject.Find("Main Camera").GetComponent<LevelPlayer>().PlayLevel(new LevelItem("Multiplayer", "", msg.map));
        Selection.SetActive(false);
    }
}
