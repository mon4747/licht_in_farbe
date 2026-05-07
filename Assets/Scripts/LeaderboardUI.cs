using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic; // เพิ่มตัวนี้เพื่อให้ใช้ List ได้
using System.Linq; // เพิ่มตัวนี้เพื่อให้ใช้คำสั่ง OrderByDescending ได้ง่ายขึ้น

public class LeaderboardUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText; 

    void Update()
    {
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
            // 1. คัดลอกข้อมูลจาก NetworkList มาลงใน List ปกติ
            List<PlayerScoreData> sortedScores = new List<PlayerScoreData>();
            foreach (var data in ScoreManager.Instance.PlayerScores)
            {
                sortedScores.Add(data);
            }

            // 2. เรียงลำดับจากมากไปน้อยโดยใช้ LINQ
            var orderedList = sortedScores.OrderByDescending(p => p.Score).ToList();

            // 3. นำข้อมูลที่เรียงแล้วมาจัด Format ข้อความ
            foreach (var data in orderedList)
            {
                // ตกแต่งเพิ่มเติม: ถ้าเป็นตัวเราเองให้ไฮไลท์สีเขียว
                string colorTag = (data.ClientId == NetworkManager.Singleton.LocalClientId) ? "<color=#00FF00>" : "<color=#FFFFFF>";
                string displayName = data.Name.Length == 0 ? $"Player {data.ClientId}" : data.Name.ToString();
                display += $"{colorTag}{displayName}: {data.Score}</color>\n";
            }
        }
        
        scoreText.text = display;
    }
}