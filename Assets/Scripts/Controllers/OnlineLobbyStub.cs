using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller tạm thời cho LobbyPanel (Online Multiplayer).
/// Phản hồi giao diện người dùng trước khi tích hợp Netcode hoàn chỉnh ở giai đoạn sau (SRP).
/// </summary>
public class OnlineLobbyStub : MonoBehaviour
{
    [Header("Input & Buttons")]
    [Tooltip("Ô nhập mã phòng khi Join Room.")]
    [SerializeField] private TMP_InputField _roomCodeInputField;

    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;

    [Tooltip("Text hiển thị thông báo trạng thái khi bấm Create/Join Room.")]
    [SerializeField] private TMP_Text _statusText;

    public TMP_InputField RoomCodeInputField
    {
        get => _roomCodeInputField;
        set => _roomCodeInputField = value;
    }
    public Button CreateRoomButton
    {
        get => _createRoomButton;
        set => _createRoomButton = value;
    }
    public Button JoinRoomButton
    {
        get => _joinRoomButton;
        set => _joinRoomButton = value;
    }
    public TMP_Text StatusText
    {
        get => _statusText;
        set => _statusText = value;
    }

    // Backward-compatibility aliases
    public TMP_InputField roomCodeInputField
    {
        get => _roomCodeInputField;
        set => _roomCodeInputField = value;
    }
    public Button createRoomButton
    {
        get => _createRoomButton;
        set => _createRoomButton = value;
    }
    public Button joinRoomButton
    {
        get => _joinRoomButton;
        set => _joinRoomButton = value;
    }
    public TMP_Text statusText
    {
        get => _statusText;
        set => _statusText = value;
    }

    private void OnEnable()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        if (_joinRoomButton != null) _joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        if (_statusText != null) _statusText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        if (_joinRoomButton != null) _joinRoomButton.onClick.RemoveListener(OnJoinRoomClicked);
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
        Debug.Log($"[OnlineLobbyStub] {message}");
        if (_statusText != null)
        {
            _statusText.text = message;
            _statusText.gameObject.SetActive(true);
        }
    }
}
