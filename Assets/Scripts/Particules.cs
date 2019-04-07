using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Particules : MonoBehaviour
{

    public int RotateSpeed = 25;
    public int FallSpeed = 5;
    public Vector2 FallDirection = new Vector3(0, 1);
    public bool Static;
    public int Mode = 1;

    void Start() { StartCoroutine(Move()); }
    IEnumerator Move()
    {
        if (!Static)
            transform.Translate(new Vector2(FallDirection.x * FallSpeed, FallDirection.y * FallSpeed));
        else transform.position = new Vector3(transform.position.x, transform.position.y - FallSpeed);
        transform.Rotate(new Vector3(0, 0, RotateSpeed / 100F));
        if (transform.position.y < -500)
            Destroy(gameObject);

        if (Mode == 2)
        {
            HsvColor hsv = HSVUtil.ConvertRgbToHsv(GetComponent<Image>().color);

            if (hsv.S == 0) hsv.S = 1;
            if (hsv.H < 360) hsv.H = hsv.H + 1;
            else hsv.H = 0;

            GetComponent<Image>().color = HSVUtil.ConvertHsvToRgb(hsv, 255);
        }

        yield return new WaitForSeconds(1 / 60F);
        StartCoroutine(Move());
    }
}
