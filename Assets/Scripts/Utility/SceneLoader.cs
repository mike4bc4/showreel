using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class SceneLoader : MonoBehaviour
    {
        public static event Action<string> onSceneLoaded;
        public static event Action<string> onSceneUnloaded;

        static SceneLoader s_Instance;

        [SerializeField] string m_InitialSceneName;
        [SerializeField] string m_MainSceneName;

        public static SceneLoader Instance
        {
            get => s_Instance;
        }

        public static string InitialSceneName
        {
            get => s_Instance.m_InitialSceneName;
        }

        public static string MainSceneName
        {
            get => s_Instance.m_MainSceneName;
        }

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void LoadScene(string sceneName)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.completed += op => onSceneLoaded?.Invoke(sceneName);
        }

        public static void UnloadScene(string sceneName)
        {
            var asyncOp = SceneManager.UnloadSceneAsync(sceneName);
            asyncOp.completed += op => onSceneUnloaded?.Invoke(sceneName);
        }
    }
}
