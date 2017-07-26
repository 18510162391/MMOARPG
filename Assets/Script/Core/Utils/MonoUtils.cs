using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoUtils
{
    private MonoUtils() { }

    /// <summary>
    ///获取组件，没有的话直接添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        if (go == null)
        {
            return null;
        }

        T returnValue = go.GetComponent<T>();
        if (returnValue != null)
        {
            return returnValue;
        }

        return go.AddComponent<T>();
    }

    /// <summary>
    /// 当前帧立即清除子对象
    /// </summary>
    /// <param name="uigrid"></param>
    public static void ClearChild(Transform trans)
    {
        if (trans.childCount > 0)
        {
            do
            {
                Transform item = trans.GetChild(0);
                GameObject.DestroyImmediate(item.gameObject);
            } while (trans.childCount > 0);
        }
    }

}
