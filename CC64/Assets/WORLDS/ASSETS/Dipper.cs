using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dipper : MonoBehaviour
{
    public float yspeed = 0.01f;
    public int target = 100;
    public int counter = 0;
    public int state = 0;
    public bool triggered = false;
    // 0 = static, 1 = down, 2 = up;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if (state == 1)
        {
            if (counter < target)
            {
                transform.Translate(0f, -yspeed, 0f);
                counter += 1;
            }
            if (triggered == false)
            {
                state = 2;
            }

        }
        if (state == 2)
        {
            if (counter > 0)
            {
                transform.Translate(0f, yspeed, 0f);
                counter -= 1;
            }

            if (triggered == true)
            {
                state = 1;
            }

        }

    }


}
