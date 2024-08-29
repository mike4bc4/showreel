using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
    static T s_Instance;

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = Resources.Load<T>(typeof(T).Name);
                if (s_Instance == null)
                {
                    throw new Exception($"Unable to load '{typeof(T).Name}' asset.");
                }
            }

            return s_Instance;
        }
    }
}
