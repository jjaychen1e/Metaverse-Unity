using UnityEngine;
using UnityEngine.UI;

public class VoiceButtonImageController : MonoBehaviour {
    public Sprite voiceOn;
    public Sprite voiceOff;
    
    public void SetStartImage() {
        GetComponent<Image>().sprite = voiceOff;
    }

    public void SetStopImage() {
        GetComponent<Image>().sprite = voiceOn;
    }
}