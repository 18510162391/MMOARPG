using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class ResourceUnit
{
    public string ResName { get; private set; }
    public UnityEngine.Object Asset { get; private set; }
    public AssetBundle Bundle { get; set; }

    private List<ResourceUnit> _DependenceList;
    public int DependenceCount { get; private set; }//引用计数

    public float _CaheTime = 0;
    private bool _bCanDispose = false;

    internal ResourceUnit(string resName, Object asset)
    {
        this.ResName = resName;
        this.Asset = asset;
        this.DependenceCount = 0;
        this._DependenceList = new List<ResourceUnit>();
        this._Reset();
    }

    private void _Reset()
    {
        this._CaheTime = GameSetting.ResReleaseCacheTime;
        this._bCanDispose = false;
    }

    public void AddDependence(ResourceUnit unit)
    {
        if (!_DependenceList.Exists(p => p.ResName == unit.ResName))
        {
            _DependenceList.Add(unit);
        }
        else
        {
            Debug.LogError("AddDependence error unit:" + unit.ResName);
        }
    }

    /// <summary>
    /// 增加引用计数
    /// </summary>
    public void AddDependenceCount()
    {
        this.DependenceCount++;
        this._Reset();
        if (this._DependenceList == null)
        {
            Debug.LogError("this.mDependenceList == null 堆栈：AddDependenceCount");
            return;
        }

        for (int i = 0; i < this._DependenceList.Count; i++)
        {
            ResourceUnit unit = this._DependenceList[i];
            if (unit == null)
            {
                Debug.LogError("unit == null 堆栈：AddDependenceCount");
                continue;
            }
            unit.AddDependenceCount();
        }
    }

    /// <summary>
    /// 减少引用计数
    /// </summary>
    public void ReduceDependenceCount()
    {
        this.DependenceCount--;

        if (this._DependenceList == null)
        {
            Debug.LogError("this.mDependenceList == null 堆栈：ReduceDependenceCount");
            return;
        }

        for (int i = 0; i < this._DependenceList.Count; i++)
        {
            ResourceUnit unit = this._DependenceList[i];
            if (unit == null)
            {
                Debug.LogError("unit == null 堆栈：ReduceDependenceCount");
                continue;
            }
            unit.ReduceDependenceCount();
        }
    }

    float _timer = 0;
    public void OnUpdate()
    {
        if (this.DependenceCount <= 0)
        {
            _timer += Time.deltaTime;
            if (_timer >= 1)
            {
                _timer = 0;

                this._CaheTime--;

                if (this._CaheTime <= 0)
                {
                    this._bCanDispose = true;
                }
            }
        }
    }

    public bool CanDispose()
    {
        return this._bCanDispose;
    }

    public int GetCacheTime()
    {
        return (int)this._CaheTime;
    }

    public void Dispose()
    {
        this.Asset = null;
        this._DependenceList.Clear();
        ResourceMonitorMgr.Instance.OnResourceUnitDispose(this);
        if (this.Bundle != null)
        {
            this.Bundle.Unload(true);
        }
        ResourceManager.Instance.RemoveResourceUnit(this.ResName);
    }
}
