using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using Dummiesman;
using System.Linq;


namespace KeiranUtils
{
    public static class LoadOBJDialog
    {
        // Triggers file picker dialog box and resulting file load
        static public GameObject[] loadOBJ()
        {
            string[] path = StandaloneFileBrowser.OpenFilePanel("Open File", "", "obj", true);
            GameObject[] gameobject = new GameObject[path.Length];

            for (int i = 0; i< path.Length; i++)
            {
                gameobject[i] = new OBJLoader().Load(path[i]);
            }
            
            // Create prefabs?


            // Return list of gameobjects
            return gameobject;
        }

    }

}
