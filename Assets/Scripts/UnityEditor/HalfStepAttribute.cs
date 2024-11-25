using UnityEngine;

namespace UnityEditor {
    public class HalfStepSliderAttribute : PropertyAttribute {
        public float Min { get; }
        public float Max { get; }

        public HalfStepSliderAttribute(float min, float max) {
            Min = min;
            Max = max;
        }
    }
}