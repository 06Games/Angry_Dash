using System;
using System.Collections;
using AngryDash.Language;
using Discord;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Intro : MonoBehaviour
{

    public VideoClip videoToPlay;
    public GameObject Fond;

    private void Start()
    {
        Fond.SetActive(true);
        StartCoroutine(playVideo(() => StartCoroutine(PlayEnd())));

        _ = LangueAPI.LoadAsync("native", Application.persistentDataPath);
    }

    private IEnumerator playVideo(Action onComplete = null)
    {
        //Add VideoPlayer to the GameObject
        var videoPlayer = gameObject.AddComponent<VideoPlayer>();

        //Add AudioSource
        var audioSource = gameObject.AddComponent<AudioSource>();

        //Disable Play on Awake for both Video and Audio
        videoPlayer.playOnAwake = false;
        audioSource.playOnAwake = false;

        //We want to play from video clip not from url
        videoPlayer.source = VideoSource.VideoClip;

        //Set Audio Output to AudioSource
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        //Assign the Audio from Video to AudioSource to be played
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);

        //Set video To Play then prepare Audio to prevent Buffering
        videoPlayer.clip = videoToPlay;
        videoPlayer.Prepare();

        //Wait until video is prepared
        while (!videoPlayer.isPrepared & !Input.anyKey) yield return null;
        if (Input.anyKey) End();
        else
        {
            //Assign the Texture from Video to RawImage to be displayed
            var rawImage = gameObject.AddComponent<RawImage>();
            var sizeMultiplier = 1080F / Screen.height;
            transform.localScale = new Vector2(videoPlayer.texture.width / sizeMultiplier, videoPlayer.texture.height / sizeMultiplier);
            rawImage.texture = videoPlayer.texture;

            //Play Video
            videoPlayer.Play();

            //Play Sound
            audioSource.Play();

            while (videoPlayer.isPlaying & !Input.anyKey) yield return null;
            Destroy(rawImage);

            End();
        }

        void End()
        {
            Destroy(videoPlayer);
            Destroy(audioSource);
            onComplete?.Invoke();
        }
    }

    private IEnumerator PlayEnd()
    {
        yield return new WaitForEndOfFrame();
        _06Games.Account.Discord.NewActivity(new Activity { State = LangueAPI.Get("native", "discordStarting_title", "Starting the game"), Assets = new ActivityAssets { LargeImage = "default" } });
        Fond.SetActive(false);

        var DM = GameObject.Find("Dependencies").GetComponent<DependenciesManager>();
        var social = FindObjectOfType<Social>();
        yield return DM.DownloadRPs(() => StartCoroutine(DM.DownloadLevels(social.NewStart)), null);
    }
}
