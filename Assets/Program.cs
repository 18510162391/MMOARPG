using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;


public class HttpHelper
{

    string path = null;
    string assetName;
    public HttpHelper(string path)
    {
        this.path = path;
    }

    public void AsyDownLoad(string url)
    {
        assetName = url.Split('/')[3];
        HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
        httpRequest.BeginGetResponse(ResponseCallback, httpRequest);
    }

    void ResponseCallback(IAsyncResult ar)
    {
        HttpWebRequest req = ar.AsyncState as HttpWebRequest;
        if (req == null) return;
        HttpWebResponse response = req.EndGetResponse(ar) as HttpWebResponse;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.Close();
            return;
        }
        WebReqState1 st = new WebReqState1(path + "/" + assetName);
        st.WebResponse = response;
        Stream responseStream = response.GetResponseStream();
        st.OrginalStream = responseStream;
        responseStream.BeginRead(st.Buffer, 0, WebReqState1.BufferSize, new AsyncCallback(ReadDataCallback), st);
    }

    void ReadDataCallback(IAsyncResult ar)
    {
        WebReqState1 rs = ar.AsyncState as WebReqState1;
        int read = rs.OrginalStream.EndRead(ar);
        if (read > 0)
        {
            rs.fs.Write(rs.Buffer, 0, read);
            rs.fs.Flush();
            rs.OrginalStream.BeginRead(rs.Buffer, 0, WebReqState1.BufferSize, new AsyncCallback(ReadDataCallback), rs);
        }
        else
        {
            Debug.LogError("完成");
            rs.fs.Close();
            rs.OrginalStream.Close();
            rs.WebResponse.Close();
        }
    }
}

internal class WebReqState1
{
    public byte[] Buffer;

    public FileStream fs;

    public const int BufferSize = 1024;

    public Stream OrginalStream;

    public HttpWebResponse WebResponse;

    public WebReqState1(string path)
    {
        Buffer = new byte[1024];
        fs = new FileStream(path, FileMode.Create);
    }

}


public class Program : MonoBehaviour
{
    void Start()
    {
        Program p = new Program();
        p.BeginLoad();
    }

    //static void Main(string[] args)
    //{


    //    Console.ReadKey();
    //}

    void BeginLoad()
    {
        Thread thread = new Thread(DownAsset); //ParameterizedThreadStart 多线程传参 
        thread.Start(@"C:\Users\Administrator\AppData\LocalLow\DefaultCompany\MMOARPG\hhh" + "|" + "m1"); //只能带一个object参数 所以使用字符串拼接
    }

    void DownAsset(System.Object file)
    {
        string[] fileName = file.ToString().Split('|');
        HttpHelper help = new HttpHelper(fileName[0]);
        help.AsyDownLoad("http://127.0.0.1/" + fileName[1] + ".bytes");//注意在手机上测试 该对Ip地址
    }
}
