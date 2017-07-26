using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceTracer : MonoBehaviour
{
    public string ResName { get; set; }

    void Start()
    {
        //1、实例化后通知资源管理器增加引用计数
        ResourceManager.Instance.OnInstantiate(ResName);

        ResourceMonitorMgr.Instance.OnInstantiate(this);
    }

    void OnDestroy()
    {
        //2、删除之后通知资源管理器减少引用计数
        ResourceManager.Instance.Destroy(ResName);

        ResourceMonitorMgr.Instance.OnDestroyInstGO(this.gameObject);
    }
}
