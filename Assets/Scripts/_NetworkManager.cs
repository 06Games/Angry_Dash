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

public class _NetworkManager : NetworkBehaviour
{

    public GameObject Selection;
    
	public void StartHost()
    {
        GetComponent<NetworkManager>().StartHost();
        StartData(true);
        Selection.SetActive(false);
    }

    public void Join()
    {
        GetComponent<NetworkManager>().StartClient();
        StartData(false);
        Selection.SetActive(false);
    }

    public void Disconnect()
    {
        GetComponent<NetworkManager>().StopHost();
        GetComponent<NetworkManager>().StopClient();
        Selection.SetActive(true);
        isSetup = false;
        player = 0;
    }

    public const short MessageType = MsgType.Highest + 1;
    private bool isSetup = false;
    private bool isClientInitializated = false;
    private NetworkClient Client;
    
    public void StartData(bool serv)
    {
        if (!isSetup)
            SetupClient(serv);
    }
    public override void OnStartClient()
    {
        isClientInitializated = true;
    }
    public MyMsgBase m_Message;
    public void SetupClient(bool server)
    {
        player = 0;
        NetworkManager net = GetComponent<NetworkManager>();
        Client = net.client;
        Client.RegisterHandler(MessageType, OnConnected);
        
        if (server)
        {
            //NetworkServer.RegisterHandler(MsgType.AddPlayer, OnPlayer);
            m_Message = new MyMsgBase();
            m_Message.map = "Blocks {\n1.0; (0.0, 0.0, 0.0); 0; FF0000255; 0\n1.0; (0.0, 5.0, 0.0); 0; FF0000255; 0\n}";
        }
        isSetup = true;
    }

    int player = 0;
    private void Update()
    {
        if(GetComponent<NetworkManager>().numPlayers != player)
        {
            NetworkServer.SendToAll(MessageType, m_Message);
            player = GetComponent<NetworkManager>().numPlayers;
        }
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        MyMsgBase msg = netMsg.ReadMessage<MyMsgBase>();
        string[] map = msg.map.Split(new string[1] { "\n" }, System.StringSplitOptions.None);
        GameObject.Find("Main Camera").GetComponent<LevelPlayer>().MapData(map);
    }
}
