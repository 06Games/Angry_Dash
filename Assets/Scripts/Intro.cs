﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Intro : MonoBehaviour {

    public VideoClip videoToPlay;
    [System.Serializable] public class OnCompleteEvent : UnityEngine.Events.UnityEvent { }

    void Start () {
        OnCompleteEvent onComplete = new OnCompleteEvent();
        onComplete.AddListener(() => PlayEnd());
        StartCoroutine(playVideo(onComplete));
	}

    IEnumerator playVideo(OnCompleteEvent onComplete = null)
    {
        //Add VideoPlayer to the GameObject
        VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();

        //Add AudioSource
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

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
        while (!videoPlayer.isPrepared) yield return null;

        //Assign the Texture from Video to RawImage to be displayed
        UnityEngine.UI.RawImage rawImage = gameObject.AddComponent<UnityEngine.UI.RawImage>();
        rawImage.texture = videoPlayer.texture;

        //Play Video
        videoPlayer.Play();

        //Play Sound
        audioSource.Play();
        
        while (videoPlayer.isPlaying) yield return null;

        Destroy(videoPlayer);
        Destroy(audioSource);
        Destroy(rawImage);

        if(onComplete != null) onComplete.Invoke();
    }

    void PlayEnd()
    {
        GameObject.Find("Dependencies").GetComponent<DependenciesManager>().DownloadDefaultsRP();
    }
}