using Discord;
using DiscordClasses;
using System;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    public static Discord.Discord discord;
    static ActivityManager activityManager;

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
    public static void UpdatePresence(string state, string detail = null, Img lImage = null, Img sImage = null, TimeSpan remainingTime = default, DateTime startTime = default)
    {
#if UNITY_EDITOR
        GameObject go = GameObject.Find("Discord");
        if (go == null) new GameObject("Discord").AddComponent<DiscordController>();
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
        Activity presence = new Activity();
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
        else presence.Assets.SmallText = presence.Assets.SmallImage = null;

        //End Timestamp
        if (remainingTime != default) presence.Timestamps.End = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + remainingTime.TotalSeconds);
        else presence.Timestamps.End = 0;

        //Start Timestamp
        if (startTime != default) presence.Timestamps.Start = Convert.ToInt64((startTime - new DateTime(1970, 1, 1)).TotalSeconds);
        else presence.Timestamps.Start = 0;

        activityManager?.UpdateActivity(presence, (res) =>
        {
            if (res == Result.Ok) Logging.Log("Discord activity updated", LogType.Log);
        });
    }

    void OnEnable()
    {
        DontDestroyOnLoad(gameObject);

        if (discord == null)
        {
            Logging.Log("Discord API is starting", LogType.Log);
            discord = new Discord.Discord(470264480786284544, (ulong)CreateFlags.Default);
            activityManager = discord.GetActivityManager();
        }
    }

    void OnDisable()
    {
        discord.Dispose();
    }

    void Update()
    {
        discord.RunCallbacks();
#endif
    }
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
