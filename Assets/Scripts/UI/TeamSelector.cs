using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Collections;

public struct TeamSelectionMessage : INetworkSerializable
{
    public int teamIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref teamIndex);
    }
}

[System.Serializable]
public struct TeamOption
{
    // ������
    public Button colorButton;

    // �ʧ�բ��/��ͺ�ͺ���� (Selection Box) �����ʴ�����Ͷ١���͡
    public GameObject selectionBox;

    // ����Ф����з�� (����ҡ Prefab ���� GameObject �����������)
    public GameObject characterModel;
}

public class TeamSelector : MonoBehaviour
{
    // ����¡�âͧ���з��
    [SerializeField] private TeamOption[] teams;

    // index �ͧ����Ѩ�غѹ
    [SerializeField] private int currentTeamIndex = 0;

    public const string PlayerTeamKey = "SelectedTeamIndex";

    private void Start()
    {
        // ��Ŵ��ҷ��������͡�������ش (�����) ��������������������� 0
        currentTeamIndex = PlayerPrefs.GetInt(PlayerTeamKey, 0);

        // �ѻവ����ʴ�������������
        HandleTeamSelectionChanged();
    }

    // �ѧ��ѹ��ѡ㹡���ѻവ����ʴ���
    public void HandleTeamSelectionChanged()
    {
        // 1. �Դ�ʧ/��ͺ���͡ ��лԴ����Фâͧ�ء���
        foreach (TeamOption team in teams)
        {
            // �Դ�ʧ�բ�Ƿ���ͺ����
            if (team.selectionBox != null)
                team.selectionBox.SetActive(false);

            // �Դ����Фõ������ͧ�������
            if (team.characterModel != null)
                team.characterModel.SetActive(false);
        }

        // 2. ��Ǩ�ͺ index ����ʹ��� (��ͧ�ѹ������͡�������������ԧ)
        if (currentTeamIndex < 0 || currentTeamIndex >= teams.Length)
        {
            currentTeamIndex = 0;
        }

        // 3. �Դ����ʴ��Ţͧ���������͡
        TeamOption selectedTeam = teams[currentTeamIndex];

        // �Դ�ʧ�բ�Ƿ���ͺ�����ͧ������١���͡
        if (selectedTeam.selectionBox != null)
            selectedTeam.selectionBox.SetActive(true);

        // �Դ����Фõ������ͧ������١���͡
        if (selectedTeam.characterModel != null)
            selectedTeam.characterModel.SetActive(true);
    }

    // �ѧ��ѹ�������ը����¡
    public void SelectTeam(int index)
    {
        currentTeamIndex = index;
        HandleTeamSelectionChanged();
    }

    // �ѧ��ѹ����Ѻ������ Connect ���ͺѹ�֡
    public void SaveTeamSelection()
    {
        PlayerPrefs.SetInt(PlayerTeamKey, currentTeamIndex);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            var message = new TeamSelectionMessage { teamIndex = currentTeamIndex };
            using (var writer = new FastBufferWriter(sizeof(int), Allocator.Temp))
            {
                writer.WriteValueSafe(message);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("TeamSelection", NetworkManager.ServerClientId, writer);
            }
        }
        else
        {
            Debug.Log("TeamSelector: saved selection locally, network unavailable so no team message sent.");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}