using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void GameStart()
    {
        SceneManager.LoadScene(1);
    }
    public void GameMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void GameScene1()
    {
        SceneManager.LoadScene(1);
    }
    public void GameScene2()
    {
        SceneManager.LoadScene(2);
    }
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // ±à¼­Æ÷ÖÐÍ£Ö¹ÔËÐÐ
#endif
    }
}
