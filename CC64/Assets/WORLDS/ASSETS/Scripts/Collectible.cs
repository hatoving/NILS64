using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{

    [SerializeField] private Animator myAnimationController;
    [SerializeField] private GameObject Shadow;
    public float Xspeed = 1;
    public float Yspeed = 1;
    public float Zspeed = 1;
    Vector3 temp;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            myAnimationController.SetBool("collected", true);
            Shadow.SetActive(false);

            temp = transform.localScale;

            temp.x += (Time.deltaTime * Xspeed);
            temp.y += (Time.deltaTime * Yspeed);
            temp.z += (Time.deltaTime * Zspeed);

            transform.localScale = temp;
        }

    }

    void Update()
    {
        if (myAnimationController.GetCurrentAnimatorStateInfo(0).IsName("CoinSparkleOver"))
        {
            gameObject.SetActive(false);
        }


}
}


