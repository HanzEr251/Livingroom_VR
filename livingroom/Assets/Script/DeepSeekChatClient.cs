using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatMessage
{
    public string role;
    [TextArea(2, 6)]
    public string content;

    public ChatMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}

[Serializable]
public class DeepSeekRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class DeepSeekResponse
{
    public Choice[] choices;
    public string error;
}

[Serializable]
public class Choice
{
    public ResponseMessage message;
}

[Serializable]
public class ResponseMessage
{
    public string role;
    public string content;
}

public class DeepSeekChatClient : MonoBehaviour
{
    [Header("DeepSeek API 配置")]
    [Tooltip("如果需要 Bearer 前缀可以直接填写完整字符串，或者只填 API Key。")]
    public string apiKey = "";
    public string apiUrl = "https://api.deepseek.cn/v1/chat/completions";
    public string model = "deepseek-chat";
    [Tooltip("仅用于开发测试：忽略 SSL 证书验证。生产环境请保持关闭。")]
    public bool ignoreSslErrors = false;

    [Header("对话历史(多轮会话) ")]
    public List<ChatMessage> messageHistory = new List<ChatMessage>();

    /// <summary>
    /// 初始化对话，将 system prompt 作为第一条消息存入历史。
    /// </summary>
    public void Initialize(string systemPrompt)
    {
        messageHistory.Clear();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messageHistory.Add(new ChatMessage("system", systemPrompt));
        }
    }

    /// <summary>
    /// 发送用户消息到 DeepSeek，并回调回答。
    /// </summary>
    public IEnumerator SendChatRequest(string userContent, Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onError?.Invoke("请先填写 DeepSeek apiKey。\n可以在 ChatDemoUI 中设置。想要快速测试，可先在 Inspector 中填入 apiKey。 ");
            yield break;
        }

        if (string.IsNullOrEmpty(userContent))
        {
            onError?.Invoke("发送内容不能为空。");
            yield break;
        }

        messageHistory.Add(new ChatMessage("user", userContent));

        DeepSeekRequest requestData = new DeepSeekRequest
        {
            model = string.IsNullOrEmpty(model) ? "deepseek-chat" : model,
            messages = messageHistory
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (ignoreSslErrors)
            {
                request.certificateHandler = new AcceptAllCertificates();
            }

            string authValue = apiKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? apiKey
                : $"Bearer {apiKey}";
            request.SetRequestHeader("Authorization", authValue);

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasNetworkError = request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasNetworkError = request.isNetworkError || request.isHttpError;
#endif
            if (hasNetworkError)
            {
                onError?.Invoke($"请求失败：{request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            string assistantText = ParseAssistantAnswer(json);

            if (string.IsNullOrEmpty(assistantText))
            {
                onError?.Invoke($"DeepSeek 返回异常，原始响应：{json}");
                yield break;
            }

            messageHistory.Add(new ChatMessage("assistant", assistantText));
            onSuccess?.Invoke(assistantText);

            
        }
    }

    private string ParseAssistantAnswer(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            DeepSeekResponse response = JsonUtility.FromJson<DeepSeekResponse>(json);
            if (response != null && response.choices != null && response.choices.Length > 0 && response.choices[0].message != null)
            {
                return response.choices[0].message.content?.Trim();
            }
        }
        catch (Exception)
        {
            // 忽略解析异常，后续尝试原始字符串
        }

        return json.Trim();
    }

    private class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
