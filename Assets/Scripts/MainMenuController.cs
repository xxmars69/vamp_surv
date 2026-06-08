using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject characterPanel;
    public GameObject creditsPanel;
    public string gameSceneName = "SampleScene";

    public void ShowCharacterSelection()
    {
        SetPanel(mainPanel, false);
        SetPanel(characterPanel, true);
        SetPanel(creditsPanel, false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowCredits()
    {
        SetPanel(mainPanel, false);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, true);
    }

    public void BackToMain()
    {
        SetPanel(mainPanel, true);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void SettingsPlaceholder()
    {
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
