using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMonitor : MonoBehaviour
{
    public string ResName { get; set; }
    public List<ResourceTracer> ResourceTracer = new List<ResourceTracer>();
    public ResourceUnit _pUnit;

    internal void OnResourceUnitDispose(ResourceUnit pUnit)
    {
        ResourceTracer.Clear();
        Destroy(this.gameObject);
    }

    internal void OnResourceUnitCreated(ResourceUnit pUnit)
    {
        _pUnit = pUnit;
    }

    internal void OnInstantiate(global::ResourceTracer tracer)
    {
        this.gameObject.name = this.ResName;
        this.ResourceTracer.Add(tracer);
    }

    internal void OnDestroyInstGO(global::ResourceTracer tracer)
    {
        this.ResourceTracer.Remove(tracer);
    }

    float _timer = 0;
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= 1)
        {
            _timer = 0;

            if (_pUnit == null)
            {
                return;
            }

            if (_pUnit.DependenceCount > 0)
            {
                return;
            }

            this.gameObject.name = this.ResName + "_" + _pUnit.GetCacheTime();
        }
    }
}
