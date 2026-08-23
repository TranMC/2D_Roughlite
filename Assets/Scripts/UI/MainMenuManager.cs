using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Chuyển sang Scene chơi game (Scene "Test" của bro)
    public void PlayGame()
    {
        SceneManager.LoadScene("Test");
    }

    // Thoát game
    public void QuitGame()
    {
        Debug.Log("Đã bấm Quit!");
        Application.Quit();
    }
}