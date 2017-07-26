using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wwwtest : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine(beginLoad());
    }

    IEnumerator beginLoad()
    {
        WWW www = new WWW("http://127.0.0.1/hello.txt");
        yield return www;
        Debug.LogError(www.error);
        Debug.LogError(www.text);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
