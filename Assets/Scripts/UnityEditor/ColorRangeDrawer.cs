#if UNITY_EDITOR
using UnityEngine;

namespace UnityEditor {
    [CustomPropertyDrawer(typeof(ColorRangeAttribute))]
    public class ColorRangeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ColorRangeAttribute range = (ColorRangeAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Float) {
                EditorGUI.BeginProperty(position, label, property);

                Rect sliderRect = new Rect(position.x, position.y, position.width - 60, position.height);
                Rect colorRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);

                EditorGUI.Slider(sliderRect, property, range.min, range.max, label);

                EditorGUI.DrawRect(colorRect, range.color);

                EditorGUI.EndProperty();
            } else {
                EditorGUI.LabelField(position, label.text, "Use ColoredRange with float.");
            }
        }
    }
}
#endif