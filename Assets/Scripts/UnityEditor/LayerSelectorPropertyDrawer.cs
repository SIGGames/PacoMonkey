#if UNITY_EDITOR
using UnityEngine;

namespace UnityEditor {
    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    public class LayerSelectorPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            LayerSelectorAttribute layerSelector = (LayerSelectorAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Integer) {
                if (layerSelector.UseDefaultLayerFieldDrawer) {
                    property.intValue = EditorGUI.LayerField(position, label, property.intValue);
                }
                else {
                    EditorGUI.BeginProperty(position, label, property);

                    // Create the list of layers
                    string[] layerList = GetLayerNames();
                    int index = Mathf.Clamp(property.intValue, 0, layerList.Length - 1);

                    // Display popup
                    int newIndex = EditorGUI.Popup(position, label.text, index, layerList);

                    // Set the new value
                    property.intValue = LayerMask.NameToLayer(layerList[newIndex]);

                    EditorGUI.EndProperty();
                }
            }
            else {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private string[] GetLayerNames() {
            string[] layerNames = new string[32];
            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName)) {
                    layerNames[i] = layerName;
                }
                else {
                    layerNames[i] = "<NoLayer>";
                }
            }

            return layerNames;
        }
    }
}
#endif