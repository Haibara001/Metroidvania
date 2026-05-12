using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainScene";

    public void StartGame()
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ContinueGame() 
    {
        int latestSlot = SaveSystem.GetMostRecentSaveSlot();

        if (latestSlot >= 0)
        {
            SaveSystem.LoadFromSlotStatic(latestSlot);
        }
        else
        {
            Debug.Log("No save data found. Please start a new game first.");
        }
    }

    public void LoadGame()
    {
        ContinueGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
