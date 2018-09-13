using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System;

public class LMFileWriter : MonoBehaviour
{

    public string productName;

    public string RootPath
    {
        get; private set;
    }

    public void Init(string _rootPath)
    {
        RootPath = _rootPath;
    }

    public string Write(string _filePath, string _content)
    {
        EnsureFolder(_filePath);

        try
        {
            using (FileStream fs = new FileStream(RootPath + "/" + _filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.SetLength(0);
                byte[] bytes = Encoding.UTF8.GetBytes(_content);
                fs.Write(bytes, 0, bytes.Length);
            }
        }
        catch (Exception _ex)
        {
            return _ex.ToString();
        }

        return string.Empty;
    }

    public string Write(string _filePath, byte[] _contents)
    {
        try
        {
            using (FileStream fs = new FileStream(RootPath + "/" + _filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(_contents, 0, _contents.Length);
            }
        }
        catch (Exception _ex)
        {
            return _ex.ToString();
        }

        return string.Empty;
    }

    public string Read(string _fileName, System.Action<List<string>> _callback)
    {
        List<string> retval = new List<string>();

        string path = RootPath + "/" + _fileName;
        try
        {
            using (StreamReader sr = new StreamReader(path, Encoding.Default))
            {
                string line = sr.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    retval.Add(line);
                    line = sr.ReadLine();
                }
            }

            _callback(retval);
        }
        catch (Exception _ex)
        {
            return _ex.ToString();
        }

        return string.Empty;
    }

    public string Read(string _fileName)
    {
        string path = Path.Combine(RootPath, _fileName);

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return string.Empty;
    }

    public T ReadJSON<T>(string _fileName)
    {
        string data = Read(_fileName);

        if (data != string.Empty)
        {
            return JsonUtility.FromJson<T>(data);
        }

        Debug.LogWarning("Find no config file, perhaps you passed the wrong path");
        return default(T);
    }

    private void EnsureFolder(string _path)
    {
        int lastSlashIndex = _path.LastIndexOf('/');

        if (lastSlashIndex < 0)
            return;

        string tmpPath = RootPath + "/" + _path.Substring(0, lastSlashIndex);

        if (!Directory.Exists(tmpPath))
        {
            Debug.Log("Folder Created: " + tmpPath);
            Directory.CreateDirectory(tmpPath);
        }
    }
}
