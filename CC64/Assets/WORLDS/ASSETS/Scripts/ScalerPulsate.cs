using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalerPulsate : MonoBehaviour {

    public float Xspeed = 1;
    public float Yspeed = 1;
    public float Zspeed = 1;
    Vector3 temp;

	void Update () {
        temp = transform.localScale;

        temp.x += (Mathf.Sin(Time.deltaTime) * Xspeed);
        temp.y += (Time.deltaTime * Mathf.Sin(Yspeed));
        temp.z += (Time.deltaTime * Mathf.Sin(Zspeed));

        transform.localScale = temp;
		
	}
}
