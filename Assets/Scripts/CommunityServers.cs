using AngryDash.Language;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CommunityServers : MonoBehaviour
{
#pragma warning disable CS0618 // En attendant la nouvelle API

    public Sprite DefaultServerIcon;

    NetworkManager NM;
    NetworkClient Client;
    string adress = "localhost";
    int port = 20000;
    string[] fav;

    void Start()
    {
        NM = GetComponent<NetworkManager>();
        if (string.IsNullOrEmpty(ConfigAPI.GetString("online.favoriteServer")))
            ConfigAPI.SetString("online.favoriteServer", "");

        RefreshFav();
    }

    public void Join()
    {
        if (InternetAPI.ValidateIPv4(transform.GetChild(1).GetChild(0).GetComponent<InputField>().text))
        {
            NM.StopClient();
            SceneManager.LoadScene("Online", new string[] { "Connect", adress + ":" + port });
        }
    }


    #region Favorite

    public void OnValueChang(InputField IF)
    {
        if (isInFav(IF.text)) transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersRemove", "Remove");
        else transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersAdd", "Add");

        int f = whereIsItInFav(IF.text);
        Transform trans = transform.GetChild(0).GetChild(0).GetChild(0);
        for (int i = 1; i < trans.childCount; i++)
            trans.GetChild(i).GetComponent<Button>().interactable = (i - 1) != f;

        Transform ipButton = transform.GetChild(1);
        for (int i = 1; i < ipButton.childCount; i++)
        {
            bool interactable = true;
            if (i == 3 & refreshing) interactable = false;
            else interactable = InternetAPI.ValidateIPv4(IF.text);
            ipButton.GetChild(i).GetComponent<Button>().interactable = interactable;
        }


        string[] a = IF.text.Split(new string[1] { ":" }, System.StringSplitOptions.None);
        try { adress = a[0]; } catch { adress = "localhost"; }
        try { port = int.Parse(a[1]); } catch { port = 20000; }
    }
    public void OpenFavServ(int f)
    {
        if (fav.Length > f)
            transform.GetChild(1).GetChild(0).GetComponent<InputField>().text = fav[f];
    }
    public void AddToFav(InputField IF)
    {
        int f = whereIsItInFav(IF.text);

        if (f == -1) //Add
        {
            if (InternetAPI.ValidateIPv4(IF.text))
            {
                string charForSplit = ", ";
                if (fav.Length == 0)
                    charForSplit = "";

                ConfigAPI.SetString("online.favoriteServer", ConfigAPI.GetString("online.favoriteServer") + charForSplit + IF.text);
            }
        }
        else //Remove
        {
            string[] newFav = new string[fav.Length - 1];
            for (int i = 0; i < fav.Length; i++)
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
            ConfigAPI.SetString("online.favoriteServer", favString);
        }
        fav = ConfigAPI.GetString("online.favoriteServer").Split(new string[1] { ", " }, System.StringSplitOptions.None);
        RefreshFav();
        OnValueChang(IF);
    }
    bool isInFav(string ip)
    {
        bool isInFav = false;
        for (int i = 0; i < fav.Length; i++)
        {
            if (fav[i] == ip & !isInFav)
                isInFav = true;
        }
        return isInFav;
    }
    int whereIsItInFav(string ip)
    {
        int f = -1;
        for (int i = 0; i < fav.Length; i++)
        {
            if (fav[i] == ip & f == -1)
                f = i;
        }
        return f;
    }
    public void RefreshFavorite(InputField IF)
    {
        if (!refreshing)
        {
            int f = whereIsItInFav(IF.text);
            if (f > -1)
                RefreshFav(f, true);
        }
    }
    void RefreshFav(int last = -1, bool _onlyOne = false)
    {
        if (this == null) return;

        onlyOne = _onlyOne;
        Transform trans = transform.GetChild(0).GetChild(0).GetChild(0);

        if (last == -1)
        {
            if (ConfigAPI.GetString("online.favoriteServer").Length > 0)
                fav = ConfigAPI.GetString("online.favoriteServer").Split(new string[1] { ", " }, System.StringSplitOptions.None);
            else fav = new string[0];

            for (int v = 0; v < fav.Length; v++)
            {
                Transform go = null;
                if (trans.childCount > v + 1) go = trans.GetChild(v + 1);
                else go = Instantiate(trans.GetChild(0).gameObject, trans).transform;

                int btn = v;
                go.GetComponent<Button>().onClick.AddListener(() => OpenFavServ(btn));
                go.GetChild(0).GetComponent<Image>().sprite = DefaultServerIcon;
                go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersSyncing", "Syncing");
                go.GetChild(2).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersPlayersUnkown", "?/?");
                go.GetChild(3).GetComponent<Text>().text = fav[v];
                go.gameObject.SetActive(true);
            }
            for (int v = fav.Length + 1; v < trans.childCount; v++)
                Destroy(trans.GetChild(v).gameObject);
        }

        int i = last + 1;
        if (onlyOne)
            i = last;

        if (fav.Length > i)
        {
            Transform go = trans.GetChild(i + 1);
            string[] a = fav[i].Split(new string[1] { ":" }, System.StringSplitOptions.None);
            try { NM.networkAddress = a[0]; } catch { NM.networkAddress = "localhost"; }
            try { NM.networkPort = int.Parse(a[1]); } catch { NM.networkPort = 20000; }
            infoActual = i;
            Client = NM.StartClient();
            refreshing = true;
            Client.RegisterHandler(MsgType.Disconnect, InfoDisconnected);
            Client.RegisterHandler(MsgID.GetServerInfo, ServInfoRecup);

            go.GetChild(0).GetComponent<Image>().sprite = DefaultServerIcon;
            go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersSyncing", "Syncing");
            go.GetChild(2).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersPlayersUnkown", "?/?");
            go.GetChild(3).GetComponent<Text>().text = fav[i];
        }
        else NM.StopClient();

        OnValueChang(transform.GetChild(1).GetChild(0).GetComponent<InputField>());
    }
    bool refreshing = false;
    int infoActual = 0;
    bool onlyOne = false;
    void InfoDisconnected(NetworkMessage netMsg)
    {
        if (this == null) return;

        if (!onlyOne)
            RefreshFav(infoActual);

        Transform go = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(infoActual + 1);
        go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersError", "Error");
        go.GetChild(2).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersPlayersUnkown", "?/?");
        refreshing = false;
        OnValueChang(transform.GetChild(1).GetChild(0).GetComponent<InputField>());
    }
    public void ServInfoRecup(NetworkMessage netMsg)
    {
        ServInfo msg = netMsg.ReadMessage<ServInfo>();
        Transform go = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(infoActual + 1);
        Texture2D text = new Texture2D(1, 1);
        text.LoadImage(msg.icon);
        go.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(text, new Rect(0, 0, text.width, text.height), new Vector2(.5f, .5f));
        go.GetChild(1).GetComponent<Text>().text = msg.Name;
        go.GetChild(2).GetComponent<Text>().text = LangueAPI.Get("native", "PlayCommunityServersPlayers", "[0]/[1]", msg.player.ToString(), msg.maxPlayer.ToString());

        if (!onlyOne)
            RefreshFav(infoActual);
        OnValueChang(transform.GetChild(1).GetChild(0).GetComponent<InputField>());
    }

    private void Update()
    {
        if (NM.IsClientConnected() & refreshing)
        {
            Client.Send(MsgID.AskForServerInfo, new ServRequest());
            refreshing = false;
        }
    }
    #endregion
}
