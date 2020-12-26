using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScalerByDistance: MonoBehaviour {


    public float MinScale = 1;
    public float MaxScale = 3;
    public float MaxDistance = 25;

    public Transform target;
    Vector3 temp;





	void Update () {

       
        temp = transform.localScale;
        float dist = Vector3.Distance(target.position, transform.position);

        if (dist < MaxDistance)
        {
            temp.x = ((MaxDistance - dist) / (MaxDistance / (MaxScale - MinScale))) + MinScale;
            temp.y = ((MaxDistance - dist) / (MaxDistance / (MaxScale - MinScale))) + MinScale;
            temp.z = ((MaxDistance - dist) / (MaxDistance / (MaxScale - MinScale))) + MinScale;

       transform.localScale = temp;
        }

        if (dist >= MaxDistance)
        {
            temp.x = MinScale;
            temp.y = MinScale;
            temp.z = MinScale;

            transform.localScale = temp;
        }

}
}