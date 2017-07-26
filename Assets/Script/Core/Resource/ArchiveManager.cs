using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class ArchiveManager : Singleton<ArchiveManager>
{
    private Dictionary<string, string> _DicFile = new Dictionary<string, string>();
    private Dictionary<string, string> _dicMD5 = new Dictionary<string, string>();

    public void Start()
    {
        //string url = PathUtils.GetResRootPath() + "/Resources.txt";
        //ResourceManager.Instance.BeginLoad(url, (obj) =>
        //{
        //    TextAsset text = obj as TextAsset;
        //    this.init(text.text);
        //});

        this.init();
    }

    private void init()
    {
        StreamReader sr = ResourceManager.OpenText("Resources.txt");

        XmlDocument xml = new XmlDocument();

        xml.LoadXml(sr.ReadToEnd());
        XmlElement root = xml.DocumentElement;

        IEnumerator itor = root.GetEnumerator();

        while (itor.MoveNext())
        {
            XmlElement resNote = itor.Current as XmlElement;

            string name = resNote.GetAttribute("Name");
            string path = resNote.GetAttribute("Path");
            string md5 = resNote.GetAttribute("md5");

            if (!this._DicFile.ContainsKey(name))
            {
                this._DicFile.Add(name, path);
            }

            if (!this._dicMD5.ContainsKey(name))
            {
                this._dicMD5.Add(name, path);
            }
        }

        sr.Close();
        xml = null;
    }

    public Dictionary<string, string> GetAllFile()
    {
        return this._DicFile;
    }

    /// <summary>
    /// 获取完整的资源路径（包括资源以及场景）
    /// </summary>
    public string GetPath(string resName)
    {
        resName = resName.ToLower();
        if (this._DicFile.ContainsKey(resName))
        {
            string relativePath = this._DicFile[resName];
            string completePath = PathUtils.GetResRootPath() + "/" + relativePath;
            return completePath;
        }
        else
        {
            Debug.LogError("获取资源路径失败 资源名称：" + resName);
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取指定资源的
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public string GetMD5(string resName)
    {
        if (this._dicMD5.ContainsKey(resName))
        {
            return this._dicMD5[resName];
        }
        else
        {
            Debug.LogError("获取资源md5失败 资源名称：" + resName);
        }
        return string.Empty;
    }
}
