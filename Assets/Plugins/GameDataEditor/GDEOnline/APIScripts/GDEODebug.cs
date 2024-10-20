using UnityEngine;
using System;
using System.Collections;

namespace GameDataEditor
{
    public static class GDEODebug
    {
        public static void Log(string msg)
        {
            #if GDEO_LOGGING
            Debug.Log(msg);
            #endif
        }

        public static void Log(object msg)
        {
            #if GDEO_LOGGING
            Debug.Log(msg);
            #endif
        }

        public static void LogError(string err)
        {
            #if GDEO_LOGGING
            Debug.Log(err);
            #endif
        }

        public static void LogException(Exception ex)
        {
            #if GDEO_LOGGING
            Debug.LogException(ex);
            #endif
        }
    }
}
