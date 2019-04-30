using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.SceneManagement;

public class Coordinator : MonoBehaviour
{
    public GameObject[] coordinatedObjects;

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Title1")
        {
            if (Input.GetMouseButtonDown(0)){
                FunctionTimer.Create(() => coordinatedObjects[0].GetComponent<Animator>().SetTrigger("LabelExit") , 0.0f);
                FunctionTimer.Create(() => coordinatedObjects[1].GetComponent<Animator>().SetTrigger("PopUP"), 0.25f);
                FunctionTimer.Create(() => coordinatedObjects[2].GetComponent<Animator>().SetTrigger("PopUP"), 1.25f);
                FunctionTimer.Create(() => coordinatedObjects[3].GetComponent<Animator>().SetTrigger("PopUP"), 2.25f);
            }
        }else if (SceneManager.GetActiveScene().name == "Title2")
        {
            if (Input.GetMouseButtonDown(0))
            {
                FunctionTimer.Create(() => coordinatedObjects[0].GetComponent<Animator>().SetTrigger("LabelExit2"), 0.0f);
                FunctionTimer.Create(() => coordinatedObjects[1].GetComponent<Animator>().SetTrigger("PopUP2"), 0.25f);
            }
        }
    }
}
