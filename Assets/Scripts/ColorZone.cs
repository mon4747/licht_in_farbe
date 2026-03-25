using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// Class สำหรับจับคู่ Player กับสี
[System.Serializable]
public class PlayerColorMapping
{
    public GameObject player;
    public Color color;
}

public class ColorZone : MonoBehaviour
{
    private Tilemap tilemap;
    private Renderer zoneRenderer;
    private Color originalColor;
    private Queue<GameObject> playerQueue = new Queue<GameObject>();

    // Array สำหรับลาก Player และกำหนดสี
    [SerializeField]
    private PlayerColorMapping[] playerMappings = new PlayerColorMapping[4];

    // ค่าที่กำหนดได้ใน Inspector
    [SerializeField]
    private string playerTag = "Player";

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        zoneRenderer = GetComponent<Renderer>();

        if (tilemap != null)
        {
            originalColor = tilemap.color;
        }
        else if (zoneRenderer != null && zoneRenderer.material != null)
        {
            originalColor = zoneRenderer.material.color;
        }
        else
        {
            originalColor = Color.white;
        }

        Debug.Log($"ColorZone Start: tilemap={(tilemap != null)}, renderer={(zoneRenderer != null)}, originalColor={originalColor}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            return;

        Debug.Log($"ColorZone TriggerEnter2D: {collision.gameObject.name}");

        if (!playerQueue.Contains(collision.gameObject))
        {
            playerQueue.Enqueue(collision.gameObject);
        }

        UpdateZoneColor();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            return;

        Debug.Log($"ColorZone TriggerExit2D: {collision.gameObject.name}");

        if (playerQueue.Count > 0 && playerQueue.Peek() == collision.gameObject)
        {
            playerQueue.Dequeue();
        }
        else
        {
            var tempList = new List<GameObject>(playerQueue);
            tempList.Remove(collision.gameObject);
            playerQueue = new Queue<GameObject>(tempList);
        }

        UpdateZoneColor();
    }

    Color GetPlayerColor(GameObject player)
    {
        string playerName = player.name.Replace("(Clone)", "").Trim();

        foreach (var mapping in playerMappings)
        {
            if (mapping == null || mapping.player == null)
                continue;

            string mappedName = mapping.player.name.Replace("(Clone)", "").Trim();
            if (mappedName == playerName)
            {
                return mapping.color;
            }
        }

        Debug.LogWarning($"ColorZone: ไม่พบการแมปสีของ player '{player.name}' (normalized '{playerName}')");
        return Color.white;
    }

    void UpdateZoneColor()
    {
        var color = originalColor;

        if (playerQueue.Count > 0)
        {
            var firstPlayer = playerQueue.Peek();
            if (firstPlayer != null)
            {
                color = GetPlayerColor(firstPlayer);
            }
        }

        if (tilemap != null)
        {
            tilemap.color = color;
        }
        else if (zoneRenderer != null && zoneRenderer.material != null)
        {
            zoneRenderer.material.color = color;
        }

        Debug.Log($"ColorZone UpdateZoneColor => {color} (queue={playerQueue.Count})");
    }
}
