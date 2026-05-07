using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class UI_BossScene : MonoBehaviour
{
    [SerializeField] private string sceneName = "BossScene";
    [SerializeField] private KeyCode interactKey = KeyCode.V;

    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            playerInRange = false;
        }
    }
}
