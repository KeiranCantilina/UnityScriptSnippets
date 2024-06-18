using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void LoadScene(int sceneIndex)
    {
  
        SceneManager.LoadScene(sceneIndex);
    }
}