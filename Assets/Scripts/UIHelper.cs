
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIHelper : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelCredit;
    public void LoadScene(string namaScene)
    {
        SceneManager.LoadScene(namaScene);
    }

    public void ToggleCredits()
    {
        bool showCredit = panelMenu.activeSelf;//apakah menu sedang aktif
        panelMenu.SetActive(!showCredit);
        panelCredit.SetActive(showCredit);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
