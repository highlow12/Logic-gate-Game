using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IUtil.ProjectWindow;
using UnityEditor;
using UnityEngine;

namespace IUtil.CustomEditor
{
    public static class FolderCustomContextMenu
    {
		/// <summary>
		/// Determine whether to reveal or not
		/// </summary>
		[MenuItem("Assets/IUtil/Customize Folder", true)]
        private static bool ValidateOpenEditor()
        {
            if (Selection.assetGUIDs.Length != 1)
                return false;

            string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            return AssetDatabase.IsValidFolder(path);
        }

        [MenuItem("Assets/IUtil/Customize Folder", false, 1000)]
        private static void OpenEditor()
        {
            string guid = Selection.assetGUIDs[0];
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            FolderCustomEditorWindow.Open(folderPath);
        }
    }

    public class FolderCustomEditorWindow : EditorWindow
    {
        private string folderPath;

        public static void Open(string path)
        {
            var window = GetWindow<FolderCustomEditorWindow>("Folder Editor");
            window.folderPath = path;

            Vector2 size = new Vector2(400, 300);
            window.minSize = size;
            window.maxSize = size;

            window.Show();
        }

        private void OnGUI()
		{
			GUIStyle centeredBoldLabel = new GUIStyle(EditorStyles.boldLabel);
			centeredBoldLabel.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			GUILayout.Label("Folder Path ( Drag a folder below )", centeredBoldLabel, GUILayout.ExpandWidth(true));
			GUILayout.Space(10);

			DrawFolderDropBox();

			GUILayout.Space(10);
			GUILayout.Label("Folder Colors", centeredBoldLabel, GUILayout.ExpandWidth(true));

			DrawFolderGrid(FolderConfigLoader.ColoredFolders, 6, position.width);
            
			GUILayout.Label("Folder Icons", centeredBoldLabel, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);

			DrawFolderGrid(FolderConfigLoader.Icons, 9, position.width);

			GUILayout.Space(10);
            DrawResetButton();
		}

        private void DrawFolderDropBox()
		{
            GUI.enabled = false;
			Rect dropArea = EditorGUILayout.GetControlRect();
			EditorGUI.TextField(dropArea, folderPath);

			GUI.enabled = true;

			Event evt = Event.current;

			switch (evt.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!dropArea.Contains(evt.mousePosition))
						return;

					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					if (evt.type == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();

						foreach (string draggedPath in DragAndDrop.paths)
						{
							if (Directory.Exists(draggedPath))
							{
								folderPath = draggedPath;
								break;
							}
						}
					}
					Event.current.Use();
					break;
			}
		}

        private void DrawFolderGrid<T>(Dictionary<T, Texture2D> dict, int iconsPerRow, float width) where T : System.Enum
        {
            var values = (T[])System.Enum.GetValues(typeof(T));
            int count = dict.Count(kv => kv.Value != null && Convert.ToInt32(kv.Key) >= 0);
            int rows = Mathf.CeilToInt((float)count / iconsPerRow);
            int drawn = 0;

            foreach (int y in Enumerable.Range(0, rows))
            {
                EditorGUILayout.BeginHorizontal();
				GUILayout.Space(20f);

				for (int x = 0; x < iconsPerRow; x++)
                {
                    if (drawn >= count) break;

                    int enumIndex = y * iconsPerRow + x;
                    if (enumIndex >= values.Length) break;

                    var key = values[enumIndex];
                    if (Convert.ToInt32(key) < 0) continue;

                    if (dict.TryGetValue(key, out var tex) && tex != null)
                    {
                        if (GUILayout.Button(tex, GUIStyle.none, GUILayout.Width((width -40f)/ iconsPerRow), GUILayout.Height((width - 40f) / iconsPerRow)))
                        {
                            FolderConfigLoader.SetCustomFolderConfig<T>(folderPath, drawn);
                        }
                        drawn++;
                    }
                }

				GUILayout.Space(20f);
				EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawResetButton()
        {
            if (GUILayout.Button("Reset Custom"))
            {
                FolderConfigLoader.ResetCustom(folderPath);
            }
        }
    }
}