using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tools {
    public static class UpdateSelectedColor {
        [MenuItem("Tools/Update Selected Color")]
        public static void UpdateAllSelectedColors() {
            Color color = EditorGUILayoutColorPicker();

            if (!EditorUtility.DisplayDialog("Confirm", $"Apply selected color {color} to all UI elements?", "Yes", "Cancel")) {
                return;
            }

            List<Selectable> allSelectables = GetAllSelectablesInScene();

            foreach (var selectable in allSelectables) {
                var colors = selectable.colors;
                colors.selectedColor = color;
                selectable.colors = colors;

                EditorUtility.SetDirty(selectable);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<Selectable> GetAllSelectablesInScene() {
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>(true);
            List<Selectable> result = new();

            foreach (var go in allGameObjects) {
                Selectable[] selectables = go.GetComponents<Selectable>();
                result.AddRange(selectables);
            }

            return result;
        }

        private static Color EditorGUILayoutColorPicker() {
            var tempWindow = ScriptableObject.CreateInstance<ColorPickerWindow>();
            tempWindow.ShowModal();
            return tempWindow.selectedColor;
        }

        private class ColorPickerWindow : EditorWindow {
            public Color selectedColor = Color.white;
            private bool _colorChosen;

            private void OnGUI() {
                selectedColor = EditorGUILayout.ColorField("Selected Color", selectedColor);

                if (GUILayout.Button("Apply")) {
                    _colorChosen = true;
                    Close();
                }
            }

            private void OnDestroy() {
                if (!_colorChosen) {
                    selectedColor = Color.white;
                }
            }
        }
    }
}