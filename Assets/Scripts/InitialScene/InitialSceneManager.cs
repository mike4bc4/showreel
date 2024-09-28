using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace InitialScene
{
    public class InitialSceneManager : MonoBehaviour
    {
        const float k_MinAnimationTime = 2f;

        [SerializeField] LoadingScreen m_LoadingScreen;

        bool m_LoadingScreenReady;
        Action m_OnLoadingScreenReady;
        float m_LoadingScreenReadyTime;

        void Start()
        {
            m_LoadingScreen.ShowDiamond(OnShowDiamondCompleted);
            SceneLoader.onSceneLoaded += OnSceneLoaded;
            SceneLoader.LoadScene(SceneLoader.MainSceneName);
        }

        void OnShowDiamondCompleted()
        {
            m_LoadingScreenReady = true;
            m_LoadingScreenReadyTime = Time.time;
            m_LoadingScreen.StartDiamondAnimation();
        }

        void OnSceneLoaded(string sceneName)
        {
            if (sceneName != SceneLoader.MainSceneName)
            {
                return;
            }

            if (m_LoadingScreenReady && m_LoadingScreenReadyTime + k_MinAnimationTime < Time.time)
            {
                HideLoadingScreenAndUnload();
            }
            else
            {
                StartCoroutine(HideLoadingScreenAndUnloadAsync());
            }
        }

        IEnumerator HideLoadingScreenAndUnloadAsync()
        {
            yield return new WaitUntil(() => m_LoadingScreenReady && m_LoadingScreenReadyTime + k_MinAnimationTime < Time.time);
            HideLoadingScreenAndUnload();
        }

        void HideLoadingScreenAndUnload()
        {
            m_LoadingScreen.Hide(() =>
            {
                SceneLoader.UnloadScene(SceneLoader.InitialSceneName);
            });
        }
    }
}
