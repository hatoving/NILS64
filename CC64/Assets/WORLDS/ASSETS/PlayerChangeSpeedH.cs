using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChangeSpeedH : MonoBehaviour
{

    public PlayerController cont;
    public float target;
    public float decrease;
    public bool switched;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("h"))
        {
            switched = !switched;
        }

        if (switched == true)
        {
            cont.movement.forwardSpeed = target;
            target -=decrease;
        }
    }
}
