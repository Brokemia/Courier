﻿using System;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class UnityLogHandler : ILogHandler {

        public void LogException(Exception exception, UnityEngine.Object context) {
            Logger.LogDetailed(exception, context.name + " (" + context.GetType() + ")");
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
            Logger.Log(logType, "Debug.Log", string.Format(format, args));
        }
    }
}