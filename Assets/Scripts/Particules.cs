using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particules : MonoBehaviour {

    public int RotateSpeed = 25;
    public int FallSpeed = 5;
    public Vector2 FallDirection = new Vector3(0, 1);
    public bool Static;

    void Update () {
        if(!Static)
            transform.Translate(new Vector2(FallDirection.x * FallSpeed, FallDirection.y * FallSpeed));
        else transform.position = new Vector3(transform.position.x, transform.position.y-FallSpeed);
        transform.Rotate(new Vector3(0, 0, RotateSpeed/100F));
        if (transform.position.y < -500)
            Destroy(gameObject);
    }
}
