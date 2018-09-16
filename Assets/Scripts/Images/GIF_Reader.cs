﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GIF_Reader : MonoBehaviour {

    public Sprite[] Frames;
    public float FramesPerSeconds = 1;

    Image Im;
    int Frame;

    private void Start()
    {
        Im = GetComponent<Image>();
        StartGIF();
    }

    public void StartGIF() { gameObject.SetActive(false); transform.parent.gameObject.SetActive(true); gameObject.SetActive(true);  Frame = 0; StartCoroutine(GIF()); }
    public void StopGIF() { StopCoroutine(GIF()); Frame = -1; transform.parent.gameObject.SetActive(false); }

    IEnumerator GIF()
    {
        if (Frame < Frames.Length - 1)
            Frame = Frame + 1;
        else if(Frame != -1)
            Frame = 0;

        float Speed = 1F / FramesPerSeconds;
        yield return new WaitForSeconds(Speed);
        Im.sprite = Frames[Frame];
        StartCoroutine(GIF());
    }
}
