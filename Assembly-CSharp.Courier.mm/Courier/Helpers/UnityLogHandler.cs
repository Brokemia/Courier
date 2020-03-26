using System;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class UnityLogHandler : ILogHandler {

        public void LogException(Exception exception, UnityEngine.Object context) {
            CourierLogger.LogDetailed(exception, context == null ? null : (context.name + " (" + context.GetType() + ")"));
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
            CourierLogger.Log(logType, "Debug.Log", string.Format(format, args));
        }
    }
}
