using UnityEngine;

namespace Utils {
    public static class Debugger {
        public static void Log(params (string Name, object Value)[] variables) {
            foreach (var variable in variables) {
                Debug.Log($"{variable.Name}: {variable.Value}");
            }
        }
    }
}