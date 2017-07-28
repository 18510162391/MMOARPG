using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathUtils
{
    private PathUtils() { }

    /// <summary>
    /// 获取Asset下的完成路径
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static string GetAssetCompletePath(string relativePath)
    {
        string returnValue = string.Format("{0}{1}", Application.dataPath, relativePath);
        return returnValue;

        //return Path.Combine(Application.dataPath, relativePath);
    }

    /// <summary>
    /// 获取某个文件的路径
    /// 例：Assets/Resources/cube.prefab 返回Assets/Resources/
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string getPath(string filePath)
    {
        string path = filePath.Replace("\\", "/");
        int index = path.LastIndexOf("/");
        if (-1 == index)
            throw new Exception("can not find /!!!");
        return path.Substring(0, index + 1);
    }

    /// <summary>
    /// 检查路径
    /// 如果不存在，则创建该路径
    /// </summary>
    /// <param name="completePath"></param>
    public static void CheckPath(string completePath)
    {
        bool exist = Directory.Exists(completePath);
        if (!exist)
        {
            Directory.CreateDirectory(completePath);
        }
    }

    /// <summary>
    /// 获取指定文件的后缀
    /// </summary>
    /// <param name="file"></param>
    public static string GetFileSuffix(string file)
    {
        string returnVlaue = string.Empty;

        int index = file.LastIndexOf('.');
        if (index >= 0)
        {
            returnVlaue = file.Substring(index);
        }

        return returnVlaue;
    }

    /// <summary>
    /// 获取文件名称
    /// </summary>
    /// <param name="filePath">文件的路径</param>
    /// <param name="suffix">是否需要包含文件后缀名</param>
    /// <returns></returns>

    public static string GetFileName(string filePath, bool suffix)
    {
        string returnValue = filePath;
        filePath = filePath.Replace("\\", "/");
        int index = filePath.LastIndexOf("/");
        if (index > 0)
        {
            returnValue = filePath.Substring(index + 1);
        }

        string strSuffix = GetFileSuffix(filePath);

        if (!suffix && !string.IsNullOrEmpty(strSuffix))
        {
            returnValue = returnValue.Replace(strSuffix, "");
        }

        return returnValue;
    }



    /// <summary>
    /// 获取相对于Assets的路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetAssetPath(string filePath)
    {
        string returnValue = string.Empty;

        int index = filePath.LastIndexOf("Assets");
        if (index >= 0)
        {
            returnValue = filePath.Substring(index);
        }

        return returnValue;
    }

    internal static string GetResRootPath()
    {
        if (Application.isEditor)
        {
            return "file://" + Application.streamingAssetsPath + "/" + GameDefine.BuildTemp;
        }
        else
        {
            return "file://" + Application.persistentDataPath + "/" + GameDefine.BuildTemp;
        }
        return null;
    }
    internal static string GetFileRootPath()
    {

//#if UNITY_EDITOR
//        return Application.streamingAssetsPath + "/" + GameDefine.BuildTemp;
//#endif
//        return Application.persistentDataPath + "/" + GameDefine.BuildTemp;

        if (Application.isEditor)
        {
            return Application.persistentDataPath + "/" + GameDefine.BuildTemp;
        }
        else
        {
            return Application.persistentDataPath + "/" + GameDefine.BuildTemp;
        }
        return null;
    }
}
