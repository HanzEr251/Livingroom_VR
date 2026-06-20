using UnityEngine;
using UnityEngine.UI;
using TMPro;                    // ← 必须引入
using Pico.Platform.Models;
using Pico.Platform;

public class PicoVoiceToTextButton : MonoBehaviour
{
    [Header("输出目标（二选一）")]
    [Tooltip("普通文本显示（推荐用于展示结果）")]
    public TMP_Text resultText;

    [Tooltip("TMP Input Field（可编辑的输入框）")]
    public TMP_InputField resultInputField;

    private Button button;
    private bool isRecognizing = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("脚本必须挂载在带有 Button 组件的物体上！");
        }

        // 注册回调
        SpeechService.SetOnAsrResultCallback(OnAsrResult);
        SpeechService.SetOnSpeechErrorCallback(OnSpeechError);

        SpeechService.InitAsrEngine();
    }

    private void OnButtonClick()
    {
        if (!isRecognizing)
            StartVoiceRecognition();
        else
            StopVoiceRecognition();
    }

    private void StartVoiceRecognition()
    {
        if (isRecognizing) return;

        // 显示提示
        SetResultText("正在聆听...");

        int resultCode = SpeechService.StartAsr(
            autoStop: true,
            showPunctual: true,
            vadMaxDurationInSeconds: 15);

        if (resultCode == 0)
        {
            isRecognizing = true;
            Debug.Log("PICO ASR 启动成功");
        }
        else
        {
            Debug.LogError($"ASR 启动失败，错误码: {resultCode}");
            SetResultText($"启动失败 (Code: {resultCode})");
        }
    }

    private void StopVoiceRecognition()
    {
        if (!isRecognizing) return;
        SpeechService.StopAsr();
        isRecognizing = false;
        Debug.Log("ASR 已停止");
    }

    // ASR 结果回调
    private void OnAsrResult(Message<AsrResult> message)
    {
        if (message.IsError)
        {
            Debug.LogError($"ASR Result Error: {message.Error}");
            SetResultText("识别错误");
            return;
        }

        AsrResult result = message.Data;
        if (result != null)
        {
            Debug.Log($"ASR 结果: {result.Text} | IsFinalResult: {result.IsFinalResult}");

            SetResultText(result.Text);

            if (result.IsFinalResult)
            {
                // 可选：识别完成后自动停止
                // StopVoiceRecognition();
            }
        }
    }

    // 错误回调
    private void OnSpeechError(Message<SpeechError> message)
    {
        string errMsg = "未知错误";
        if (!message.IsError && message.Data != null)
        {
            errMsg = message.Data.Message;
        }
        else if (message.IsError)
        {
            errMsg = message.Error.ToString();
        }

        Debug.LogError($"ASR 错误: {errMsg}");
        SetResultText($"识别错误: {errMsg}");
        isRecognizing = false;
    }

    // 统一设置文本（同时支持 TMP_Text 和 TMP_InputField）
    private void SetResultText(string text)
    {
        if (resultText != null)
            resultText.text = text;

        if (resultInputField != null)
            resultInputField.text = text;
    }

    private void OnDestroy()
    {
        if (isRecognizing)
            SpeechService.StopAsr();
    }
}