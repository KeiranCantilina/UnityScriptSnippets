using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeiranUtils;

public class ButtonTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickMe()
    {
        GameObject[] gameObjects = LoadOBJDialog.loadOBJ();
        foreach (GameObject g in gameObjects)
        {
            UnityEngine.Debug.Log(g.name);
        }
       
    }
}
