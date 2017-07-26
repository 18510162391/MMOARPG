using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateDataBaseScript
{
    private const string GENERATE_SCRIPT_PATH = "/Script/DataBase/";
    private const string TEMPLETE_PATH = "Assets/Script/Core/Common/CSV/Templete/";

    private static string _registerList;


    [MenuItem("Tools/DataBase/Generate Script")]
    public static void GenerateScript()
    {
        Initalize();

        GenerateIDataBaseScript();
        GenerateAllDataBaseScript();
        GenerateDataBaseCtrlScript();

        AssetDatabase.Refresh();
    }

    private static void Initalize()
    {
        string path = Application.dataPath + GENERATE_SCRIPT_PATH;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }

    private static void GenerateDataBaseCtrlScript()
    {
        string text = GetTempleteText(E_TempleteType.DataBaseCtrl);
        text = text.Replace("&DataBaseManager", "Ctrl_DataBase");
        text = text.Replace("&RegisterList", _registerList);

        CreateScript("Ctrl_DataBase", text);
    }

    private static void GenerateAllDataBaseScript()
    {
        string[] file = Directory.GetFiles(Application.dataPath + "/Resources/", "*.CSV", SearchOption.AllDirectories);

        for (int i = 0; i < file.Length; i++)
        {
            string filePath = file[i];

            string fileName = Path.GetFileNameWithoutExtension(filePath).Replace(".csv", "");
            string dataPath = filePath.Replace(Application.dataPath + "/Resources/", "");
            string templeteText = GetTempleteText(E_TempleteType.DataBase);
            string CSVText = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/" + dataPath).text;

            _registerList += string.Format("RegisterDataBase(new {0}());", fileName);
            _registerList += "\n\t\t\t";

            templeteText = templeteText.Replace("&DataBaseContainerClass", string.Format("{0}Data", fileName));
            templeteText = templeteText.Replace("&DataBaseAttribute", GetDataContainerAttribute(CSVText));
            templeteText = templeteText.Replace("&DataBaseTypeClass", fileName);
            templeteText = templeteText.Replace("&DataBaseID", string.Format("{0}", i + 1));
            templeteText = templeteText.Replace("&DataBasePath", string.Format("\"{0}\"", dataPath.Replace(".csv","")));
            templeteText = templeteText.Replace("&Deserialize", GetDataBaseDeserialize(CSVText));

            CreateScript(fileName, templeteText);
        }
    }

    private static string GetDataContainerAttribute(string CSVText)
    {
        string returnValue = string.Empty;
        string[] head = CSVConvert.ConvertCSVHead(CSVText);

        for (int i = 0; i < head.Length; i++)
        {
            string[] attriArray = head[i].Split('/');

            returnValue += string.Format("public {0} {1}; ", attriArray[0], attriArray[1]);
            if (i != head.Length - 1)
            {
                returnValue += "\n\t\t";
            }
        }
        return returnValue;
    }

    private static string GetDataBaseDeserialize(string CSVText)
    {
        string returnValue = string.Empty;

        string[] head = CSVConvert.ConvertCSVHead(CSVText);

        for (int i = 0; i < head.Length; i++)
        {
            string[] attriArray = head[i].Split('/');

            if (attriArray[0] == "int")
            {
                returnValue += string.Format("_tempDataContianer.{0} = int.Parse( _data[i][{1}]);", attriArray[1], i);
            }
            else if (attriArray[0] == "float")
            {
                returnValue += string.Format("_tempDataContianer.{0} = float.Parse( _data[i][{1}]);", attriArray[1], i);
            }
            else if (attriArray[0] == "string")
            {
                returnValue += string.Format("_tempDataContianer.{0} = Convert.ToString( _data[i][{1}]);", attriArray[1], i);
            }
            else if (attriArray[0] == "int[]")
            {
                returnValue += string.Format("_tempDataContianer.{0} = CSVConvert.ConvertToArray<int>(_data[i][{1}]);", attriArray[1], i);
            }
            else if (attriArray[0] == "string[]")
            {
                returnValue += string.Format("_tempDataContianer.{0} = CSVConvert.ConvertToArray<string>(_data[i][{1}]);", attriArray[1], i);
            }
            else if (attriArray[0] == "float[]")
            {
                returnValue += string.Format("_tempDataContianer.{0} = CSVConvert.ConvertToArray<float>(_data[i][{1}]);", attriArray[1], i);
            }

            if (i != head.Length - 1)
            {
                returnValue += "\n\t\t\t\t\t";
            }
        }
        return returnValue;
    }

    private static void GenerateIDataBaseScript()
    {
        string text = GetTempleteText(E_TempleteType.IDataBase);

        CreateScript("IDataBase", text);
    }

    private static void CreateScript(string scriptName, string text)
    {
        string filePath = Path.Combine(Application.dataPath + GENERATE_SCRIPT_PATH, scriptName + ".cs");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        StreamWriter sw = File.CreateText(filePath);
        sw.WriteLine(text);
        sw.Close();
    }

    private static string GetTempleteText(E_TempleteType templeteType)
    {

        string AssetPath = TEMPLETE_PATH + "Templete_IDataBase.txt";

        switch (templeteType)
        {
            case E_TempleteType.IDataBase:
                AssetPath = TEMPLETE_PATH + "Templete_IDataBase.txt";
                break;
            case E_TempleteType.DataBase:
                AssetPath = TEMPLETE_PATH + "Templete_DataBase.txt";

                break;
            case E_TempleteType.DataBaseCtrl:
                AssetPath = TEMPLETE_PATH + "Templete_DataBaseManager.txt";

                break;
            default:
                break;
        }
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetPath);
        return textAsset.text;
    }

    public enum E_TempleteType
    {
        IDataBase,
        DataBase,
        DataBaseCtrl,
    }
}
