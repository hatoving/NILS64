using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerMoveToTarget2 : MonoBehaviour
{
    public GameObject target;
    public int counter;

    void Update()
    {
        counter -= 1;

        if (counter <= 0)
        {
            target.GetComponent<MoveToTarget2>().switched = true;

        }

    }


}