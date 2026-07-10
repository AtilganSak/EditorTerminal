using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace EditorTerminal
{
    static class PackageOps
    {
        public static void PollAndReport(Request request, string actionLabel, string id)
        {
            EditorTerminalWindow.SetAllBusy($"{actionLabel} '{id}' isleniyor...");

            EditorApplication.CallbackFunction poll = null;
            poll = () =>
            {
                if (!request.IsCompleted)
                    return;

                EditorApplication.update -= poll;

                string message;
                if (request.Status == StatusCode.Success)
                {
                    message = $"{actionLabel} '{id}': basarili.";
                    Debug.Log($"[EditorTerminal] {message}");
                }
                else
                {
                    message = $"error: {actionLabel} '{id}' basarisiz - {request.Error?.message}";
                    Debug.LogError($"[EditorTerminal] {message}");
                }

                EditorTerminalWindow.ClearAllBusy(message);
            };
            EditorApplication.update += poll;
        }
    }
}
