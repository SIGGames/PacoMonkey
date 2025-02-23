#if UNITY_EDITOR
using UnityEngine;

namespace UnityEditor.ColorRangeDrawers {
    [CustomPropertyDrawer(typeof(ColorRangeAttribute))]
    [CustomPropertyDrawer(typeof(ColorRangeValue))]
    public class ColorRangeValueDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ColorRangeAttribute rangeAttr = attribute as ColorRangeAttribute;

            SerializedProperty valueProp = property.FindPropertyRelative("value");
            SerializedProperty colorProp = property.FindPropertyRelative("color");

            float sliderWidth = position.width * 0.85f;
            float colorWidth = position.width - sliderWidth - 5f;
            Rect sliderRect = new Rect(position.x, position.y, sliderWidth, position.height);
            Rect colorRect = new Rect(position.x + sliderWidth + 5f, position.y, colorWidth, position.height);

            float min = rangeAttr?.min ?? 0f;
            float max = rangeAttr?.max ?? 1f;
            valueProp.floatValue = EditorGUI.Slider(sliderRect, label, valueProp.floatValue, min, max);

            if (colorProp != null && colorProp.colorValue.Equals(new Color(0, 0, 0, 0))) {
                colorProp.colorValue = Color.white;
            }

            colorProp.colorValue = EditorGUI.ColorField(colorRect, colorProp.colorValue);
        }
    }
}
#endif