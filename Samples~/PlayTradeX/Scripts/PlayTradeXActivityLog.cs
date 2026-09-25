using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayTradeX.UI
{
    public sealed class PlayTradeXActivityLog : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private int maxEntries = 100;
        [SerializeField] private bool includeMilliseconds;

        private int entryCount;

        public void Success(string message) => AddEntry("SUCCESS", message, "#39E58C");
        public void Fail(string message) => AddEntry("FAIL", message, "#FF4D67");
        public void Info(string message) => AddEntry("INFO", message, "#58B9FF");
        public void Warning(string message) => AddEntry("WARNING", message, "#FFC857");

        public void Clear()
        {
            if (logText != null) logText.text = string.Empty;
            entryCount = 0;
            ScrollToBottom();
        }

        private void AddEntry(string status, string message, string color)
        {
            if (logText == null) return;
            string time = DateTime.Now.ToString(includeMilliseconds ? "HH:mm:ss.fff" : "HH:mm:ss");
            string line = $"<color=#7895B2>[{time}]</color> <color={color}><b>{status}</b></color>  <color=#DCEBFA>{Escape(message)}</color>";
            logText.text = string.IsNullOrEmpty(logText.text) ? line : logText.text + "\n" + line;
            entryCount++;
            Trim();
            
            StartCoroutine(ScrollNextFrame());
        }

        private void Trim()
        {
            if (maxEntries <= 0 || entryCount <= maxEntries) return;
            string[] lines = logText.text.Split('\n');
            int keep = Mathf.Min(maxEntries, lines.Length);
            logText.text = string.Join("\n", lines, lines.Length - keep, keep);
            entryCount = keep;
        }

        private IEnumerator ScrollNextFrame()
        {
            // Wait until TMP and Unity layout have processed the new line.
            yield return null;

            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                content);

            Canvas.ForceUpdateCanvases();

            // 0 = bottom
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private void ScrollToBottom()
        {
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}