using TMPro;
using UnityEngine;

public class FamilyChatManager : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text roundText;
    public TMP_Text currentSpeakerText;
    public TMP_Text chatHistoryText;
    public TMP_Text emotionScoreText;
    public TMP_Text hintText;
    public TMP_InputField userInputField;

    private int round = 0;
    private string currentSpeaker = "系统";

    public void StartChat()
    {
        round = 1;
        currentSpeaker = "奶奶";

        roundText.text = "第 " + round + " 轮";
        currentSpeakerText.text = "当前发言人：" + currentSpeaker;
        chatHistoryText.text = "【系统】家族群聊开始\n";
        hintText.text = "提示：请使用语音或文字输入";
    }

    public void NextTurn()
    {
        string[] speakers = { "妈妈", "爸爸", "爷爷" };
        int idx = System.Array.IndexOf(speakers, currentSpeaker);
        idx = (idx + 1) % speakers.Length;
        currentSpeaker = speakers[idx];
        round++;

        roundText.text = "第 " + round + " 轮";
        currentSpeakerText.text = "当前发言人：" + currentSpeaker;
        chatHistoryText.text += "【系统】进入第 " + round + " 轮，当前发言人：" + currentSpeaker + "\n";
    }

    public void EndChat()
    {
        chatHistoryText.text += "【系统】群聊结束\n";
        hintText.text = "提示：本轮结束";
    }

    public void SendText()
    {
        string msg = userInputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        chatHistoryText.text += "【我】" + msg + "\n";
        userInputField.text = "";
    }

    public void ClearInput()
    {
        userInputField.text = "";
    }

    public void SetVoiceText(string text)
    {
        userInputField.text = text;
    }

    public void SetEmotionScore(string scoreText)
    {
        emotionScoreText.text = "情商评价：" + scoreText;
    }
}