using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    public bool switched = false;
    public float speed = 1.0f;
    public Transform fixorigin;
    public Transform target;
    public float lookatspeed = 1.0f;
    public Transform lookat;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown("q")){
            switched = !switched;
        }

        if (switched == true) 
        { 
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            // Determine which direction to rotate towards
            Vector3 targetDirection = lookat.position - transform.position;

            // The step size is equal to speed times frame time.
            float singleStep = lookatspeed * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            // Draw a ray pointing at our target in
            Debug.DrawRay(transform.position, newDirection, Color.red);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        if (switched == false)
        {
            transform.position = fixorigin.position;
            transform.rotation = fixorigin.rotation;
        }

    }

}
