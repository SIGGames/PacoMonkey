﻿using UnityEngine;

namespace UI {
    public class QuitGame : MonoBehaviour {
        public void Quit() {
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}