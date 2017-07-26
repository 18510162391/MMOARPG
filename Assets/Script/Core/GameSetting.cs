using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetting
{
    private GameSetting() { }
    public static float ResReleaseCacheTime = 10f;//模型资源释放缓冲时间

    /// <summary>
    /// 是否开启资源监控，默认编辑器下才会开启
    /// </summary>
    public static bool EnableResMonitor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#endif
            return false;
        }
    }

    /// <summary>
    /// 是否开启资源更新，默认编辑器下不会开启
    /// </summary>
    public static bool EnableResUpdate
    {
        get
        {
#if UNITY_EDITOR
            return false;
#endif
            return true;
        }
    }

    /// <summary>
    /// 是否将资源移动到Application.persistentDataPath目录下
    /// </summary>
    public static bool EnableMoveResToPersistentDataPath
    {
        get
        {
#if UNITY_EDITOR
            return false;
#endif
            return true;
        }
    }
}
