using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerAppear : MonoBehaviour
{
    public GameObject target;
    public int counter;

    void Update()
    {
        counter -= 1;

        if (counter <= 0)
        {
            target.gameObject.SetActive(true);
        }

    }


}