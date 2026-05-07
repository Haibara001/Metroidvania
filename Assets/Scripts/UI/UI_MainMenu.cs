using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainScene";

    public void StartGame()
    {
        SceneManager.LoadScene(sceneName);
    }

}
