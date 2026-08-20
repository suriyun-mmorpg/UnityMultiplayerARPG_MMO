using UnityEngine;
using LiteNetLibManager;
using System.Collections.Concurrent;

namespace MultiplayerARPG.MMO
{
    public class LogGUI : MonoBehaviour
    {
        private struct LogData
        {
            public string logText;
            public Color logColor;
        }

        [Tooltip("Height of log area")]
        public int logAreaHeight = 100;
        [Tooltip("Amount of logs to show")]
        public int showLogSize = 20;

#if !UNITY_SERVER || DEVELOPMENT_BUILD
        private Vector2 _scrollPosition;
        private readonly ConcurrentQueue<LogData> _printingLogs = new ConcurrentQueue<LogData>();
        private bool _logScrollingToBottom;
        private bool _loggingEnabled = false;
#endif

        public void SetupLogger(string fileName)
        {
            LogManager.LoggerManager = new LoggerManager(new DefaultLoggerFactory($"Logs/{fileName}"));
#if !UNITY_SERVER || DEVELOPMENT_BUILD
            _loggingEnabled = true;
#endif
        }

        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private void HandleLog(LogType type, string logString)
        {
#if !UNITY_SERVER || DEVELOPMENT_BUILD
            if (!_loggingEnabled)
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
            _printingLogs.Enqueue(new LogData()
            {
                logText = logString,
                logColor = color,
            });
            if (_printingLogs.Count > showLogSize)
                _printingLogs.TryDequeue(out _);
            _logScrollingToBottom = true;
#endif
        }

        public void HandleLog(string condition, string stackTrace, LogType type)
        {
            HandleLog(type, condition);
            switch (type)
            {
                case LogType.Assert:
                case LogType.Log:
                    Logging.Log(condition);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    Logging.LogError("{0}\n{1}", condition, stackTrace);
                    break;
                case LogType.Warning:
                    Logging.LogWarning(condition);
                    break;
            }
        }

#if !UNITY_SERVER || DEVELOPMENT_BUILD
        void OnGUI()
        {
            if (_logScrollingToBottom)
            {
                _scrollPosition.y = Mathf.Infinity;
                _logScrollingToBottom = false;
            }
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(logAreaHeight));
            foreach (LogData logData in _printingLogs)
            {
                GUI.color = logData.logColor;
                GUILayout.Label(logData.logText);
            }
            GUILayout.EndScrollView();
        }
#endif
    }
}
