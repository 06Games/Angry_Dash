using UnityEngine;
using System;
using UnityEngine.Events;

public class SurClique : MonoBehaviour {
    
    [SerializeField] private OnCompleteEvent OnCompleteMethods;
    public enum QuelClique
    {
        Gauche,
        Droit
    }
    bool clic;

    public QuelClique SurQuelClique;
	void Update () {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (SurQuelClique == QuelClique.Gauche)
            clic = Input.GetKey(KeyCode.Mouse0);
        else clic = Input.GetKey(KeyCode.Mouse1);

        if (clic)
            OnCompleteMethods.Invoke();
#elif UNITY_ANDROID || UNITY_IOS
        if (SurQuelClique == QuelClique.Gauche)
            SimpleGesture.OnShortTap(Tap);
        else SimpleGesture.OnLongTap(Tap);
#endif
    }

    public void Tap() { OnCompleteMethods.Invoke(); }

    [Serializable]public class OnCompleteEvent : UnityEvent { }
}
