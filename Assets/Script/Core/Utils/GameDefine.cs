using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class GameDefine
{
    private GameDefine() { }

    public static string BuildTemp = "GameData";
    public static string BuildTargetPath = "/StreamingAssets/" + BuildTemp;

    public static string AssetBundleSuffix = ".bytes";
    public static string REMOTE_CDN = "http://127.0.0.1/";

#if UNITY_EDITOR

    #region 打包编辑器

    /// <summary>
    /// 需要打包的资源目录
    /// </summary>
    public static string BuildSourcePath_Res = "/Resources";

    /// <summary>
    /// 需要打包的场景目录
    /// </summary>
    public static string BuildSourcePath_Scene = "/Scene";

    /// <summary>
    /// 打包目标目录
    /// 场景
    /// 
    /// </summary>

    public static string SaveAssetDepPath = BuildTargetPath;
    public static string CommonAssetSavePath = "Common/";

    public static BuildAssetBundleOptions BuildAssetsOption = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;

    #endregion

#endif

    #region 枚举
    #region 资源
    public enum E_ResourceLoadState
    {
        NONE,
        WAITLOAD,
        LOADING,
        LOADED,
    }
    #endregion
    #endregion

    #region delegate
    public delegate void OnResourceUpdateComplete();
    #endregion

}
