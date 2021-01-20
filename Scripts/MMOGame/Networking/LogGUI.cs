using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LiteNetLibManager;
using System.Collections.Concurrent;

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
    private readonly ConcurrentQueue<LogData> PrintingLogs = new ConcurrentQueue<LogData>();
    private readonly ConcurrentQueue<string> WritingLogs = new ConcurrentQueue<string>();
    private bool shouldScrollToBottom;

    private void Start()
    {
        if (!Directory.Exists("./log"))
            Directory.CreateDirectory("./log");
        logSavePath = $"./log/{logFileName}.txt";
        if (openLogDir && !Application.isConsolePlatform && !Application.isMobilePlatform)
        {
            // Open log folder while running standalone platforms
            Application.OpenURL("./log/");
        }
        // Write log file header
        using (StreamWriter writer = new StreamWriter(logSavePath, true, Encoding.UTF8))
        {
            writer.WriteLine($"\n\n -- Log Start: {System.DateTime.Now}");
        }
    }

    private void OnEnable()
    {
        Logging.onLog += HandleLog;
        Application.logMessageReceivedThreaded += HandleLog;
    }

    private void OnDisable()
    {
        Logging.onLog -= HandleLog;
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    private void LateUpdate()
    {
        using (StreamWriter writer = new StreamWriter(logSavePath, true, Encoding.UTF8))
        {
            string logText;
            while (WritingLogs.TryDequeue(out logText))
            {
                writer.WriteLine(logText);
            }
        }
    }

    private void HandleLog(LogType type, string tag, string logString)
    {
        if (string.IsNullOrEmpty(logSavePath))
            return;
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
        PrintingLogs.Enqueue(new LogData()
        {
            logText = $"[{tag}] {logString}",
            logColor = color,
        });
        if (PrintingLogs.Count > showLogSize)
            PrintingLogs.TryDequeue(out _);
        WritingLogs.Enqueue($"({type})[{tag}] {logString}\n");
        shouldScrollToBottom = true;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (string.IsNullOrEmpty(logSavePath))
            return;
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
        PrintingLogs.Enqueue(new LogData()
        {
            logText = $"[UnityEngine.Debug] {condition}",
            logColor = color,
        });
        if (PrintingLogs.Count > showLogSize)
            PrintingLogs.TryDequeue(out _);
        WritingLogs.Enqueue($"({type})[UnityEngine.Debug] {condition}\n");
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
        foreach (LogData logData in PrintingLogs)
        {
            GUI.color = logData.logColor;
            GUILayout.Label(logData.logText);
        }
        GUILayout.EndScrollView();
    }
}
