using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ParticulesManager : MonoBehaviour {

    public float spawnSpeed;
    public GameObject Prefab;
    public Sprite[] Sp;
    public int[] sizeRange;

    private void Start()
    {
        /* //En cas d'erreur de Pading
        PreviewLabs.PlayerPrefs.DeleteAll();
        PreviewLabs.PlayerPrefs.Flush();
        */

        for (int i = 0; i < 5; i++)
            Particule();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            Particule();
            yield return new WaitForSeconds(spawnSpeed);
        }
    }

    void Particule()
    {
        System.Random rnd = new System.Random();

        Vector2 pos = new Vector2(rnd.Next(0, Screen.width), Screen.height + sizeRange[1]);
        GameObject go = Instantiate(Prefab, pos, new Quaternion(), transform);
        float scale = rnd.Next(sizeRange[0], sizeRange[1]);
        go.transform.localScale = new Vector2(scale, scale);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, 0, rnd.Next(0, 360));
        go.transform.rotation = rot;
        go.GetComponent<Image>().sprite = Sp[rnd.Next(0, Sp.Length - 1)];

        Particules pa = go.GetComponent<Particules>();
        int sens = rnd.Next(0, 1);
        if (sens == 0)
            sens = -1;
        pa.RotateSpeed = rnd.Next(25, 50) * sens;
        pa.FallSpeed = rnd.Next(5, 8);
    }
}
