using UnityEngine;
using TMPro;
using Unity.Netcode;

public class LeaderboardUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText; 

    void Update()
    {
        // ตรวจสอบว่า ScoreManager พร้อมทำงานหรือยัง
        if (ScoreManager.Instance != null && ScoreManager.Instance.IsSpawned)
        {
            UpdateDisplay();
        }
    }

    public void UpdateDisplay()
    {
        if (scoreText == null) return;

        string display = "<color=#FFD700>LEADERBOARD</color>\n";
        if (ScoreManager.Instance.PlayerScores != null)
        {
            foreach (var data in ScoreManager.Instance.PlayerScores)
            {
                display += $"Player {data.ClientId}: {data.Score}\n";
            }
        }
        scoreText.text = display;
    }
}