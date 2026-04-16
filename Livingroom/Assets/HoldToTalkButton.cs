using UnityEngine;
using UnityEngine.EventSystems;

public class HoldToTalkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public VoiceInputManager voiceInputManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        voiceInputManager.BeginRecord();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        voiceInputManager.EndRecord();
    }
}