using TMPro;
using UnityEngine;

public class DebugHelper : MonoBehaviour {
    private static TextMeshProUGUI _debugLog;

    private static object _lock = new object();

    private static TextMeshProUGUI DebugLog {
        get {
            if (_debugLog == null) {
                lock (_lock) {
                    _debugLog = GameObject.Find("Debug Log").GetComponent<TextMeshProUGUI>();
                }
            }

            return _debugLog;
        }
    }

    public static void Log(string message) {
        if (DebugLog == null) {
            return;
        }
        
        var lines = DebugLog.text.Split('\n');
        if (lines.Length >= 10) {
            DebugLog.text = "";
        }

        DebugLog.text += message + "\n";
    }
}