#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public static class FullscreenGameView
{
    static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    static EditorWindow instance;

    [MenuItem("Window/General/Game (Fullscreen) %2", priority = 2)]
    public static void Toggle()
    {
        if (GameViewType == null)
        {
            Debug.LogError("GameView type not found.");
            return;
        }

        if (instance != null)
        {
            instance.Close();
            instance = null;
        }
        else
        {
            instance = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);

            var desktopResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            var fullscreenRect = new Rect(Vector2.zero, desktopResolution);
            instance.ShowPopup();
            instance.position = fullscreenRect;
            instance.Focus();
        }
    }
}

#endif