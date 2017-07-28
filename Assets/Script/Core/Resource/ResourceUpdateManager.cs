using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections;

class ResourceUpdateManager : UnitySingleton<ResourceUpdateManager>
{
    private GameDefine.OnResourceUpdateComplete _onResourceUpdateCompplete;
    private Action _onUpdateFinish;
    private bool _enableResCheck = false;

    private List<FileContainer> _RemoteFile = new List<FileContainer>();
    private List<FileContainer> _LocalFile = new List<FileContainer>();
    private List<FileContainer> _WaitUpdateFile = new List<FileContainer>();
    private List<ResourceHttpRequest> _resHttpRequest = new List<ResourceHttpRequest>();

    private long _TotalWaitUpdateSize;//需要更新资源的总大小
    public long HasUpdatedSize;//已经完成更新的资源大小

    void Update()
    {
        if (!this._enableResCheck)
        {
            return;
        }

        if (this._resHttpRequest.Count <= 0 || this._resHttpRequest.Find(p => p.HasLoadFinish == false) == null)
        {
            this._enableResCheck = false;

            if (this._onUpdateFinish != null)
            {
                this._onUpdateFinish();
            }
        }
        Debug.LogError("已经更新资源：" + this.HasUpdatedSize);
    }

    public void BeginUpdate(GameDefine.OnResourceUpdateComplete onResourceUpdateCompplete)
    {
        this._onResourceUpdateCompplete = onResourceUpdateCompplete;

        //1、是否开启资源更新
        if (!GameSetting.EnableResUpdate)
        {
            if (_onResourceUpdateCompplete != null)
            {
                _onResourceUpdateCompplete();
            }
            return;
        }

        //2、是否将资源移动到Application.persistentDataPath目录下
        if (GameSetting.EnableMoveResToPersistentDataPath)
        {
            if (!Directory.Exists(Application.persistentDataPath + "/" + GameDefine.BuildTemp))
            {
                this._MoveResToPersistentDataPath();
            }
        }

        //3、下载资源列表文件
        this._LoadLocalFile();

        this._LoadRemoteFile(() =>
        {
            //4、比对资源
            this._CheckFile();

            //5、下载资源
            this._BeginUpdate(() =>
            {
                //6、更新本地资源列表文件以及资源依赖文件

                //7、更新完成
                if (_onResourceUpdateCompplete != null)
                {
                    _onResourceUpdateCompplete();
                }
            });

        });
    }

    private void _BeginUpdate(Action onUpdateComplete)
    {
        this._enableResCheck = true;
        this._onUpdateFinish = onUpdateComplete;
        this._resHttpRequest.Clear();

        for (int i = 0; i < this._WaitUpdateFile.Count; i++)
        {
            ResourceHttpRequest pRequest = new ResourceHttpRequest(_WaitUpdateFile[i].FilePath);
            _resHttpRequest.Add(pRequest);
        }
        if (_resHttpRequest.Count > 0)
        {
            ResourceHttpRequest pRequestResources = new ResourceHttpRequest("Resources.txt");
            ResourceHttpRequest pRequestAssetDep = new ResourceHttpRequest("AllAssetsDep.txt");
            _resHttpRequest.Add(pRequestResources);
            _resHttpRequest.Add(pRequestAssetDep);

            ResourceHttpRequest.Load(_resHttpRequest);
        }
    }

    /// <summary>
    /// 对比资源，找到需要更新的资源
    /// </summary>
    private void _CheckFile()
    {
        this._WaitUpdateFile.Clear();
        this._TotalWaitUpdateSize = 0;

        for (int i = 0; i < this._RemoteFile.Count; i++)
        {
            FileContainer remoteFile = this._RemoteFile[i];

            FileContainer localFile = this._LocalFile.Find(p => p.FileName == remoteFile.FileName);
            if (localFile == null)
            {
                //如果本地资源不存在该资源，则认为该资源是需要更新的。
                this._WaitUpdateFile.Add(remoteFile);
                _TotalWaitUpdateSize += remoteFile.fileSize;

                continue;
            }

            if (!localFile.FileMD5.Equals(remoteFile.FileMD5))
            {
                //如果本地资源的md5与远程资源的md5不同，则认为该资源是需要更新的。
                this._WaitUpdateFile.Add(remoteFile);
                _TotalWaitUpdateSize += remoteFile.fileSize;

                continue;
            }
        }

        Debug.LogError("需要更新的资源：" + this._TotalWaitUpdateSize);
    }

    /// <summary>
    /// 加载本地资源文件
    /// </summary>
    private void _LoadLocalFile()
    {
        this._LocalFile.Clear();

        StreamReader sr = ResourceManager.OpenText("Resources.txt");

        string stream = sr.ReadToEnd();

        if (!string.IsNullOrEmpty(stream))
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(stream);

            XmlElement root = xml.DocumentElement;
            IEnumerator itor = root.GetEnumerator();

            while (itor.MoveNext())
            {
                XmlElement element = itor.Current as XmlElement;

                string fileName = element.GetAttribute("name");
                string filePath = element.GetAttribute("path");
                string fileMD5 = element.GetAttribute("md5");

                FileContainer container = new FileContainer();
                container.FileName = fileName;
                container.FilePath = filePath;
                container.FileMD5 = fileMD5;

                this._LocalFile.Add(container);
            }
            sr.Close();
        }
    }

    /// <summary>
    /// 加载cdn资源文件
    /// </summary>
    /// <param name="onLoadFinish"></param>
    private void _LoadRemoteFile(Action onLoadFinish)
    {
        this._RemoteFile.Clear();
        StartCoroutine(_BeginLoadRemoteFile(onLoadFinish));
    }

    IEnumerator _BeginLoadRemoteFile(Action onLoadFinish)
    {
        string url = GameDefine.REMOTE_CDN + GameDefine.BuildTemp + "/Resources.txt";
        WWW www = new WWW(url);

        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("------------------------------加载远程资源文件出错：" + www.error);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.text))
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(www.text);

                XmlElement root = xml.DocumentElement;
                IEnumerator itor = root.GetEnumerator();

                while (itor.MoveNext())
                {
                    XmlElement element = itor.Current as XmlElement;

                    string fileName = element.GetAttribute("name");
                    string filePath = element.GetAttribute("path");
                    string fileMD5 = element.GetAttribute("md5");
                    string fileSize = element.GetAttribute("size");

                    FileContainer container = new FileContainer();
                    container.FileName = fileName;
                    container.FilePath = filePath;
                    container.FileMD5 = fileMD5;
                    container.fileSize = long.Parse(fileSize);

                    this._RemoteFile.Add(container);
                }
            }
        }

        www.Dispose();

        if (onLoadFinish != null)
        {
            onLoadFinish();
        }
    }

    private void _MoveResToPersistentDataPath()
    {
        string sourcePath = Application.streamingAssetsPath;
        string targetPath = Application.persistentDataPath;

        string[] filesPath = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < filesPath.Length; i++)
        {
            string filePath = filesPath[i];
            string fileName = PathUtils.GetFileName(filePath, true);
            string suffix = PathUtils.GetFileSuffix(filePath);
            if (suffix == ".meta")
            {
                continue;
            }
            string str = filePath.Replace(sourcePath, "");
            str = str.Replace("\\", "/");

            string moveToPath = targetPath + str;
            moveToPath = PathUtils.getPath(moveToPath);
            PathUtils.CheckPath(moveToPath);
            File.Copy(filePath, moveToPath + fileName, true);
        }
    }
}

public class FileContainer
{
    public string FileName { get; set; }
    public string FilePath { get; set; }

    public string FileMD5 { get; set; }

    public long fileSize { get; set; }
}
