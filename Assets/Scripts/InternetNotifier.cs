using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InternetNotifier : MonoBehaviour
{
    bool connected;
    void Awake() { connected = InternetAPI.IsConnected(); }

    void Update()
    {
        if (connected != InternetAPI.IsConnected())
        {
            connected = InternetAPI.IsConnected();
            if (connected)
            {
                transform.GetChild(0).GetComponent<UImage_Reader>().SetID("native/GUI/other/internetNotification/connected").Load();
                transform.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "internet.connected", "You are now connected to the internet");
            }
            else
            {
                transform.GetChild(0).GetComponent<UImage_Reader>().SetID("native/GUI/other/internetNotification/disconnected").Load();
                transform.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "internet.disconnected", "You are now offline");
            }
            StartCoroutine(Notify());
        }
    }

    IEnumerator Notify()
    {
        int goal = -50 + Screen.height;
        for (int i = 0; i <= 1; i++)
        {
            float pos = transform.position.y;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool LastFrame = true;
            System.TimeSpan totalTime = System.TimeSpan.FromMilliseconds(250);
            while (sw.Elapsed < totalTime | LastFrame)
            {
                float Time = sw.ElapsedMilliseconds;
                if (sw.Elapsed >= totalTime)
                {
                    LastFrame = false;
                    Time = (long)totalTime.TotalMilliseconds;
                }
                float totalDist = goal - pos;
                float doneDist = transform.position.y - pos;
                float wantedDist = totalDist / (float)totalTime.TotalMilliseconds * Time;
                float mvt = wantedDist - doneDist;
                transform.position += new Vector3(0, mvt);
                yield return new WaitForEndOfFrame();
            }
            sw.Stop();

            yield return new WaitForSeconds(5);
            goal = 50 + Screen.height;
        }
    }
}
