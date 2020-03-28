﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mod.Courier {
    // https://github.com/EverestAPI/Everest/blob/master/Celeste.Mod.mm/Mod/Everest/Logger.cs
    public static class CourierLogger {
        /// <summary>
        /// Log a string to the console and to log.txt
        /// </summary>
        /// <param name="tag">The tag, preferably short enough to identify your mod, but not too long to clutter the log.</param>
        /// <param name="str">The string / message to log.</param>
        public static void Log(string tag, string str)
            => Log(LogType.Log, tag, str);
        /// <summary>
        /// Log a string to the console and to log.txt
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="tag">The tag, preferably short enough to identify your mod, but not too long to clutter the log.</param>
        /// <param name="str">The string / message to log.</param>
        public static void Log(LogType level, string tag, string str) {
            Console.Write("(");
            Console.Write(DateTime.Now);
            Console.Write(") [Courier] [");
            Console.Write(level.ToString());
            Console.Write("] [");
            Console.Write(tag);
            Console.Write("] ");
            Console.WriteLine(str);
        }

        /// <summary>
        /// Log a string to the console and to log.txt, including a call stack trace.
        /// </summary>
        /// <param name="tag">The tag, preferably short enough to identify your mod, but not too long to clutter the log.</param>
        /// <param name="str">The string / message to log.</param>
        public static void LogDetailed(string tag, string str) {
            Log(LogType.Log, tag, str);
            Console.WriteLine(new StackTrace(1, true).ToString());
        }
        /// <summary>
        /// Log a string to the console and to log.txt, including a call stack trace.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="tag">The tag, preferably short enough to identify your mod, but not too long to clutter the log.</param>
        /// <param name="str">The string / message to log.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogDetailed(LogType level, string tag, string str) {
            Log(level, tag, str);
            Console.WriteLine(new StackTrace(1, true));
        }

        /// <summary>
        /// Print the exception to the console, including extended loading / reflection data useful for mods.
        /// </summary>
        public static void LogDetailed(this Exception e, string tag = null) {
            if (tag == null) {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Detailed exception log:");
            }
            for (Exception e_ = e; e_ != null; e_ = e_.InnerException) {
                Console.WriteLine("--------------------------------");
                Console.WriteLine(e_.GetType().FullName + ": " + e_.Message + "\n" + e_.StackTrace);
                if (e_ is ReflectionTypeLoadException) {
                    ReflectionTypeLoadException rtle = (ReflectionTypeLoadException) e_;
                    for (int i = 0; i < rtle.Types.Length; i++) {
                        Console.WriteLine("ReflectionTypeLoadException.Types[" + i + "]: " + rtle.Types[i]);
                    }
                    for (int i = 0; i < rtle.LoaderExceptions.Length; i++) {
                        LogDetailed(rtle.LoaderExceptions[i], tag + (tag == null ? "" : ", ") + "rtle:" + i);
                    }
                }
                if (e_ is TypeLoadException) {
                    Console.WriteLine("TypeLoadException.TypeName: " + ((TypeLoadException) e_).TypeName);
                }
                if (e_ is BadImageFormatException) {
                    Console.WriteLine("BadImageFormatException.FileName: " + ((BadImageFormatException) e_).FileName);
                }
            }
        }
    }
}