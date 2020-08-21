using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;

namespace RWBeheading
{
    public static class CustomLogger
    {
        [Conditional("DEBUG")]
        public static void Dev(string msg, params object[] args)
        {
            Log.Message(string.Format("[Beheading] #dev: " + msg, args));
        }

        public static void NeedCheck(string msg, params object[] args)
        {
#if DEBUG
            Log.Error(string.Format("[Beheading] " + msg, args));
#else
            Log.Warning(string.Format("[Beheading] " + msg, args));
#endif
        }

        public static void ExceptionHandle(Exception e)
        {
#if DEBUG
            Log.Error(string.Format("[Beheading] Exception: " + e));
#else
            Log.Warning(string.Format("[Beheading] Exception: " + e));
#endif
        }

        public static void Error(string msg, params object[] args)
        {
            Log.Error(string.Format("[Beheading] " + msg, args));
        }
    }
}
