using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ResourceManager : UnitySingleton<ResourceManager>
{
    private Dictionary<string, ResourceUnit> _dicRes = new Dictionary<string, ResourceUnit>();
    private Dictionary<string, ResourceRequest> _dicResRequest = new Dictionary<string, ResourceRequest>();
    private List<ResourceUnit> _releaseList = new List<ResourceUnit>();

    public void Load(string resName, Action<UnityEngine.Object> _OnLoadComplete)
    {
        //1、本地是否存在缓存资源

        ResourceUnit pUnit = null;
        if (this._dicRes.TryGetValue(resName, out pUnit))
        {
            if (_OnLoadComplete != null)
            {
                UnityEngine.Object obj = pUnit.Asset;

                _OnLoadComplete(obj);
            }
            return;
        }

        //2、该资源是否正在下载队列中
        ResourceRequest request = null;
        if (this._dicResRequest.TryGetValue(resName, out request))
        {
            if (_OnLoadComplete != null)
            {
                request.AddLoadCallBack(_OnLoadComplete);
            }
            return;
        }

        //3、构建下载资源
        ResourceRequest loadRequest = new ResourceRequest(resName);
        loadRequest.AddLoadCallBack(_OnLoadComplete);

        List<string> deps = AssetInfoManager.Instance.GetAssetDeps(resName);
        loadRequest.AddDepRes(deps);

        //先下载依赖资源
        for (int i = 0; i < loadRequest.depRes.Count; i++)
        {
            string depItem = loadRequest.depRes[i];
            this.Load(depItem, null);
        }

        //4、放入下载队列中
        if (!this._dicResRequest.ContainsKey(resName))
        {
            this._dicResRequest.Add(resName, loadRequest);
        }
    }

    private void Update()
    {
        //每帧轮询资源请求队列
        this._CheckResourceRequest();


        //1、如果有需要释放的资源，则放在释放队列中
        var enumer = this._dicRes.GetEnumerator();
        while (enumer.MoveNext())
        {
            ResourceUnit pUnit = enumer.Current.Value;
            if (pUnit != null)
            {
                pUnit.OnUpdate();
                if (pUnit.CanDispose())
                {
                    this._releaseList.Add(pUnit);
                }
            }
        }

        //2、依次从释放队列中释放资源
        if (this._releaseList.Count > 0)
        {
            for (int i = 0; i < _releaseList.Count; i++)
            {
                ResourceUnit pUnit = null;
                if (this._dicRes.TryGetValue(_releaseList[i].ResName, out pUnit))
                {
                    pUnit.Dispose();
                }
            }
            this._releaseList.Clear();
        }

    }

    private void _CheckResourceRequest()
    {
        if (this._dicResRequest == null || this._dicResRequest.Count <= 0)
        {
            return;
        }
        var enumer = this._dicResRequest.GetEnumerator();
        while (enumer.MoveNext())
        {
            ResourceRequest pRequest = enumer.Current.Value;

            if (pRequest.LoadState == GameDefine.E_ResourceLoadState.LOADING)
            {
                return;
            }

            //1、该资源是否可以下载（如果存在依赖资源，需要等到依赖资源下载完毕）

            for (int i = 0; i < pRequest.depRes.Count; i++)
            {
                string depName = pRequest.depRes[i];
                GameDefine.E_ResourceLoadState state = ResourceManager.Instance.GetResState(depName);
                if (state != GameDefine.E_ResourceLoadState.LOADED)
                {
                    pRequest.LoadState = GameDefine.E_ResourceLoadState.WAITLOAD;
                    return;
                }
            }

            //3、依赖资源已经准备完毕，可以开始下载此资源
            pRequest.LoadState = GameDefine.E_ResourceLoadState.LOADING;

            string completePath = ArchiveManager.Instance.GetPath(pRequest.ResName);
            StartCoroutine(_BeginLoad(completePath));
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private IEnumerator _BeginLoad(string url)
    {
        WWW www = new WWW(url);

        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            www.Dispose();
            Debug.LogError("资源下载有问题，error：" + www.error);
        }
        else
        {
            AssetBundle bundle = www.assetBundle;
            www.Dispose();

            this._OnLoadFinish(bundle);
        }
    }

    /// <summary>
    /// 资源加载完成后，放入缓存中。
    /// </summary>
    /// <param name="resName"></param>
    /// <param name="obj"></param>
    private void _OnLoadFinish(AssetBundle bundle)
    {
        UnityEngine.Object obj = bundle.LoadAsset(bundle.GetAllAssetNames()[0]);

        if (obj == null)
        {
            Debug.LogError("_OnLoadFinish obj==null");
            return;
        }
        string resName = obj.name;

        //1、构建资源缓存
        ResourceUnit unit = new ResourceUnit(resName, obj);
        unit.Bundle = bundle;
        List<string> deps = AssetInfoManager.Instance.GetAssetDeps(resName);
        for (int i = 0; i < deps.Count; i++)
        {
            string depResName = deps[i];

            ResourceUnit depUnit = null;
            if (this._dicRes.TryGetValue(depResName, out depUnit))
            {
                unit.AddDependence(depUnit);
            }
        }
        if (!this._dicRes.ContainsKey(resName))
        {
            Debug.Log("资源下载成功 ：" + obj.name);
            this._dicRes.Add(resName, unit);

            ResourceMonitorMgr.Instance.OnResourceUnitCreated(unit);
        }

        //2、移除资源请求
        ResourceRequest request = null;

        if (this._dicResRequest.TryGetValue(resName, out request))
        {
            request.CallBackOnLoaded(obj);
            this._dicResRequest.Remove(resName);
        }
    }

    public void RemoveResourceUnit(string resName)
    {
        if (this._dicRes.ContainsKey(resName))
        {
            this._dicRes.Remove(resName);
        }
        else
        {
            Debug.LogError("移除指定资源cache失败！ 资源：" + resName);
        }
    }


    /// <summary>
    /// 获取指定资源的状态
    /// </summary>
    /// <param name="resName"></param>
    /// <param name="loadState"></param>
    /// <returns></returns>
    public GameDefine.E_ResourceLoadState GetResState(string resName)
    {
        if (this._dicRes.ContainsKey(resName))
        {
            return GameDefine.E_ResourceLoadState.LOADED;
        }

        ResourceRequest pRequest = null;
        if (this._dicResRequest.TryGetValue(resName, out pRequest))
        {
            return pRequest.LoadState;
        }
        return GameDefine.E_ResourceLoadState.NONE;

    }


    public void OnInstantiate(string resName)
    {
        ResourceUnit pUnit = null;
        if (this._dicRes.TryGetValue(resName, out pUnit))
        {
            pUnit.AddDependenceCount();
        }
    }

    public void Destroy(string resName)
    {
        ResourceUnit pUnit = null;
        if (this._dicRes.TryGetValue(resName, out pUnit))
        {
            pUnit.ReduceDependenceCount();
        }
    }

    public static StreamReader OpenText(string fileName)
    {
        return new StreamReader(Open(fileName), System.Text.Encoding.Default);
    }

    public static Stream Open(string fileName)
    {
        //string localPath = ArchiveManager.Instance.GetPath(fileName);
        string localPath = PathUtils.GetFileRootPath() + "/" + fileName;

        if (File.Exists(localPath))
        {
            return File.Open(localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        else
        {
            //如果文件还没找到，则从原始包中找
            TextAsset text = Resources.Load(fileName) as TextAsset;
            if (text == null)
            {
                Debug.LogError("无法找到文件 " + fileName);
                return new MemoryStream(new byte[] { });
            }
            return new MemoryStream(text.bytes);
        }
    }
}

public class ResourceRequest
{
    public string ResName { get; private set; }

    public List<string> depRes;//该资源对应的依赖资源列表，需要等到所有依赖资源加载完毕，才能开始下载此资源
    public GameDefine.E_ResourceLoadState LoadState { get; set; }

    private List<Action<UnityEngine.Object>> _LoadFinishCallBack;


    public ResourceRequest(string resName)
    {
        this.ResName = resName;
        this.LoadState = GameDefine.E_ResourceLoadState.NONE;
        this._LoadFinishCallBack = new List<Action<UnityEngine.Object>>();
        this.depRes = new List<string>();
    }

    public void AddDepRes(List<string> resName)
    {
        this.depRes.AddRange(resName);
    }

    public void AddLoadCallBack(Action<UnityEngine.Object> loadCallBack)
    {
        this._LoadFinishCallBack.Add(loadCallBack);
    }


    /// <summary>
    /// 资源下载完成后的回调
    /// </summary>
    public void CallBackOnLoaded(UnityEngine.Object obj)
    {
        for (int i = 0; i < _LoadFinishCallBack.Count; i++)
        {
            Action<UnityEngine.Object> callBack = _LoadFinishCallBack[i];
            if (callBack != null)
            {
                callBack(obj);
            }
        }
    }
}
