using UnityEngine;

namespace UnityEditor.ColorRangeDrawers {
    public class ColorRangeAttribute : PropertyAttribute {
        public readonly float min;
        public readonly float max;

        public ColorRangeAttribute(float min, float max) {
            this.min = min;
            this.max = max;
        }
    }
}