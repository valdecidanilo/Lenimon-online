using UnityEditor;
using UnityEngine;

namespace GameSettings.Editor
{
    public class GameSettingsWindow : EditorWindow
    {
        private GameSettings settings;

        [MenuItem("Tools/Splicemon/Game Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameSettingsWindow>("Game Settings");
            window.minSize = new Vector2(350, 300);
        }

        private void OnEnable()
        {
            settings = GameSettings.Instance;
        }

        private void OnGUI()
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("GameSettings asset not found in Resources folder.", MessageType.Error);
                if (GUILayout.Button("Create GameSettings.asset"))
                {
                    CreateSettingsAsset();
                }
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Battle and Move Settings", EditorStyles.boldLabel);
            settings.encounterBattleChance = EditorGUILayout.Slider("Encounter Chance", settings.encounterBattleChance, 0f, 1f);
            settings.radiusBattle = EditorGUILayout.FloatField("Battle Radius", settings.radiusBattle);
            settings.moveTurnDelay = EditorGUILayout.FloatField("Move Delay", settings.moveTurnDelay);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            settings.gridSize = EditorGUILayout.FloatField("Grid Size", settings.gridSize);
            settings.radiusGrass = EditorGUILayout.FloatField("Grass Radius", settings.radiusGrass);
            settings.radiusPlayer = EditorGUILayout.FloatField("Radius", settings.radiusPlayer);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings);
            }
        }

        private void CreateSettingsAsset()
        {
            var asset = CreateInstance<GameSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            AssetDatabase.CreateAsset(asset, "Assets/Resources/GameSettings.asset");
            AssetDatabase.SaveAssets();
            settings = asset;
        }
    }
}
