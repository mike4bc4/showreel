using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public class StartSceneSetter
    {
        static StartSceneSetter()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.Log("Unable to set play mode scene - build settings scenes is empty.");
                return;
            }

            var path = EditorBuildSettings.scenes[0].path;
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorSceneManager.playModeStartScene = scene;
        }
    }
}
