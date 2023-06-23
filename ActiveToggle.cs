using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveToggle : MonoBehaviour
{
    public GameObject gameobject;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleActive()
    {
        gameobject.SetActive(!gameobject.activeSelf);
    }

}