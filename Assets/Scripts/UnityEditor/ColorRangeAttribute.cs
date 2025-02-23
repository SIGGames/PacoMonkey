using UnityEngine;

namespace UnityEditor {
    public class ColorRangeAttribute : PropertyAttribute {
        public readonly float min;
        public readonly float max;
        public readonly Color color;

        public ColorRangeAttribute(float min, float max, float r = 0, float g = 0, float b = 0) {
            this.min = min;
            this.max = max;
            color = new Color(r, g, b);
        }
    }
}