﻿using System.Globalization;
using Mechanics;
using UnityEditor;
using UnityEngine;

namespace Editor {
    [CustomEditor(typeof(PatrolPath))]
    public class PatrolPathGizmo : UnityEditor.Editor {
        public void OnSceneGUI() {
            var path = target as PatrolPath;
            using (var cc = new EditorGUI.ChangeCheckScope()) {
                if (path != null) {
                    var sp = path.transform.InverseTransformPoint(
                        Handles.PositionHandle(path.transform.TransformPoint(path.startPosition), path.transform.rotation));
                    var ep = path.transform.InverseTransformPoint(
                        Handles.PositionHandle(path.transform.TransformPoint(path.endPosition), path.transform.rotation));
                    if (cc.changed) {
                        sp.y = 0;
                        ep.y = 0;
                        path.startPosition = sp;
                        path.endPosition = ep;
                    }
                }
            }

            if (path != null)
                Handles.Label(path.transform.position, (path.startPosition - path.endPosition).magnitude.ToString(CultureInfo.InvariantCulture));
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void OnDrawGizmo(PatrolPath path, GizmoType gizmoType) {
            var start = path.transform.TransformPoint(path.startPosition);
            var end = path.transform.TransformPoint(path.endPosition);
            Handles.color = Color.yellow;
            Handles.DrawDottedLine(start, end, 5);
            Handles.DrawSolidDisc(start, path.transform.forward, 0.1f);
            Handles.DrawSolidDisc(end, path.transform.forward, 0.1f);
        }
    }
}