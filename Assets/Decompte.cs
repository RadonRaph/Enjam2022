using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decompte : MonoBehaviour
{

public GameObject Three;
public GameObject Two;
public GameObject One;
public GameObject Go;

public bool decompte = true;
float time = 1;
int chiffre = 3;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (decompte)
        {
            time -= Time.deltaTime;

            if(time <= 0 && chiffre == 3)
            {
                chiffre = 2;
                time = 1;
            }

            if(time <= 0 && chiffre == 2)
            {
                chiffre = 1;
                time = 1;
            }

            if(time <= 0 && chiffre == 1)
            {
                chiffre = 0;
                time = 1;
                decompte = false;
            }
        }
    }
}
