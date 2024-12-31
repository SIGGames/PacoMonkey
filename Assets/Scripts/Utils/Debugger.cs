using System;

namespace Utils {
    public static class Debugger {
        public static void Print(params (string Name, object Value)[] variables) {
            Console.WriteLine("===== Debugger Output =====");
            foreach (var variable in variables) {
                string valueString = variable.Value != null ? variable.Value.ToString() : "null";
                Console.WriteLine($"[Variable: {variable.Name}] -> Value: {valueString}");
            }

            Console.WriteLine("===========================");
        }
    }
}