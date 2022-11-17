using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance = null;

    private void Start()
    {
        if (instance != null) return;
        instance = this;
    }


    public void NextLevel() 
    {
        SceneManager.LoadScene(0);
    }
}
