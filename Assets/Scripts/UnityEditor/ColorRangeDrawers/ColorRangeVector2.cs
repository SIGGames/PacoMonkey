using UnityEngine;

namespace UnityEditor.ColorRangeDrawers {
    [System.Serializable]
    public struct ColorRangeVector2 {
        public Vector2 value;
        public Color color;

        public ColorRangeVector2(Vector2 value, Color color = default) {
            this.value = value;
            this.color = color.Equals(default) ? Color.black : color;
        }
    }
}