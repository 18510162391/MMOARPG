using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;


public class ResourceHttpRequest
{
    private static System.Object _LockRequestList = new object();
    private static string _SavePathRoot = Application.persistentDataPath;
    string _url = null;
    string _assetName;//相对路径
    public bool HasLoadFinish = false;
    public ResourceHttpRequest(string url)
    {
        HasLoadFinish = false;
        this._assetName = url;
        this._url = GameDefine.REMOTE_CDN + GameDefine.BuildTemp + "/" + url;
    }

    public void AsyDownLoad()
    {
        HttpWebRequest httpRequest = WebRequest.Create(this._url) as HttpWebRequest;
        httpRequest.BeginGetResponse(ResponseCallback, httpRequest);
    }

    void ResponseCallback(IAsyncResult ar)
    {
        HttpWebRequest req = ar.AsyncState as HttpWebRequest;
        if (req == null)
        {
            return;
        }
        HttpWebResponse response = req.EndGetResponse(ar) as HttpWebResponse;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.Close();
            Debug.LogError("--------------------------response.StatusCode != HttpStatusCode.OK " + response.StatusCode);
            return;
        }
        WebReqState st = new WebReqState(_SavePathRoot + "/" + GameDefine.BuildTemp + "/" + _assetName);
        st.WebResponse = response;
        Stream responseStream = response.GetResponseStream();
        st.OrginalStream = responseStream;
        responseStream.BeginRead(st.Buffer, 0, WebReqState.BufferSize, new AsyncCallback(ReadDataCallback), st);
    }

    void ReadDataCallback(IAsyncResult ar)
    {
        WebReqState rs = ar.AsyncState as WebReqState;
        int read = rs.OrginalStream.EndRead(ar);
        if (read > 0)
        {
            rs.fs.Write(rs.Buffer, 0, read);
            rs.fs.Flush();
            rs.OrginalStream.BeginRead(rs.Buffer, 0, WebReqState.BufferSize, new AsyncCallback(ReadDataCallback), rs);
        }
        else
        {
            rs.fs.Close();
            rs.OrginalStream.Close();
            rs.WebResponse.Close();
            HasLoadFinish = true;
        }
    }

    public static void Load(List<ResourceHttpRequest> httpRequest)
    {
        Thread thread = new Thread(DownAsset);
        thread.Start(httpRequest);
    }
    private static void DownAsset(System.Object httpRequest)
    {
        lock (_LockRequestList)
        {
            List<ResourceHttpRequest> httpRequestList = httpRequest as List<ResourceHttpRequest>;

            for (int i = 0; i < httpRequestList.Count; i++)
            {
                ResourceHttpRequest request = httpRequestList[i];
                request.AsyDownLoad();
            }
        }
    }
}

internal class WebReqState
{
    public byte[] Buffer;

    public FileStream fs;

    public const int BufferSize = 1024;

    public Stream OrginalStream;

    public HttpWebResponse WebResponse;

    public WebReqState(string path)
    {
        Debug.LogError("------------------------WebReqState " + path);
        Buffer = new byte[1024];

        string pathRoot = PathUtils.getPath(path);
        PathUtils.CheckPath(pathRoot);
        fs = new FileStream(path, FileMode.Create);
    }
}

