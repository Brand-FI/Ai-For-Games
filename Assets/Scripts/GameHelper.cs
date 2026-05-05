using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHelper : MonoBehaviour
{
    public static GameHelper Instance;
    public GameObject panelWin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void showPanelWin()
    {
        panelWin.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Retry()
    {
        Time.timeScale = 1;
        panelWin.SetActive(false);
        SceneManager.LoadScene("Level 1");
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        panelWin.SetActive(false);
        SceneManager.LoadScene("Menu");
    }
}
