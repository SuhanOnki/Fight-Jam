using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NINESOFT.TUTORIAL_SYSTEM
{
    public class NSTutorialSystemInfoWindow : EditorWindow
    {

        [MenuItem("Window/Ninesoft/TutorialSystem")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<NSTutorialSystemInfoWindow>("Tutorial System");
        }

        [MenuItem("GameObject/Create Tutorial Manager with NS")]
        public static void CreateTutorial()
        {
            var obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<TutorialManager>("Assets/NINESOFT_ASSETS/TutorialSystem/Assets/Prefabs/Managers/TutorialManager.prefab"));
            Selection.activeObject = obj;
        }

        private void OnGUI()
        {
            this.minSize = new Vector2(300, 300);
            this.maxSize = new Vector2(300, 300);

            GUIContent gui = new GUIContent("   NINESOFT\n   Tutorial System", NSEditorData.GetIcon("logo"));
            EditorGUILayout.LabelField(gui, EditorStyles.whiteLargeLabel, GUILayout.Height(50f), GUILayout.Width(Screen.width * .8f));
            NSEditorData.DrawUILine();
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            if (GUILayout.Button(new GUIContent("DOCUMENTATION", NSEditorData.GetIcon("doc")), GUILayout.Height(40f)))
            {
                Application.OpenURL("https://9ninesoft9.blogspot.com/2023/07/tutorial-system.html");
            }
            if (GUILayout.Button(new GUIContent("OUR OTHER ASSETS", NSEditorData.GetIcon(4)), GUILayout.Height(40f)))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/28895");
            }

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            NSEditorData.DrawUILine();
            EditorGUILayout.LabelField("THANKS FOR PURCHASING!", EditorStyles.whiteLargeLabel, GUILayout.Height(20f));
            NSEditorData.DrawUILine();

            EditorGUILayout.LabelField(NSPackageInfo.PackageName + " | Version " + NSPackageInfo.Version, EditorStyles.centeredGreyMiniLabel);
        }

    }
}
