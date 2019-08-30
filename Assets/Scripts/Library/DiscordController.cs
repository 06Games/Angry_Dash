using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AngryDash.Language;
using Discord;
using DiscordClasses;

public class DiscordController : MonoBehaviour
{
    public Discord.Discord discord;
    public ActivityManager activityManager;

    /// <summary>
    /// Update the Discord RPC Presence
    /// </summary>
    /// <param name="state">Titre 1</param>
    /// <param name="detail">Titre 2 (null pour le désactiver)</param>
    /// <param name="lImage">Image 1 (null pour garder l'image actuelle)</param>
    /// <param name="sImage">Image 2 (null pour désactiver)</param>
    /// <param name="remainingTime">Temps restant avant la fin de la partie (-2 keep, -1 pour le désactiver, 0 pour actuelement)</param>
    /// <param name="startTime">Temps en seconde depuis le démarage de la partie (-2 keep, -1 pour le désactiver, 0 pour actuelement)</param>
    /// <param name="minPartySize">Taille actuelle de la partie, du lobby ou du groupe (-1 pour le désactiver)</param>
    /// <param name="maxPartySize">Taille maximale de la partie, du lobby ou du groupe (-1 pour le désactiver)</param>
    public static void Presence(string state, string detail = null, Img lImage = null, Img sImage = null, int remainingTime = -1, long startTime = -1, int minPartySize = -1, int maxPartySize = -1)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        GameObject go = GameObject.Find("Discord");
        if (go != null) go.GetComponent<DiscordController>().UpdatePresence(state, detail, lImage, sImage, remainingTime, startTime);
#endif
    }

#if UNITY_STANDALONE || UNITY_EDITOR
    Activity presence;
    void UpdatePresence(string detail, string state, Img lImage, Img sImage, int remainingTime, long startTime)
    {
        presence.State = state; //State
        presence.Details = detail; //Detail

        //Large Image
        if (lImage != null)
        {
            presence.Assets.LargeImage = lImage.key;
            presence.Assets.LargeText = lImage.legende;
        }

        //Small Image
        if (sImage != null)
        {
            presence.Assets.SmallImage = sImage.key;
            presence.Assets.SmallText = sImage.legende;
        }
        else
        {
            presence.Assets.SmallImage = null;
            presence.Assets.SmallText = null;
        }

        //End Timestamp
        if (remainingTime >= 0)
            presence.Timestamps.End = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) + remainingTime;
        else if (remainingTime == -1)
            presence.Timestamps.End = 0;

        //Start Timestamp
        if (startTime == 0)
            presence.Timestamps.Start = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        else if (startTime > 0)
            presence.Timestamps.Start = startTime;
        else if (startTime == -1)
            presence.Timestamps.Start = 0;

        activityManager.UpdateActivity(presence, (res) =>
        {
            if (res == Result.Ok) Logging.Log("Discord activity updated", LogType.Log);
        });
    }

    void OnEnable()
    {
        Logging.Log("Discord API is starting", LogType.Log);
        DontDestroyOnLoad(gameObject);

        discord = new Discord.Discord(470264480786284544, (ulong)CreateFlags.Default);
        activityManager = discord.GetActivityManager();
        Presence(LangueAPI.Get("native", "discordStarting_title", "Starting the game"), "", new Img("default"));
    }

    void OnDisable()
    {
        discord.Dispose();
    }

    void Update()
    {
        discord.RunCallbacks();
    }
#endif
}

namespace DiscordClasses
{
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
