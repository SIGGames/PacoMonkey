using UnityEngine;

namespace UnityEditor {
    [System.Serializable]
    public struct ColorRangeValue {
        public float value;
        public Color color;

        public ColorRangeValue(float value, Color color = default) {
            this.value = value;
            this.color = color.Equals(default) ? Color.black : color;
        }
    }
}