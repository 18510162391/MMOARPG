using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMonitorMgr : UnitySingleton<ResourceMonitorMgr>
{

    Dictionary<string, ResourceMonitor> _dicResourceMonitor = new Dictionary<string, ResourceMonitor>();
    private GameObject _transParent;
    void Awake()
    {
        _transParent = GameObject.Find("ResourceMonitor");
        if (_transParent == null)
        {
            _transParent = new GameObject();
            _transParent.name = "ResourceMonitor";
            //_transParent.hideFlags = HideFlags.DontSave;
            GameObject.DontDestroyOnLoad(_transParent);
        }
    }

    public void OnResourceUnitCreated(ResourceUnit pUnit)
    {
        if (!GameSetting.EnableResMonitor)
        {
            return;
        }

        GameObject go = new GameObject();
        go.name = pUnit.ResName;
        go.transform.parent = _transParent.transform;

        ResourceMonitor monitor = MonoUtils.GetOrAddComponent<ResourceMonitor>(go);
        monitor.ResName = pUnit.ResName;

        if (!this._dicResourceMonitor.ContainsKey(pUnit.ResName))
        {
            this._dicResourceMonitor.Add(pUnit.ResName, monitor);
            monitor.OnResourceUnitCreated(pUnit);
        }
    }

    public void OnResourceUnitDispose(ResourceUnit pUnit)
    {
        if (!GameSetting.EnableResMonitor)
        {
            return;
        }

        ResourceMonitor monitor = null;
        if (this._dicResourceMonitor.TryGetValue(pUnit.ResName, out monitor))
        {
            monitor.OnResourceUnitDispose(pUnit);
            this._dicResourceMonitor.Remove(pUnit.ResName);
        }
    }

    public void OnInstantiate(ResourceTracer tracer)
    {
        if (!GameSetting.EnableResMonitor)
        {
            return;
        }

        ResourceMonitor monit = null;
        if (this._dicResourceMonitor.TryGetValue(tracer.ResName, out monit))
        {
            monit.OnInstantiate(tracer);
        }


        List<string> deps = AssetInfoManager.Instance.GetAssetDeps(tracer.ResName);

        for (int i = 0; i < deps.Count; i++)
        {
            string depItem = deps[i];

            ResourceMonitor monitor = null;
            if (this._dicResourceMonitor.TryGetValue(depItem, out monitor))
            {
                monitor.OnInstantiate(tracer);
            }
        }

    }

    public void OnDestroyInstGO(GameObject goTracer)
    {
        if (!GameSetting.EnableResMonitor)
        {
            return;
        }

        ResourceTracer tracer = goTracer.GetComponent<ResourceTracer>();
        if (tracer == null)
        {
            return;
        }

        ResourceMonitor monit = null;
        if (this._dicResourceMonitor.TryGetValue(tracer.ResName, out monit))
        {
            monit.OnDestroyInstGO(tracer);
        }

        List<string> deps = AssetInfoManager.Instance.GetAssetDeps(tracer.ResName);

        for (int i = 0; i < deps.Count; i++)
        {
            string depItem = deps[i];

            ResourceMonitor monitor = null;
            if (this._dicResourceMonitor.TryGetValue(depItem, out monitor))
            {
                monitor.OnDestroyInstGO(tracer);
            }
        }
    }

    void OnDestroy()
    {
        //MonoUtils.ClearChild(this.transform);
    }
}
