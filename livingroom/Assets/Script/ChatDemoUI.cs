using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChatDemoUI : MonoBehaviour
{
    [Header("AI 亲戚配置")]
    public AIRelativeConfig relativeConfig;

    [Header("DeepSeek 聊天客户端")]
    public DeepSeekChatClient chatClient;

    [Header("UI 组件")]
    public InputField inputField;
    public Button sendButton;
    public Text chatRecordText;
    public Text statusText;

    private bool waitingForResponse = false;

    private void Start()
    {
        if (relativeConfig == null)
        {
            Debug.LogError("请在 Inspector 中将 AIRelativeConfig 资产拖到 ChatDemoUI.relativeConfig 。");
            return;
        }

        if (chatClient == null)
        {
            Debug.LogError("请在 Inspector 中将 DeepSeekChatClient 对象拖到 ChatDemoUI.chatClient 。");
            return;
        }

        chatClient.Initialize(relativeConfig.systemPrompt);
        sendButton.onClick.AddListener(OnSendButtonClicked);
        UpdateStatus("请在输入框中输入消息，然后点击发送。", false);
    }

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(OnSendButtonClicked);
    }

    private void OnSendButtonClicked()
    {
        if (waitingForResponse)
        {
            UpdateStatus("请等待上一条消息返回后再发送。", true);
            return;
        }

        string userText = inputField.text?.Trim();
        if (string.IsNullOrEmpty(userText))
        {
            UpdateStatus("请输入聊天内容。", true);
            return;
        }

        AppendChatLine("我", userText);
        inputField.text = string.Empty;
        UpdateStatus("正在发送请求...", false);

        StartCoroutine(SendMessageCoroutine(userText));
    }

    private IEnumerator SendMessageCoroutine(string userText)
    {
        waitingForResponse = true;

        yield return chatClient.SendChatRequest(userText,
            assistantReply =>
            {
                AppendChatLine(relativeConfig.relativeName, assistantReply);
                UpdateStatus("收到回复。", false);
                waitingForResponse = false;
            },
            errorMessage =>
            {
                UpdateStatus(errorMessage, true);
                waitingForResponse = false;
            });
    }

    private void AppendChatLine(string speaker, string text)
    {
        if (chatRecordText == null)
            return;

        string line = $"<b>{speaker}:</b> {text}\n";
        chatRecordText.text += line;
    }

    private void UpdateStatus(string message, bool isError)
    {
        if (statusText == null)
            return;

        statusText.text = message;
        statusText.color = isError ? Color.red : Color.green;
    }
}
