using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;
using System.Collections.Generic;

public class ColorZone : NetworkBehaviour
{
    private NetworkVariable<ulong> zoneOwnerId = new NetworkVariable<ulong>(ulong.MaxValue);
    private NetworkVariable<Vector4> zoneColor = new NetworkVariable<Vector4>(Vector4.zero);
    private List<ulong> playersInZone = new List<ulong>();
    private Tilemap tilemap;
    private SpriteRenderer zoneRenderer;
    private Color originalColor;

    [SerializeField] private float scoreTickRate = 1f;
    private float nextScoreTick = 0f;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (tilemap != null) originalColor = tilemap.color;
        else if (zoneRenderer != null) originalColor = zoneRenderer.color;

        zoneOwnerId.OnValueChanged += (oldVal, newVal) => UpdateVisualColor(newVal, zoneColor.Value);
        zoneColor.OnValueChanged += (oldVal, newVal) => UpdateVisualColor(zoneOwnerId.Value, newVal);
    }

    void Update()
    {
        if (!IsServer) return;

        if (zoneOwnerId.Value != ulong.MaxValue && Time.time >= nextScoreTick)
        {
            if (ScoreManager.Instance != null)
            {
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
        if (playersInZone.Count > 0)
        {
            ulong ownerId = playersInZone[0];
            Color newColor = originalColor;
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(ownerId, out var client) && client.PlayerObject != null)
            {
                newColor = GetPlayerColor(client.PlayerObject.gameObject);
            }
            zoneColor.Value = newColor;
            zoneOwnerId.Value = ownerId;
        }
        else
        {
            zoneOwnerId.Value = ulong.MaxValue;
            zoneColor.Value = Vector4.zero;
        }
    }

    private Color GetPlayerColor(GameObject playerObject)
    {
        var playerRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (playerRenderer != null && playerRenderer.color != Color.white)
        {
            return playerRenderer.color;
        }

        string playerName = playerObject.name.ToLowerInvariant();
        if (playerName.Contains("yellow")) return Color.yellow;
        if (playerName.Contains("red")) return Color.red;
        if (playerName.Contains("blue")) return Color.blue;
        if (playerName.Contains("canine")) return Color.green;

        return originalColor;
    }

    private void UpdateVisualColor(ulong ownerId, Vector4 colorValue)
    {
        Color targetColor = originalColor;
        if (ownerId != ulong.MaxValue)
        {
            targetColor = new Color(colorValue.x, colorValue.y, colorValue.z, colorValue.w);
        }
        if (tilemap != null) tilemap.color = targetColor;
        if (zoneRenderer != null) zoneRenderer.color = targetColor;
    }
}
