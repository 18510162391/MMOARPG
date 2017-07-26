using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

public class AssetInfo
{
    private int mIndex;//资源索引
    private string mName;//资源名称
    private int mLevel;//资源等级
    private List<int> mDependence = new List<int>();//依赖
    private int mAssetBundleSize;//AB包大小

    public void Import(XmlElement element)
    {
        this.mIndex = int.Parse(element.GetAttribute("Index"));
        this.mName = element.GetAttribute("FileName");
        this.mLevel = int.Parse(element.GetAttribute("Level"));
        //this.mAssetBundleSize = int.Parse(element.GetAttribute("bundlesize"));

        string dependence = element.GetAttribute("Deps");
        if (!string.IsNullOrEmpty(dependence))
        {
            string[] dep = dependence.Split(',');
            for (int i = 0; i < dep.Length; i++)
            {
                mDependence.Add(int.Parse(dep[i]));
            }
        }
    }

    public int Index
    {
        get
        {
            return this.mIndex;
        }
    }
    public string Name
    {
        get
        {
            return this.mName;
        }
    }

    public int Level
    {
        get
        {
            return this.mLevel;
        }
    }

    public int AssetBunleSize
    {
        get
        {
            return this.mAssetBundleSize;
        }
    }

    public List<int> Dependence
    {
        get
        {
            return this.mDependence;
        }
    }
}

public class AssetInfoManager : Singleton<AssetInfoManager>
{
    private Dictionary<string, AssetInfo> mDicNameAsset = new Dictionary<string, AssetInfo>();
    private Dictionary<int, AssetInfo> mDicIndexAsset = new Dictionary<int, AssetInfo>();

    private List<string> _tempDeps;

    public AssetInfoManager()
    {
        _tempDeps = new List<string>();
    }
    public void Start()
    {
        this.LoadAssetInfo();
    }

    public void LoadAssetInfo()
    {
        StreamReader sr = ResourceManager.OpenText("allassetsdep.txt");

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(sr.ReadToEnd());

        XmlElement root = xml.DocumentElement;
        IEnumerator itor = root.GetEnumerator();

        while (itor.MoveNext())
        {
            XmlElement element = itor.Current as XmlElement;

            AssetInfo asset = new AssetInfo();
            asset.Import(element);

            if (!mDicNameAsset.ContainsKey(asset.Name))
            {
                this.mDicNameAsset.Add(asset.Name, asset);
            }

            if (!mDicIndexAsset.ContainsKey(asset.Index))
            {
                this.mDicIndexAsset.Add(asset.Index, asset);
            }
        }
        sr.Close();
    }

    public AssetInfo GetAssetInfo(string name)
    {
        if (this.mDicNameAsset.ContainsKey(name))
        {
            return this.mDicNameAsset[name];
        }

        return null;
    }

    /// <summary>
    /// 获取指定资源的所有依赖资源名称
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public List<string> GetAssetDeps(string resName)
    {
        _tempDeps.Clear();
        AssetInfo info = this.GetAssetInfo(resName);
        List<int> deps = info.Dependence;

        for (int i = 0; i < deps.Count; i++)
        {
            AssetInfo depItem = GetAssetInfo(deps[i]);
            if (deps != null)
            {
                _tempDeps.Add(depItem.Name);
            }
            else
            {
                Debug.LogError("无法找到资源：" + resName + " 的依赖资源，索引：" + deps[i]);
            }
        }

        return _tempDeps;
    }

    public AssetInfo GetAssetInfo(int index)
    {
        if (this.mDicIndexAsset.ContainsKey(index))
        {
            return this.mDicIndexAsset[index];
        }
        return null;
    }
}
