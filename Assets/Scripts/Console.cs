using System;
using UnityEngine;
using TMPro;

public class Console : MonoBehaviour
{
    public TextMeshProUGUI consoleText;


    private void Awake()
    {
        consoleText.overflowMode = TextOverflowModes.Truncate;
        consoleText.alignment = TextAlignmentOptions.BottomLeft;
        Application.logMessageReceived += HandleLog;
    }
    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        consoleText.text += $"[{DateTime.Now.ToString("HH:mm:ss")}]>{GetColorTag(type)}{logString}</color>\n";
    }

    string GetColorTag(LogType type)
    {
        string color;
        switch (type)
        {
            case LogType.Warning:
                color = "yellow";
                break;
            case LogType.Error:
            case LogType.Exception:
                color = "red";
                break;
            default:
                color = "white";
                break;
        }
        return $"<color={color}>";
    }
}
