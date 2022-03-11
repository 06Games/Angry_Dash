using System;
using System.Collections;
using System.Diagnostics;
using AngryDash.Image.Reader;
using AngryDash.Language;
using UnityEngine;
using UnityEngine.UI;

public class InternetNotifier : MonoBehaviour
{
    private bool connected;
    private void Awake() { connected = InternetAPI.IsConnected(); }

    private void Update()
    {
        if (connected != InternetAPI.IsConnected())
        {
            connected = InternetAPI.IsConnected();
            if (connected)
            {
                transform.GetChild(0).GetComponent<UImage_Reader>().SetID("native/GUI/other/internetNotification/connected").LoadAsync();
                transform.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "internet.connected", "You are now connected to the internet");
            }
            else
            {
                transform.GetChild(0).GetComponent<UImage_Reader>().SetID("native/GUI/other/internetNotification/disconnected").LoadAsync();
                transform.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "internet.disconnected", "You are now offline");
            }
            StartCoroutine(Notify());
        }
    }

    private IEnumerator Notify()
    {
        var goal = -50 + Screen.height;
        for (var i = 0; i <= 1; i++)
        {
            var pos = transform.position.y;
            var sw = new Stopwatch();
            sw.Start();
            var LastFrame = true;
            var totalTime = TimeSpan.FromMilliseconds(250);
            while (sw.Elapsed < totalTime | LastFrame)
            {
                float Time = sw.ElapsedMilliseconds;
                if (sw.Elapsed >= totalTime)
                {
                    LastFrame = false;
                    Time = (long)totalTime.TotalMilliseconds;
                }
                var totalDist = goal - pos;
                var doneDist = transform.position.y - pos;
                var wantedDist = totalDist / (float)totalTime.TotalMilliseconds * Time;
                var mvt = wantedDist - doneDist;
                transform.position += new Vector3(0, mvt);
                yield return new WaitForEndOfFrame();
            }
            sw.Stop();

            yield return new WaitForSeconds(5);
            goal = 50 + Screen.height;
        }
    }
}
