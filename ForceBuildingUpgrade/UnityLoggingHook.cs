﻿using System;
using System.Reflection;
using ColossalFramework.Plugins;
using UnityEngine;

namespace ModTools
{
    public static class UnityLoggingHook
    {
        private static MethodInfo logMethod = typeof(UnityEngine.Debug).GetMethod("Internal_Log",
            BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo logExceptionMethod = typeof(UnityEngine.Debug).GetMethod("Internal_LogException",
            BindingFlags.NonPublic | BindingFlags.Static);

        private static bool hookEnabled = false;

        private static RedirectCallsState log;
        private static RedirectCallsState logFormat;
        private static RedirectCallsState logError;
        private static RedirectCallsState logErrorFormat;
        private static RedirectCallsState logWarning;
        private static RedirectCallsState logWarningFormat;
        private static RedirectCallsState logException;

        public static void EnableHook()
        {
            if (hookEnabled)
            {
                return;
            }

            try
            {
                log = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("Log", new[] { typeof(object) }),
                    typeof(UnityLoggingHook).GetMethod("Log", new[] { typeof(object) })
                );

                logFormat = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("LogFormat", new[] { typeof(string), typeof(object[]) }),
                    typeof(UnityLoggingHook).GetMethod("LogFormat", new[] { typeof(string), typeof(object[]) })
                );

                logWarning = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("LogWarning", new[] { typeof(object) }),
                    typeof(UnityLoggingHook).GetMethod("LogWarning", new[] { typeof(object) })
                );

                logWarningFormat = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("LogWarningFormat", new[] { typeof(string), typeof(object[]) }),
                    typeof(UnityLoggingHook).GetMethod("LogWarningFormat", new[] { typeof(string), typeof(object[]) })
                );

                logError = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("LogError", new[] { typeof(object) }),
                    typeof(UnityLoggingHook).GetMethod("LogError", new[] { typeof(object) })
                );

                logErrorFormat = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("LogErrorFormat", new[] { typeof(string), typeof(object[]) }),
                    typeof(UnityLoggingHook).GetMethod("LogErrorFormat", new[] { typeof(string), typeof(object[]) })
                );

                logException = RedirectionHelper.RedirectCalls
                (
                    typeof(UnityEngine.Debug).GetMethod("LogException", new[] { typeof(Exception) }),
                    typeof(UnityLoggingHook).GetMethod("LogException", new[] { typeof(Exception) })
                );
            }
            catch (Exception ex)
            {
                global::ModTools.Log.Error("Failed to hook Unity's debug logging, reason: " + ex.Message);
            }

            hookEnabled = true;
            global::ModTools.Log.Warning("Unity logging subsystem hooked by ModTools");
        }

        public static void DisableHook()
        {
            if (!hookEnabled)
            {
                return;
            }

            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("Log", new[] { typeof(object) }), log);
            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("LogFormat", new[] { typeof(string), typeof(object[]) }), logFormat);
            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("LogWarning", new[] { typeof(object) }), logWarning);
            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("LogWarningFormat", new[] { typeof(string), typeof(object[]) }), logWarningFormat);
            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("LogError", new[] { typeof(object) }), logError);
            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("LogErrorFormat", new[] { typeof(string), typeof(object[]) }), logErrorFormat);
            RedirectionHelper.RevertRedirect(typeof(UnityEngine.Debug).GetMethod("LogException", new[] { typeof(Exception) }), logException);
            hookEnabled = false;
        }

        public static void Log(object message)
        {
            if (message == null)
            {
                message = "Null";
            }
            logMethod.Invoke(null, new object[] { 0, message.ToString(), null });
            if (ModTools.Instance.console != null)
            {
                try
                {
                    ModTools.Instance.console.AddMessage(message.ToString(), LogType.Log);
                }
                catch (Exception) { }
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            var message = string.Format(format, args);
            Log(message);
        }

        public static void LogWarning(object message)
        {
            if (message == null)
            {
                message = "Null";
            }
            logMethod.Invoke(null, new object[] { 1, message.ToString(), null });
            if (ModTools.Instance.console != null)
            {
                try
                {
                    ModTools.Instance.console.AddMessage(message.ToString(), LogType.Warning);
                }
                catch (Exception)
                {
                }
            }
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            var message = string.Format(format, args);
            LogWarning(message);
        }

        public static void LogError(object message)
        {
            if (message == null)
            {
                message = "Null";
            }
            logMethod.Invoke(null, new object[] { 2, message.ToString(), null });

            if (ModTools.Instance.console != null)
            {
                try
                {
                    ModTools.Instance.console.AddMessage(message.ToString(), LogType.Error);
                }
                catch (Exception)
                {
                }
            }
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            var message = string.Format(format, args);
            LogError(message);
        }

        public static void LogException(Exception exception)
        {
            logExceptionMethod.Invoke(null, new object[] { exception, null });
            if (ModTools.Instance.console != null)
            {
                try
                {
                    var message = exception != null ? exception.ToString() : "Null exception";
                    ModTools.Instance.console.AddMessage(message, LogType.Exception);
                }
                catch (Exception)
                {
                }
            }
        }

    }

}
