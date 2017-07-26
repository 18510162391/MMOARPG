using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<T>();
                if (_instance == null)
                {

                    GameObject go = GameObject.Find("GameStart");
                    //go.hideFlags = HideFlags.DontSave;
                    if (go != null)
                    {
                        _instance = go.AddComponent<T>();
                    }
                }
            }
            return _instance;
        }
    }

    public virtual void OnAwake()
    {
        //DontDestroyOnLoad(this.gameObject);
    }
}
