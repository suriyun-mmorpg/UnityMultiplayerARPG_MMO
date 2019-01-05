using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LogGUI : MonoBehaviour
{
    private struct LogData
    {
        public string logText;
        public Color logColor;
    }

    [Tooltip("Log file name prefix, log file name will followed by _ and ticks")]
    public string logFileName = "log";
    [Tooltip("Height of log area")]
    public int logAreaHeight = 100;
    [Tooltip("If this is TRUE it will open log directory when start")]
    public bool openLogDir = false;
    [Tooltip("Amount of logs to show")]
    public int showLogSize = 20;

    private Vector2 scrollPosition;
    private string logSavePath;
    private List<LogData> logList = new List<LogData>();
    private bool shouldScrollToBottom;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        if (string.IsNullOrEmpty(logSavePath))
        {
            logSavePath = Application.persistentDataPath + "/" + logFileName + ".txt";
            if (openLogDir)
                Application.OpenURL(Application.persistentDataPath);
            // Write log file header
            using (StreamWriter writer = new StreamWriter(logSavePath, true, Encoding.UTF8))
            {
                writer.WriteLine("\n\n -- Log Start: " + System.DateTime.Now);
            }
        }
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        using (StreamWriter writer = new StreamWriter(logSavePath, true, Encoding.UTF8))
        {
            writer.WriteLine("(" + type + ") " + logString + "\n" + stackTrace.Replace("\n", "\n\t\t"));
        }
        Color color = Color.white;
        switch (type)
        {
            case LogType.Error:
                color = Color.red;
                break;
            case LogType.Warning:
                color = Color.yellow;
                break;
            case LogType.Exception:
                color = Color.magenta;
                break;
        }
        logList.Add(new LogData()
        {
            logText = logString,
            logColor = color,
        });
        if (logList.Count > showLogSize)
            logList.RemoveAt(0);
        shouldScrollToBottom = true;
    }

    void OnGUI()
    {
        if (shouldScrollToBottom)
        {
            scrollPosition.y = Mathf.Infinity;
            shouldScrollToBottom = false;
        }
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(logAreaHeight));
        for (int i = 0; i < logList.Count; ++i)
        {
            LogData logData = logList[i];
            GUI.color = logData.logColor;
            GUILayout.Label(logData.logText);
        }
        GUILayout.EndScrollView();
    }
}
