using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particules : MonoBehaviour {

    public int RotateSpeed = 25;
    public int FallSpeed = 5;

    void Update () {
        transform.position = new Vector3(transform.position.x, transform.position.y-FallSpeed);
        transform.Rotate(new Vector3(0, 0, RotateSpeed/100F));
        if (transform.position.y < -500)
            Destroy(gameObject);
    }
}
