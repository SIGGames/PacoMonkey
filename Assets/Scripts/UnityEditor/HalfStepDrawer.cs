using UnityEngine;

namespace UnityEditor {
    [CustomPropertyDrawer(typeof(HalfStepSliderAttribute))]
    public class HalfStepSliderDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType == SerializedPropertyType.Float) {
                EditorGUI.BeginProperty(position, label, property);

                HalfStepSliderAttribute halfStep = (HalfStepSliderAttribute)attribute;
                float min = halfStep.Min;
                float max = halfStep.Max;

                EditorGUI.BeginChangeCheck();
                float value = EditorGUI.Slider(position, label, property.floatValue, min, max);
                value = Mathf.Round(value * 2) / 2;

                if (EditorGUI.EndChangeCheck()) {
                    property.floatValue = value;
                    property.serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.EndProperty();
            } else {
                EditorGUI.LabelField(position, label.text, "Use [HalfStepSlider] with float.");
            }
        }
    }
}