using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller tạm thời cho LobbyPanel (Online Multiplayer). Chưa tích hợp networking thật -
/// chỉ phản hồi UI để có thể duyệt qua flow trước khi triển khai netcode ở giai đoạn sau.
/// </summary>
public class OnlineLobbyStub : MonoBehaviour
{
    [Tooltip("Ô nhập room code khi Join Room.")]
    public TMP_InputField roomCodeInputField;

    public Button createRoomButton;
    public Button joinRoomButton;

    [Tooltip("Text hiển thị thông báo 'Coming soon' khi bấm Create/Join Room.")]
    public TMP_Text statusText;

    private void OnEnable()
    {
        if (createRoomButton != null) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (createRoomButton != null) createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        if (joinRoomButton != null) joinRoomButton.onClick.RemoveListener(OnJoinRoomClicked);
    }

    public void OnCreateRoomClicked()
    {
        ShowComingSoon("Chức năng Online Multiplayer (Create Room) sẽ được triển khai ở giai đoạn sau.");
    }

    public void OnJoinRoomClicked()
    {
        ShowComingSoon("Chức năng Online Multiplayer (Join Room) sẽ được triển khai ở giai đoạn sau.");
    }

    private void ShowComingSoon(string message)
    {
        Debug.Log(message);
        if (statusText != null)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(true);
        }
    }
}
