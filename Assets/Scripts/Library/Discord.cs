using Discord;
using System;
using UnityEngine;
using discord = Discord.Discord;

namespace DiscordAPI
{
    public class Discord : MonoBehaviour
    {
        public static discord discord;
        static ActivityManager activityManager;

        /// <summary>Update the Discord Activity</summary>
        /// <param name="state">Title</param>
        /// <param name="detail">Subtitle</param>
        /// <param name="image">Image</param>
        /// <param name="icon">Icon</param>
        /// <param name="remainingTime">Time remaining before the end of the game</param>
        /// <param name="startTime">Start date of the game</param>
        public static void NewActivity(string state, string detail = null, Img image = null, Img icon = null, TimeSpan remainingTime = default, DateTime startTime = default)
        {
#if UNITY_EDITOR
            if (GameObject.Find("Discord") == null) new GameObject("Discord").AddComponent<Discord>();
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
            Activity presence = new Activity();
            presence.State = state; //State
            presence.Details = detail; //Detail

            //Image
            presence.Assets.LargeImage = image?.identifier ?? "default";
            presence.Assets.LargeText = image?.caption;

            //Icon
            presence.Assets.SmallImage = icon?.identifier;
            presence.Assets.SmallText = icon?.caption;

            //Remaining Time
            if (remainingTime != default) presence.Timestamps.End = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + remainingTime.TotalSeconds);
            else presence.Timestamps.End = 0;

            //Start Date
            if (startTime != default) presence.Timestamps.Start = Convert.ToInt64((startTime - new DateTime(1970, 1, 1)).TotalSeconds);
            else presence.Timestamps.Start = 0;

            activityManager?.UpdateActivity(presence, (res) =>
            {
                if (res == Result.Ok) Logging.Log("Discord activity updated", LogType.Log);
                else Logging.Log("Failed to update the discord activity, error: " + res, LogType.Warning);
            });
#endif
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
            if (discord == null)
            {
                Logging.Log("Discord API is starting", LogType.Log);
                discord = new discord(470264480786284544, (ulong)CreateFlags.Default);
                activityManager = discord.GetActivityManager();
            }
        }
        void OnDisable() { discord.Dispose(); }
        void Update() { discord.RunCallbacks(); }
#endif
    }

    /// <summary>Discord image previously created at the address https://discordapp.com/developers/applications/ </summary>
    public class Img
    {
        /// <summary>Invoke a discord image</summary>
        /// <param name="id">The image identifier</param>
        /// <param name="cap">The image caption</param>
        public Img(string id = null, string cap = null) { identifier = id; caption = cap; }

        /// <summary>The image identifier</summary>
        public string identifier;
        /// <summary>The image caption</summary>
        public string caption;
    }
}
