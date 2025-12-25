using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    internal static class MsAppHelper
    {
        public static string UpdateEditorState(string content, string screenName, bool add)
        {
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            int screensOrderIndex = -1;
            
            // Find ScreensOrder section
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "ScreensOrder:")
                {
                    screensOrderIndex = i;
                    break;
                }
            }

            if (screensOrderIndex == -1)
            {
                // If ScreensOrder doesn't exist, try to find EditorState to add it
                int editorStateIndex = lines.FindIndex(l => l.Trim() == "EditorState:");
                if (editorStateIndex != -1 && add)
                {
                    lines.Insert(editorStateIndex + 1, "  ScreensOrder:");
                    lines.Insert(editorStateIndex + 2, $"    - {screenName}");
                    return string.Join(Environment.NewLine, lines);
                }
                return content;
            }

            // Find the range of existing screens
            int currentIndex = screensOrderIndex + 1;
            List<string> currentScreens = new List<string>();
            int firstScreenIndex = currentIndex;

            while (currentIndex < lines.Count)
            {
                string line = lines[currentIndex];
                string trimmed = line.Trim();
                
                // Check if it's a list item (starts with "- ") and has correct indentation (at least 4 spaces)
                if (trimmed.StartsWith("- ") && (line.Length - line.TrimStart().Length) >= 4)
                {
                    currentScreens.Add(trimmed.Substring(2));
                    currentIndex++;
                }
                else
                {
                    break;
                }
            }

            if (add)
            {
                if (!currentScreens.Contains(screenName))
                {
                    // Add to the end of the list
                    lines.Insert(currentIndex, $"    - {screenName}");
                }
            }
            else
            {
                int indexInList = currentScreens.IndexOf(screenName);
                if (indexInList != -1)
                {
                    lines.RemoveAt(firstScreenIndex + indexInList);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
