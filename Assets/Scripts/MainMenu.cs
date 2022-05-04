using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string FirstScene;
    public GameObject LoadingScene;
    public void StartGame()
    {
        LoadingScene.SetActive(true);
        SceneManager.LoadScene(FirstScene);
    }
}
