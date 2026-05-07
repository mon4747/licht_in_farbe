using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
    public float gameDuration = 300f; // 5 นาที
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>();
    private NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false);

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public GameObject summaryPanel;
    public TextMeshProUGUI winnerText; // Text แสดงชื่อคนชนะใน Panel

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            timeRemaining.Value = gameDuration;
            isGameOver.Value = false;
        }
    }

    void Update()
    {
        if (IsServer && !isGameOver.Value)
        {
            if (timeRemaining.Value > 0)
            {
                timeRemaining.Value -= Time.deltaTime;
            }
            else
            {
                EndGame();
            }
        }

        UpdateTimerUI();

        // เมื่อเกมจบ ใครแตะหน้าจอก็จะเรียกคำสั่งกลับ Lobby
        if (isGameOver.Value && Input.GetMouseButtonDown(0))
        {
            ReturnToLobby();
        }
    }

    private void EndGame()
    {
        timeRemaining.Value = 0;
        isGameOver.Value = true;
        
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.canAddScore = false; // หยุดรับคะแนนที่ Server

        ShowSummaryClientRpc();
    }

    [ClientRpc]
    private void ShowSummaryClientRpc()
    {
        Time.timeScale = 0; // หยุด Physics และการเคลื่อนที่ในเครื่อง Client
        summaryPanel.SetActive(true);

        if (ScoreManager.Instance != null)
        {
            PlayerScoreData winner = ScoreManager.Instance.GetWinner();
            string winnerName = winner.Name.Length == 0 ? $"Player {winner.ClientId}" : winner.Name.ToString();
            winnerText.text = $"Winner: {winnerName}\nScore: {winner.Score}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(timeRemaining.Value / 60);
        int seconds = Mathf.FloorToInt(timeRemaining.Value % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1; // สำคัญ: ต้อง Reset เวลาก่อนเปลี่ยน Scene
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Menu");
        }
    }
}