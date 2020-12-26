using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaChanger : MonoBehaviour
{
    public float startalpha = 1;
    public float fadeSpeed = 0.01f;
    public bool triggered = false;
    public GameObject activate;
    public float countdown = 5;

    void Start()
    {
        this.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (triggered == true)
        {
            if (countdown > 0)
            {
                countdown -= 1;
            }

            if (countdown <= 0) 
            { 
            if (startalpha > 0)
            {
                this.GetComponent<MeshRenderer>().enabled = true;
                Color color = this.GetComponent<MeshRenderer>().material.color;
                color.a = startalpha;
                this.GetComponent<MeshRenderer>().material.color = color;
                startalpha -= fadeSpeed;
                activate.gameObject.SetActive(true);
            }

            if (startalpha <= 0)
            {
                Destroy(gameObject);
            }
            }
        }
    }
}
