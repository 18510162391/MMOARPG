using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Start()
    {
        Debug.Log(Application.persistentDataPath);

        ResourceUpdateManager.Instance.BeginUpdate(() =>
        {
            ArchiveManager.Instance.Start();
            AssetInfoManager.Instance.Start();

            ResourceManager.Instance.Load("Cube", (obj) =>
            {
                GameObject go = Instantiate(obj) as GameObject;
                go.transform.position = Vector3.zero;
                ResourceTracer tracer = MonoUtils.GetOrAddComponent<ResourceTracer>(go);
                tracer.ResName = obj.name;

                Debug.Log("Finish");
            });
        });
    }
}
