using UnityEngine;

namespace Utils {
    public static class LayerUtils {
        public static readonly LayerMask Default = LayerMask.GetMask("Default"); // 0
        public static readonly LayerMask TransparentFX = LayerMask.GetMask("TransparentFX"); // 1
        public static readonly LayerMask IgnoreRaycast = LayerMask.GetMask("Ignore Raycast"); // 2
        public static readonly LayerMask Wall = LayerMask.GetMask("Wall"); // 3
        public static readonly LayerMask Water = LayerMask.GetMask("Water"); // 4
        public static readonly LayerMask UI = LayerMask.GetMask("UI"); // 5
        public static readonly LayerMask Zone = LayerMask.GetMask("Zone"); // 6
        public static readonly LayerMask Ground = LayerMask.GetMask("Ground"); // 7

        public static int GetBitMask(int layer) {
            return 1 << layer;
        }
    }
}