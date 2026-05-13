using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthUI : MonoBehaviour
{
    [Header("Hearts")]
    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    [Header("Stats")]
    public Text killText;
    public Text timerText;
    public GameObject gameOverPanel;

    private int killCount = 0;
    private float startTime;
    private bool isDead = false;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Actualizam Timer-ul
        float t = Time.time - startTime;
        string minutes = ((int)t / 60).ToString("00");
        string seconds = (t % 60).ToString("00");
        if (timerText != null) timerText.text = minutes + ":" + seconds;
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            int heartValue = (i + 1) * 2;
            if (currentHealth >= heartValue) heartImages[i].sprite = fullHeart;
            else if (currentHealth == heartValue - 1) heartImages[i].sprite = halfHeart;
            else heartImages[i].sprite = emptyHeart;
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }

    public void AddKill()
    {
        killCount++;
        if (killText != null) killText.text = "Kills: " + killCount;
    }
}
