#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace UnityEditor.ColorRangeDrawers {
    [CustomPropertyDrawer(typeof(ColorRangeVector2))]
    public class ColorRangeVector2Drawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ColorRangeAttribute rangeAttr = fieldInfo.GetCustomAttributes(typeof(ColorRangeAttribute), true).Length > 0
                ? (ColorRangeAttribute)fieldInfo.GetCustomAttributes(typeof(ColorRangeAttribute), true)[0]
                : null;
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            SerializedProperty colorProp = property.FindPropertyRelative("color");
            float totalWidth = position.width;
            float sliderWidth = totalWidth * 0.85f;
            float colorWidth = totalWidth - sliderWidth - 5f;
            Rect sliderRectX = new Rect(position.x, position.y, sliderWidth, EditorGUIUtility.singleLineHeight);
            Rect sliderRectY = new Rect(position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, sliderWidth,
                EditorGUIUtility.singleLineHeight);
            Rect colorRect = new Rect(position.x + sliderWidth + 5f, position.y, colorWidth,
                EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing);
            Vector2 vec = valueProp.vector2Value;
            float min = rangeAttr != null ? rangeAttr.min : 0f;
            float max = rangeAttr != null ? rangeAttr.max : 1f;
            vec.x = EditorGUI.Slider(sliderRectX, label.text + " X", vec.x, min, max);
            vec.y = EditorGUI.Slider(sliderRectY, label.text + " Y", vec.y, min, max);
            valueProp.vector2Value = vec;
            if (colorProp != null && colorProp.colorValue.Equals(new Color(0, 0, 0, 0))) {
                colorProp.colorValue = Color.white;
            }

            colorProp.colorValue = EditorGUI.ColorField(colorRect, colorProp.colorValue);
        }
    }
}
#endif