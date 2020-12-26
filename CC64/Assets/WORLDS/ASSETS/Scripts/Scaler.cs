using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaler : MonoBehaviour {

    public float Xspeed = 1;
    public float Yspeed = 1;
    public float Zspeed = 1;
    Vector3 temp;

	void Update () {
        temp = transform.localScale;

        temp.x += (Time.deltaTime * Xspeed);
        temp.y += (Time.deltaTime * Yspeed);
        temp.z += (Time.deltaTime * Zspeed);

        transform.localScale = temp;
		
	}
}
