using UnityEngine;
using LiteNetLibManager;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ZLogger;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class LogGUI : MonoBehaviour
{
    private struct LogData
    {
        public string logText;
        public Color logColor;
    }

    public string logFolder = "log";
    public string logExtension = "log";
    [Tooltip("Height of log area")]
    public int logAreaHeight = 100;
    [Tooltip("Amount of logs to show")]
    public int showLogSize = 20;

#if !UNITY_SERVER || DEVELOPMENT_BUILD
    private Vector2 scrollPosition;
    private readonly ConcurrentQueue<LogData> PrintingLogs = new ConcurrentQueue<LogData>();
    private bool logScrollingToBottom;
    private bool loggingEnabled = false;
#endif

    public void SetupLogger(string fileName)
    {
        LogManager.DefaultLoggerManager = CreateLoggerManager($"{fileName}.info");
        LogManager.ErrorLoggerManager = CreateLoggerManager($"{fileName}.err");
        LogManager.WarningLoggerManager = CreateLoggerManager($"{fileName}.warn");
#if !UNITY_SERVER || DEVELOPMENT_BUILD
        loggingEnabled = true;
#endif
    }

    private LoggerManager CreateLoggerManager(string fileName)
    {
        return new LoggerManager(UnityLoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
#if !UNITY_SERVER || DEVELOPMENT_BUILD
            builder.AddZLoggerUnityDebug(options =>
            {
                options.PrefixFormatter = LogManager.PrefixFormatterConfigure;
            });
#endif
            builder.AddZLoggerFile($"{logFolder}/{fileName}.{logExtension}", options =>
            {
                options.PrefixFormatter = LogManager.PrefixFormatterConfigure;
            });
        }));
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
        if (!loggingEnabled)
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
            logText = logString,
            logColor = color,
        });
        if (PrintingLogs.Count > showLogSize)
            PrintingLogs.TryDequeue(out _);
        logScrollingToBottom = true;
#endif
    }

    public void HandleLog(string condition, string stackTrace, LogType type)
    {
        HandleLog(type, condition);
        switch (type)
        {
            case LogType.Log:
                if (!LogManager.IsLoggerDisposed)
                    Logging.Log(condition);
                break;
            case LogType.Exception:
                if (!LogManager.IsErrorLoggerDisposed)
                    Logging.LogError(condition + "\n" + stackTrace);
                break;
            case LogType.Error:
                if (!LogManager.IsErrorLoggerDisposed)
                    Logging.LogError(condition + "\n" + stackTrace);
                break;
            case LogType.Warning:
                if (!LogManager.IsWarningLoggerDisposed)
                    Logging.LogWarning(condition);
                break;
        }
    }

#if !UNITY_SERVER || DEVELOPMENT_BUILD
    void OnGUI()
    {
        if (logScrollingToBottom)
        {
            scrollPosition.y = Mathf.Infinity;
            logScrollingToBottom = false;
        }
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(logAreaHeight));
        foreach (LogData logData in PrintingLogs)
        {
            GUI.color = logData.logColor;
            GUILayout.Label(logData.logText);
        }
        GUILayout.EndScrollView();
    }
#endif

    private class LogGUIProvider : ILoggerProvider
    {
        LogGUIProcessor logProcessor;

        public LogGUIProvider(LogGUI logGUI, IOptions<ZLoggerOptions> options)
        {
            logProcessor = new LogGUIProcessor(logGUI, options.Value);
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new AsyncProcessZLogger(categoryName, logProcessor);
        }

        public void Dispose()
        {
        }
    }

    private class LogGUIProcessor : IAsyncLogProcessor
    {
        readonly LogGUI logGUI;
        readonly ZLoggerOptions options;

        public LogGUIProcessor(LogGUI logGUI, ZLoggerOptions options)
        {
            this.logGUI = logGUI;
            this.options = options;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public void Post(IZLoggerEntry log)
        {
            try
            {
                string msg = log.FormatToString(options, null);
                switch (log.LogInfo.LogLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        logGUI.HandleLog(LogType.Log, msg);
                        break;
                    case LogLevel.Warning:
                    case LogLevel.Critical:
                        logGUI.HandleLog(LogType.Warning, msg);
                        break;
                    case LogLevel.Error:
                        if (log.LogInfo.Exception != null)
                        {
                            logGUI.HandleLog(LogType.Exception, msg);
                        }
                        else
                        {
                            logGUI.HandleLog(LogType.Error, msg);
                        }
                        break;
                    case LogLevel.None:
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                log.Return();
            }
        }
    }
}
