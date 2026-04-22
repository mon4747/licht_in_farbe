using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;
using System.Collections.Generic;

[System.Serializable]
public class PlayerColorMapping
{
    public GameObject player; 
    public Color color;
}

public class ColorZone : NetworkBehaviour
{
    private NetworkVariable<ulong> zoneOwnerId = new NetworkVariable<ulong>(ulong.MaxValue);
    private List<ulong> playersInZone = new List<ulong>();
    private Tilemap tilemap;
    private SpriteRenderer zoneRenderer;
    private Color originalColor;

    [SerializeField] private PlayerColorMapping[] playerMappings;
    [SerializeField] private float scoreTickRate = 1f;
    private float nextScoreTick = 0f;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (tilemap != null) originalColor = tilemap.color;
        else if (zoneRenderer != null) originalColor = zoneRenderer.color;

        zoneOwnerId.OnValueChanged += (oldVal, newVal) => UpdateVisualColor(newVal);
    }

   // ใน Update() ของ ColorZone.cs
void Update()
{
    if (!IsServer) return;

    if (zoneOwnerId.Value != ulong.MaxValue && Time.time >= nextScoreTick)
    {
        if (ScoreManager.Instance != null)
        {
            // ส่งแต้ม 10 คะแนนให้เจ้าของพื้นที่
            ScoreManager.Instance.AddScoreRpc(zoneOwnerId.Value, 10);
            Debug.Log($"Zone giving score to {zoneOwnerId.Value}"); 
        }
        nextScoreTick = Time.time + scoreTickRate;
    }
}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || !collision.CompareTag("Player")) return;
        var networkObj = collision.GetComponent<NetworkObject>();
        if (networkObj != null)
        {
            ulong id = networkObj.OwnerClientId;
            if (!playersInZone.Contains(id)) playersInZone.Add(id);
            UpdateOwnerLogic();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer || !collision.CompareTag("Player")) return;
        var networkObj = collision.GetComponent<NetworkObject>();
        if (networkObj != null)
        {
            ulong id = networkObj.OwnerClientId;
            playersInZone.Remove(id);
            UpdateOwnerLogic();
        }
    }

    private void UpdateOwnerLogic()
    {
        zoneOwnerId.Value = playersInZone.Count > 0 ? playersInZone[0] : ulong.MaxValue;
    }

    private void UpdateVisualColor(ulong ownerId)
    {
        Color targetColor = originalColor;
        if (ownerId != ulong.MaxValue)
        {
            int index = (int)(ownerId % (ulong)playerMappings.Length);
            targetColor = playerMappings[index].color;
        }
        if (tilemap != null) tilemap.color = targetColor;
        if (zoneRenderer != null) zoneRenderer.color = targetColor;
    }
}