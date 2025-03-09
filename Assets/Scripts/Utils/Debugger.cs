using UnityEngine;

namespace Utils {
    public static class Debugger {
        public static void Log(params (string Name, object Value)[] variables) {
            foreach (var variable in variables) {
                Debug.Log($"{variable.Name}: {variable.Value}");
            }
        }

        public static void LogIfNull(params (string Name, object Value)[] variables) {
            foreach (var variable in variables) {
                if (variable.Value == null) {
                    Debug.Log($"The '{variable.Name}' is null");
                }
            }
        }
    }
}