using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject upgradeUI;
    public GameObject gameOverUI;
    public GameObject orbitalShieldPrefab;
    private GameObject currentShield;

    private int currentXP = 0;
    private int xpToLevelUp = 5;

    void Awake() { Instance = this; }

    public void AddXP()
    {
        currentXP++;
        if (currentXP >= xpToLevelUp)
        {
            currentXP = 0;
            xpToLevelUp += 3;
            ShowUpgradeUI();
        }
    }

    void ShowUpgradeUI()
    {
        Time.timeScale = 0f;
        if (upgradeUI != null) upgradeUI.SetActive(true);
    }

    public void ApplyUpgrade_FireRate()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.GetComponent<AutoAttack>().fireRate *= 0.8f;
        ResumeGame();
    }

    public void ApplyUpgrade_Shield()
    {
        if (currentShield == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && orbitalShieldPrefab != null)
            {
                currentShield = Instantiate(orbitalShieldPrefab, player.transform.position, Quaternion.identity);
                currentShield.transform.SetParent(player.transform);
            }
        }
        else
        {
            currentShield.GetComponent<OrbitalShield>().rotationSpeed += 50f;
            currentShield.transform.localScale *= 1.2f;
        }
        ResumeGame();
    }

    void ResumeGame()
    {
        if (upgradeUI != null) upgradeUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
