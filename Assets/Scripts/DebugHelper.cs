using System;
using TMPro;
using UnityEngine;

public class DebugHelper : MonoBehaviour {
    private static TextMeshProUGUI _debugLog;

    private static object _lock = new object();

    private static TextMeshProUGUI DebugLog {
        get {
            if (_debugLog == null) {
                lock (_lock) {
                    if (_debugLog == null) {
                        _debugLog = GameObject.Find("Debug Log").GetComponent<TextMeshProUGUI>();
                    }
                }
            }

            return _debugLog;
        }
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) {
            DebugHelper.Log(logString + "\n" + stackTrace);
        }
    }

    public static void Log(string message) {
        if (DebugLog == null) {
            return;
        }

        var lines = DebugLog.text.Split('\n');
        if (lines.Length >= 5) {
            DebugLog.text = "";
        }

        DebugLog.text += message + "\n";
    }

    private void Update() {
        DebugLog.ForceMeshUpdate();
    }
}