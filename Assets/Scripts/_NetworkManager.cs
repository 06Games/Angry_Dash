using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

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
    public Sprite DefaultServerIcon;
    public GameObject Items;

    public LoadingScreenControl LSC;

    NetworkManager NM;

    string adress = "localhost";
    int port = 7777;
    string[] fav;

    public void OnValueChang(InputField IF)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (isInFav(IF.text))
            Selection.transform.GetChild(2).GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = LangueAPI.String("multiplayerRemove");
        else Selection.transform.GetChild(2).GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = LangueAPI.String("multiplayerAdd");

        string[] a = IF.text.Split(new string[1] { ":" }, System.StringSplitOptions.None);
        try { adress = a[0]; } catch { adress = "localhost"; }
        try { port = int.Parse(a[1]); } catch { port = 20000; }
    }
    public void OpenFavServ(int f)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (fav.Length > f)
            Selection.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<InputField>().text = fav[f];
    }
    public void AddToFav(InputField IF)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        int f = whereIsItInFav(IF.text);

        if (f == -1) //Add
        {
            if (fav.Length < 4 & InternetAPI.ValidateIPv4(IF.text))
            {
                string charForSplit = ", ";
                if (fav.Length == 0)
                    charForSplit = "";

                ConfigAPI.SetString("online.favoriteServer", ConfigAPI.GetString("online.favoriteServer") + charForSplit + IF.text);
            }
        }
        else //Remove
        {
            string[] newFav = new string[fav.Length-1];
            for(int i = 0; i < 4 & i < fav.Length; i++)
            {
                if (i < f)
                    newFav[i] = fav[i];
                else if (i > f)
                    newFav[i - 1] = fav[i];
            }

            string favString = "";
            if (newFav.Length > 0)
                favString = newFav[0];
            for (int i = 1; i < newFav.Length; i++)
                favString = favString + ", " + newFav[i];
            ConfigAPI.SetString("online.favoriteServer",favString);
        }
        fav = ConfigAPI.GetString("online.favoriteServer").Split(new string[1] { ", " }, System.StringSplitOptions.None);
        RefreshFav();
        OnValueChang(IF);
    }
    bool isInFav(string ip)
    {
        bool isInFav = false;
        for (int i = 0; i < 4 & i < fav.Length; i++)
        {
            if (fav[i] == ip & !isInFav)
                isInFav = true;
        }
        return isInFav;
    }
    int whereIsItInFav(string ip)
    {
        int f = -1;
        for (int i = 0; i < 4 & i < fav.Length; i++)
        {
            if (fav[i] == ip & f == -1)
                f = i;
        }
        return f;
    }
    public void RefreshFavorite(InputField IF)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (!refreshing)
        {
            int f = whereIsItInFav(IF.text);
            if (f > -1)
                RefreshFav(f, true);
        }
    }
    void RefreshFav(int last = -1, bool _onlyOne = false)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        onlyOne = _onlyOne;
        Transform trans = Selection.transform.GetChild(2).GetChild(0).GetChild(1);

        if (last == -1)
        {
            if (ConfigAPI.GetString("online.favoriteServer").Length > 0)
                fav = ConfigAPI.GetString("online.favoriteServer").Split(new string[1] { ", " }, System.StringSplitOptions.None);
            else fav = new string[0];

            for (int v = 0; v < 4; v++)
            {
                Transform go = trans.GetChild(v);
                go.gameObject.SetActive(true);
                go.GetComponent<Button>().interactable = fav.Length > v;
                for (int c = 0; c < go.childCount; c++)
                    go.GetChild(c).gameObject.SetActive(fav.Length > v);
                if (v < fav.Length)
                {
                    go.GetChild(0).GetComponent<Image>().sprite = DefaultServerIcon;
                    go.GetChild(1).GetComponent<Text>().text = LangueAPI.String("multiplayerSyncing");
                    go.GetChild(2).GetComponent<Text>().text = LangueAPI.String("multiplayerPlayersUnkown");
                    go.GetChild(3).GetComponent<Text>().text = fav[v];
                }
            }
        }

        int i = last+1;
        if (onlyOne)
            i = last;

        if (i < 4)
        {
            Transform go = trans.GetChild(i);
            if (fav.Length > i)
            {
                string[] a = fav[i].Split(new string[1] { ":" }, System.StringSplitOptions.None);
                try { NM.networkAddress = a[0]; } catch { NM.networkAddress = "localhost"; }
                try { NM.networkPort = int.Parse(a[1]); } catch { NM.networkPort = 20000; }
                infoActual = i;
                Client = NM.StartClient();
                refreshing = true;
                Client.RegisterHandler(MsgType.Disconnect, InfoDisconnected);
                Client.RegisterHandler(MsgID.GetServerInfo, ServInfoRecup);

                go.GetChild(0).GetComponent<Image>().sprite = DefaultServerIcon;
                go.GetChild(1).GetComponent<Text>().text = LangueAPI.String("multiplayerSyncing");
                go.GetChild(2).GetComponent<Text>().text = LangueAPI.String("multiplayerPlayersUnkown");
                go.GetChild(3).GetComponent<Text>().text = fav[i];
            }
            else NM.StopClient();
        }
        else NM.StopClient();

        Selection.transform.GetChild(2).GetChild(1).GetChild(3).GetComponent<Button>().interactable = !refreshing;
    }
    bool refreshing = false;
    int infoActual = 0;
    bool onlyOne = false;
    void InfoDisconnected(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (!onlyOne)
            RefreshFav(infoActual);

        Transform go = Selection.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(infoActual);
        go.GetChild(1).GetComponent<Text>().text = LangueAPI.String("multiplayerError");
        go.GetChild(2).GetComponent<Text>().text = LangueAPI.String("multiplayerPlayersUnkown");
        refreshing = false;
        Selection.transform.GetChild(2).GetChild(1).GetChild(3).GetComponent<Button>().interactable = !refreshing;
    }
    public void ServInfoRecup(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        ServInfo msg = netMsg.ReadMessage<ServInfo>();
        Transform go = Selection.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(infoActual);
        Texture2D text = new Texture2D(1, 1);
        text.LoadImage(msg.icon);
        go.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(text, new Rect(0, 0, text.width, text.height), new Vector2(.5f, .5f));
        go.GetChild(1).GetComponent<Text>().text = msg.Name;
        go.GetChild(2).GetComponent<Text>().text = LangueAPI.StringWithArgument("multiplayerPlayers", new string[2] { msg.player.ToString(), msg.maxPlayer.ToString() });

        if (!onlyOne)
            RefreshFav(infoActual);
        Selection.transform.GetChild(2).GetChild(1).GetChild(3).GetComponent<Button>().interactable = !refreshing;
    }


    private void Start()
    {
        NM = GetComponent<NetworkManager>();

        if (string.IsNullOrEmpty(ConfigAPI.GetString("online.favoriteServer")))
            ConfigAPI.SetString("online.favoriteServer", "");
        
        string[] launchArgs = LSC.GetArgs();
        if (launchArgs == null) launchArgs = new string[0];
        if (launchArgs.Length > 0)
        {
            string[] param = launchArgs[0].Split(new string[] { ":" }, StringSplitOptions.None);
            adress = param[0];
            port = int.Parse(param[1]);
            
            Join();
        }
        else RefreshFav();

        Client.RegisterHandler(MsgID.GetServerMap, OnConnected);
    }

    public void StartHost()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        NM.StartHost();
        StartData(true);

        Discord.Presence(LangueAPI.String("discordServer_title"), "", new DiscordClasses.Img("default"), null, -1, 0);
    }

    public void Join()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        NM.networkAddress = adress;
        NM.networkPort = port;
        NetworkClient NC = NM.StartClient();
        Selection.GetComponent<CreatorManager>().ChangArray(3);
        NC.RegisterHandler(MsgType.Disconnect, Disconnected);
        NC.RegisterHandler(MsgType.Error, Error);
        StartData(false);
        mapRequested = false;

        Discord.Presence(LangueAPI.String("discordServer_title"), "", new DiscordClasses.Img("default",LangueAPI.StringWithArgument("discordServer_caption", new string[] { adress, port.ToString() })), null, -1, 0);
        Recent.LvlPlayed(adress + ":" + port, "S", "");
    }

    void Error(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        print("Client connection error !");
    }

    void Disconnected(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (Selection.GetComponent<CreatorManager>().array == 3)
            Selection.GetComponent<CreatorManager>().ChangArray(2);
        else if (netMsg.msgType == 33)
            Disconnect();
        else print("Client disconnected, error code " + netMsg.msgType);
    }

    public void Disconnect()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        NM.StopHost();
        NM.StopClient();
        Selection.GetComponent<CreatorManager>().ChangArray(0);
        Selection.SetActive(true);
        isSetup = false;
        player = 0;

        for (int i = 0; i < Items.transform.childCount; i++)
            Destroy(Items.transform.GetChild(i).gameObject);

        Discord.Presence(LangueAPI.String("discordHome_title"), "", new DiscordClasses.Img("default"));
    }

    private bool isSetup = false;
    private NetworkClient Client;
    
    public void StartData(bool serv)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (!isSetup)
            SetupClient(serv);
    }
    public MyMsgBase m_Message;
    public void SetupClient(bool server)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        player = 0;
        NetworkManager net = NM;
        Client = net.client;
        Client.RegisterHandler(MsgID.GetServerMap, OnConnected);
        
        if (server)
        {
            m_Message = new MyMsgBase();
            m_Message.map = "Blocks {\n}";
        }
        isSetup = true;
        serv = server;
    }

    int player = 0;
    bool serv = false;
    bool mapRequested = true;
    private void Update()
    {
        if (Items == null)
            Items = GameObject.Find("Main Camera").GetComponent<LevelPlayer>().SummonPlace.gameObject;

        if (NM.numPlayers != player & serv)
        {
            NetworkServer.SendToAll(MsgID.GetServerMap, m_Message);
            player = NM.numPlayers;
        }

        if(NM.IsClientConnected() & refreshing)
        {
            Client.Send(MsgID.AskForServerInfo, new ServRequest());
            refreshing = false;
        }

        if (!mapRequested & NM.IsClientConnected())
        {
            Client.Send(MsgID.AskForServerMap, new ServRequest());
            mapRequested = true;
            //Selection.GetComponent<CreatorManager>().array = 2;
        }
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online") return;

        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

        MyMsgBase msg = netMsg.ReadMessage<MyMsgBase>();
        string[] map = msg.map.Split(new string[1] { "\n" }, System.StringSplitOptions.None);
        GameObject.Find("Main Camera").GetComponent<LevelPlayer>().MapData(map);
        Selection.SetActive(false);
    }
}
