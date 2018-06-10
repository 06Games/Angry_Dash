using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenControl : MonoBehaviour {
    public GameObject loadingScreenObj;
    public Slider slider; 
    AsyncOperation async;

    public void LoadScreen(string Scene) {
        loadingScreenObj = transform.GetChild(0).gameObject;
        slider = loadingScreenObj.transform.GetChild(0).GetComponent<Slider>();

        StartCoroutine(LoadingScreen(Scene));
    }

    IEnumerator LoadingScreen(string Scene) {
        loadingScreenObj.SetActive(true);
        async = SceneManager.LoadSceneAsync(Scene);
        async.allowSceneActivation = false;
        while (async.isDone == false)
        {
            slider.value = async.progress;
            if (async.progress == 0.9f)
            {
                slider.value = 1f;
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}