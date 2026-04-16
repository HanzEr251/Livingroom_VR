using UnityEngine;

public class VoiceInputManager : MonoBehaviour
{
    public FamilyChatManager familyChatManager;

    private AudioClip clip;
    private string micName;
    private bool isRecording = false;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            Debug.Log("检测到麦克风: " + micName);
        }
        else
        {
            Debug.LogError("没有检测到麦克风");
        }
    }

    public void BeginRecord()
    {
        if (isRecording || string.IsNullOrEmpty(micName)) return;

        clip = Microphone.Start(micName, false, 10, 16000);
        isRecording = true;
        Debug.Log("开始录音");
    }

    public void EndRecord()
    {
        if (!isRecording) return;

        Microphone.End(micName);
        isRecording = false;
        Debug.Log("结束录音");

        // 这里先用假数据代替语音识别结果
        familyChatManager.SetVoiceText("这是语音输入转换出来的文字");
    }
}