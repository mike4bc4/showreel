using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class CoroutineRunner : MonoBehaviour
{
    static CoroutineRunner s_Instance;

    public static CoroutineRunner instance
    {
        get
        {
            if (s_Instance == null)
            {
                var runner = FindAnyObjectByType<CoroutineRunner>(FindObjectsInactive.Include);
                if (runner == null)
                {
                    var runnerGameObject = new GameObject("CoroutineRunner");
                    runner = runnerGameObject.AddComponent<CoroutineRunner>();
                    runnerGameObject.hideFlags |= HideFlags.NotEditable;
                }

                s_Instance = runner;
            }

            // if (!s_Instance.isActiveAndEnabled || s_Instance.transform.parent != null)
            // {
            //     s_Instance.transform.SetParent(null);
            //     s_Instance.gameObject.SetActive(true);
            //     s_Instance.enabled = true;
            //     Debug.Log($"'{s_Instance}' and its Game Object are in use and cannot be disabled or moved.");
            // }

            return s_Instance;
        }
    }

    void OnEnable()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
        }
        else if (s_Instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}
