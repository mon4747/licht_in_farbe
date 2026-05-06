using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;

    public async void StartHost()
    {
        if (HostSingleton.Instance == null || HostSingleton.Instance.GameManager == null)
        {
            Debug.LogError("Host setup is not ready. Make sure HostSingleton is created before calling StartHost.");
            return;
        }

        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        if (ClientSingleton.Instance == null || ClientSingleton.Instance.GameManager == null)
        {
            Debug.LogError("Client setup is not ready. Make sure ClientSingleton is created before calling StartClient.");
            return;
        }

        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}
 