using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System;

public class BuildAssetBundle
{
    private static BuildTarget _BuildTarget = BuildTarget.StandaloneWindows64;

    private static List<string> _ScenePaths = new List<string>();
    private static List<string> _ResPaths = new List<string>();
    public static List<string> CommonrResPaths = new List<string>();

    private static Dictionary<int, List<AssetUnit>> _DicAssetInfo = new Dictionary<int, List<AssetUnit>>();


    [MenuItem("Tools/Build/Window")]
    public static void BuildWindow()
    {
        _BuildTarget = BuildTarget.StandaloneWindows64;
        _BuildAssetBundle();
    }

    [MenuItem("Tools/Build/IOS")]
    public static void BuildIOS()
    {
        _BuildTarget = BuildTarget.iOS;
        _BuildAssetBundle();
    }

    [MenuItem("Tools/Build/Android")]
    public static void BuildAndroid()
    {
        _BuildTarget = BuildTarget.Android;
        _BuildAssetBundle();
    }
    [MenuItem("Tools/Build/BuildFile")]

    public static void BuildFiles()
    {
        AssetImporter resources = AssetImporter.GetAtPath("Assets/Build/Resources.txt");
        resources.assetBundleName = "Resources" + GameDefine.AssetBundleSuffix;

        AssetImporter AllAssetsDep = AssetImporter.GetAtPath("Assets/Build/AllAssetsDep.txt");
        AllAssetsDep.assetBundleName = "AllAssetsDep" + GameDefine.AssetBundleSuffix;

        string str = GetBuildPath();
        BuildPipeline.BuildAssetBundles(str, BuildAssetBundleOptions.None, _BuildTarget);

        File.Delete("Assets/Build/Resources.txt");
        File.Delete("Assets/Build/AllAssetsDep.txt");
        _DeleteManifest();
        AssetDatabase.Refresh();
    }


    private static void _BuildAssetBundle()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();

        //1、获取需要打包的资源
        _GetResList();

        //2、获取需要打包的场景
        _GetSceneList();

        //3、生成AssetUnit
        _GetAllAssetUnit();

        //4、生成资源依赖XML信息
        _SaveAssetUnit();

#if UNITY_5
        //5.1设置AssetBundle的名称
        _SetAssetBundleName();
#endif
        //6、打包
        _BeginBuild();

        //7 打包后生成资源列表文件 并写入MD5
        _CreateFileInfo();

        _DeleteManifest();
    }

    /// <summary>
    /// 生成资源列表文件，并写入md5
    /// </summary>
    private static void _CreateFileInfo()
    {
        string[] names = AssetDatabase.GetAllAssetBundleNames();

        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("Resources");

        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            string fileName = PathUtils.GetFileName(name, false);
            string md5 = _GetMD5(Application.dataPath + GameDefine.BuildTargetPath + "/" + name);

            XmlElement node = doc.CreateElement("ResItem");
            node.SetAttribute("name", fileName);
            node.SetAttribute("path", name);
            node.SetAttribute("size", _GetFileSize(Application.dataPath + GameDefine.BuildTargetPath + "/" + name));
            node.SetAttribute("md5", md5);

            root.AppendChild(node);
        }

        doc.AppendChild(root);

        string savePath = PathUtils.GetAssetCompletePath(GameDefine.BuildTargetPath);
        PathUtils.CheckPath(savePath);

        savePath = savePath + "/Resources.txt";
        doc.Save(savePath);

        AssetDatabase.Refresh();
    }

    private static string _GetFileSize(string filePath)
    {
        long size = 0;
        if (File.Exists(filePath))
        {
            FileInfo info = new FileInfo(filePath);

            size = info.Length;

            long sizeBytes = File.ReadAllBytes(filePath).LongLength;
        }
        return size.ToString();
    }

    private static string GetBuildPath()
    {
        return Application.dataPath + GameDefine.BuildTargetPath;
    }

    private static string _GetMD5(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    /// <summary>
    /// 从所有依赖资源中分析出公用的资源
    /// </summary>
    private static void _GetCommonAssets(string[] allResDeps)
    {
        CommonrResPaths.Clear();

        List<string> allResDep = new List<string>();
        allResDep.AddRange(allResDeps);

        for (int i = 0; i < allResDep.Count; i++)
        {
            string depPath = allResDeps[i];

            if (allResDep.FindAll(p => p == depPath).Count > 1)
            {
                //存在多个资源共享这一个依赖
                CommonrResPaths.Add(depPath);
            }
        }
    }

    /// <summary>
    /// 如果资源拥有名称则忽略
    /// 如果没有，则根据资源名称设置
    /// </summary>
    private static void _SetAssetBundleName()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();

        foreach (var item in _DicAssetInfo)
        {
            List<AssetUnit> assetUnit = item.Value;

            for (int i = 0; i < assetUnit.Count; i++)
            {
                AssetUnit unit = assetUnit[i];

                string resPath = unit.AssetPath;

                //设置资源名称
                _SetAssetBundleName(resPath, resPath);
            }
        }

        //for (int i = 0; i < _ResPaths.Count; i++)
        //{
        //    string resPath = _ResPaths[i];

        //    //设置资源名称
        //    _SetAssetBundleName(resPath, resPath);

        //    //依赖资源设置资源名称
        //    string[] depRes = AssetDatabase.GetDependencies(resPath);

        //    for (int j = 0; j < depRes.Length; j++)
        //    {
        //        string depPath = depRes[j];
        //        string bundleName = string.Empty;

        //        if (CommonrResPaths.Contains(depPath))
        //        {
        //            //此资源是共享资源，放在common文件夹下面
        //            bundleName = Path.Combine(GameDefine.CommonAssetSavePath, PathUtils.GetFileName(depPath, true));

        //            _SetAssetBundleName(depPath, bundleName);
        //        }
        //    }
        //}
    }


    /// <summary>
    /// 设置指定目录下资源的资源名称
    /// 例：Assets/Resources/Model/cube.prefab 设置成Model/cube.bytes
    /// </summary>
    /// <param name="AssetPath"></param>
    private static void _SetAssetBundleName(string AssetPath, string bundleName)
    {
        AssetImporter pImporter = AssetImporter.GetAtPath(AssetPath);

        if (pImporter == null)
        {
            Debug.LogError("_SetAssetBundleName 失败 无法找到AssetImporter 路径=" + AssetPath);
            return;
        }

        string assetName = string.Empty;
        string suffix = PathUtils.GetFileSuffix(AssetPath);

        if (CommonrResPaths.Contains(AssetPath))
        {
            assetName = GameDefine.CommonAssetSavePath + PathUtils.GetFileName(AssetPath, true);
        }
        else
        {
            if (suffix == ".unity")
            {
                //如果是场景的话，放在Scene文件夹下。
                assetName = "Scene" + PathUtils.GetFileName(AssetPath, true);
            }
            else
            {
                //如果是资源的话，放在对应的文件夹下
                assetName = bundleName.Replace("Assets" + GameDefine.BuildSourcePath_Res + "/", "");
            }
        }
        assetName = assetName.Replace(suffix, GameDefine.AssetBundleSuffix);

        if (!string.IsNullOrEmpty(pImporter.assetBundleName) && pImporter.assetBundleName != assetName.ToLower())
        {
            //删除老的ab包
            string completePath = Application.dataPath + "" + GameDefine.BuildTargetPath + "/" + pImporter.assetBundleName;
            if (File.Exists(completePath))
            {
                File.Delete(completePath);
                Debug.Log("删除成功 " + completePath);
            }
        }

        pImporter.assetBundleName = assetName;

    }

    /// <summary>
    /// 删除unity5打包相关的Manifest文件
    /// </summary>
    private static void _DeleteManifest()
    {
        string[] paths = Directory.GetFiles(Application.dataPath + GameDefine.BuildTargetPath, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < paths.Length; i++)
        {
            string pathItem = paths[i];

            string suff = PathUtils.GetFileSuffix(pathItem);
            string fileName = PathUtils.GetFileName(pathItem, false);
            if (suff == ".meta" || suff == GameDefine.AssetBundleSuffix)
            {
                continue;
            }
            if (suff == ".manifest" || fileName == GameDefine.BuildTargetPath.Replace("/", ""))
            {
                File.Delete(pathItem);
            }
        }

        AssetDatabase.Refresh();
    }

    private static void _BeginBuild()
    {
#if UNITY_5

        //资源打包
        string buildPath = Application.dataPath + GameDefine.BuildTargetPath;
        PathUtils.CheckPath(buildPath);

        BuildPipeline.BuildAssetBundles(buildPath, BuildAssetBundleOptions.None, _BuildTarget);

        AssetDatabase.Refresh();

#elif UNITY_4
        int levelTotal = _DicAssetInfo.Count;

        for (int level = 1; level <= levelTotal; level++)
        {
            BuildPipeline.PushAssetDependencies();

            List<AssetUnit> Assets = _DicAssetInfo[level];

            for (int i = 0; i < Assets.Count; i++)
            {
                AssetUnit unit = Assets[i];


                //生成路径
                string savePath = Application.dataPath + GameDefine.BuildTargetPath + unit.AssetPath.Replace("Assets", "") + GameDefine.AssetBundleSuffix;

                string BuildName = unit.FileName + unit.Suffix;

                PathUtils.CheckPath(PathUtils.getPath(savePath));

                if (unit.Suffix != ".unity")
                {
                    //资源打包
                    //获取资源
                    Object obj = AssetDatabase.LoadMainAssetAtPath(unit.AssetPath);
                    if (obj == null)
                    {
                        Debug.LogError("打包错误，无法找到主资源 路径：" + unit.AssetPath);
                        continue;
                    }
                    BuildPipeline.BuildAssetBundleExplicitAssetNames(new Object[] { obj }, new string[] { BuildName }, savePath, GameDefine.BuildAssetsOption, _BuildTarget);
                }
                else
                {
                    //场景打包

                    BuildPipeline.PushAssetDependencies();

                    BuildPipeline.BuildStreamedSceneAssetBundle(new string[] { unit.AssetPath }, savePath, _BuildTarget);

                    BuildPipeline.PopAssetDependencies();
                }
            }
        }

        for (int level = 1; level <= levelTotal; level++)
        {
            BuildPipeline.PopAssetDependencies();
        }
        AssetDatabase.Refresh();
#endif

    }

    private static void _SaveAssetUnit()
    {
        int totalLevel = _DicAssetInfo.Count;

        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("AllAssetsDep");

        for (int level = 1; level <= totalLevel; level++)
        {
            List<AssetUnit> Assets = _DicAssetInfo[level];

            for (int i = 0; i < Assets.Count; i++)
            {
                AssetUnit unit = Assets[i];

                XmlElement node = doc.CreateElement("AssetUnit");
                node.SetAttribute("Index", unit.Index.ToString());
                node.SetAttribute("FileName", unit.FileName);
                node.SetAttribute("Suffix", unit.Suffix);
                node.SetAttribute("Level", unit.Level.ToString());

                string depString = _GetAssetDepString(unit);
                node.SetAttribute("Deps", depString);
                root.AppendChild(node);

                unit.DepString = depString;
            }
        }

        doc.AppendChild(root);

        string savePath = PathUtils.GetAssetCompletePath(GameDefine.SaveAssetDepPath);
        PathUtils.CheckPath(savePath);

        savePath = savePath + "/AllAssetsDep.txt";
        doc.Save(savePath);
    }




    private static string _GetAssetDepString(AssetUnit asset)
    {
        StringBuilder builder = new StringBuilder();

        List<string> deps = asset.DepResList;
        List<AssetUnit> Assets = new List<AssetUnit>();

        for (int i = 0; i < deps.Count; i++)
        {
            string assetPath = deps[i];

            AssetUnit unit = _GetAssetUnit(assetPath);

            if (unit != null)
            {
                Assets.Add(unit);
            }
        }

        if (Assets.Count > 0)
        {
            Assets.Sort(SortAssetByLevel);
        }

        for (int i = 0; i < Assets.Count; i++)
        {
            builder.Append(Assets[i].Index);

            if (i < Assets.Count - 1)
            {
                builder.Append(",");
            }
        }
        return builder.ToString();
    }

    private static int SortAssetByLevel(AssetUnit x, AssetUnit y)
    {
        if (x.Level > y.Level)
        {
            return 1;
        }
        else if (x.Level < y.Level)
        {
            return -1;
        }
        return 0;
    }

    private static AssetUnit _GetAssetUnit(string assetPath)
    {
        AssetUnit returnValue = null;
        foreach (var item in _DicAssetInfo)
        {
            List<AssetUnit> assets = item.Value;
            returnValue = assets.Find(p => p.AssetPath == assetPath);
            if (returnValue != null)
            {
                break;
            }
        }

        return returnValue;
    }

    private static void _GetAllAssetUnit()
    {
        _DicAssetInfo.Clear();

        List<string> allRes = new List<string>();
        allRes.AddRange(_ScenePaths);
        allRes.AddRange(_ResPaths);

        if (allRes.Count <= 0)
        {
            return;
        }

        //1、初始化所有的AssetUnit

        string[] allResDeps = AssetDatabase.GetDependencies(allRes.ToArray());

        _GetCommonAssets(allResDeps);

        Dictionary<int, List<AssetUnit>> dicAllAsset = new Dictionary<int, List<AssetUnit>>();
        for (int i = 0; i < allResDeps.Length; i++)
        {
            string filePath = allResDeps[i];

            if (CommonrResPaths.Contains(filePath) || allRes.Contains(filePath))
            {

            }

            AssetUnit unit = new AssetUnit(filePath);

            List<AssetUnit> assetList = null;
            if (dicAllAsset.TryGetValue(unit.Level, out assetList))
            {
                assetList.Add(unit);
            }
            else
            {
                assetList = new List<AssetUnit>();
                assetList.Add(unit);
                dicAllAsset.Add(unit.Level, assetList);
            }
        }

        //2、按照资源层级排序
        for (int level = 1; level <= dicAllAsset.Count; level++)
        {
            if (dicAllAsset.ContainsKey(level))
            {
                _DicAssetInfo.Add(level, dicAllAsset[level]);
            }
        }

        //3、资源进行编号
        int index = 0;
        foreach (var item in _DicAssetInfo)
        {
            List<AssetUnit> assets = item.Value;

            for (int i = 0; i < assets.Count; i++)
            {
                AssetUnit asset = assets[i];
                index++;
                asset.Index = index;
            }
        }
    }

    private static void _GetSceneList()
    {
        _ScenePaths.Clear();

        string completePath = PathUtils.GetAssetCompletePath(GameDefine.BuildSourcePath_Scene);
        PathUtils.CheckPath(completePath);

        string[] scenes = Directory.GetFiles(completePath, "*.unity", SearchOption.AllDirectories);

        for (int i = 0; i < scenes.Length; i++)
        {
            string file = scenes[i];

            string suffix = PathUtils.GetFileSuffix(file);
            string fileName = PathUtils.GetFileName(file, false);
            if (suffix == ".meta" || fileName == "GameStart")
            {
                continue;
            }

            file = file.Replace("\\", "/");

            file = PathUtils.GetAssetPath(file);
            ///Assets/Scene/TestScene.unity
            _ScenePaths.Add(file);
        }
    }

    private static void _GetResList()
    {
        _ResPaths.Clear();

        string completePath = PathUtils.GetAssetCompletePath(GameDefine.BuildSourcePath_Res);
        PathUtils.CheckPath(completePath);

        string[] res = Directory.GetFiles(completePath, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < res.Length; i++)
        {
            string file = res[i];

            file = file.Replace("\\", "/");

            string suffix = PathUtils.GetFileSuffix(file);
            if (suffix == ".meta")
            {
                continue;
            }

            file = PathUtils.GetAssetPath(file);
            _ResPaths.Add(file);
        }
    }

    /// <summary>
    /// 获取资源的依赖层级
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static int GetAssetLevel(string filePath)
    {
        int returnValue = 1;
        string assetPath = PathUtils.GetAssetPath(filePath);
        if (!string.IsNullOrEmpty(assetPath))
        {
            string[] deps = AssetDatabase.GetDependencies(new string[] { assetPath });

            if (deps.Length == 1)
            {
                //依赖只有自己
                returnValue = 1;
            }
            else
            {
                for (int i = 0; i < deps.Length; i++)
                {
                    string depItem = deps[i];
                    if (depItem == assetPath)
                    {
                        continue;
                    }

                    int level = GetAssetLevel(depItem);

                    returnValue = level >= returnValue ? level + 1 : returnValue;
                }
            }
        }
        return returnValue;
    }
}

public class MyPostprocessor : AssetPostprocessor
{
    void OnPostprocessAssetbundleNameChanged(string path,
            string previous, string next)
    {
        Debug.Log("Changed: " + path + " old: " + previous + " new: " + next);
    }
}

public class AssetUnit
{
    public int Index;//资源编号
    public string FileName;//资源名称,不包括后缀
    public string AssetPath;//相对于Assets的路径
    public int Level;//资源层级
    public string DepString;//依赖资源索引字符串
    public List<string> DepResList;//包含依赖资源的路径
    public string Suffix;

    public AssetUnit(string filePath)
    {
        FileName = PathUtils.GetFileName(filePath, false);
        Level = BuildAssetBundle.GetAssetLevel(filePath);
        AssetPath = PathUtils.GetAssetPath(filePath);
        Suffix = PathUtils.GetFileSuffix(filePath);
        string[] deps = AssetDatabase.GetDependencies(new string[] { AssetPath });

        DepResList = new List<string>();
        for (int i = 0; i < deps.Length; i++)
        {
            string depItem = deps[i];
            if (depItem == AssetPath || !BuildAssetBundle.CommonrResPaths.Contains(depItem))
            {
                continue;
            }
            DepResList.Add(depItem);
        }
    }
}
