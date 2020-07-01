using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
 
public class ConfigFile : MonoBehaviour
{
    private static string path = Path.Combine(Settings.lastConfigPath, "Settings.ini");
    private static Dictionary<string, Dictionary<string, string>> IniDictionary = new Dictionary<string, Dictionary<string, string>>();
    private static bool Initialized = false;

    public enum Sections 
    {
        General,
        Controls,
        Simulation,
    }

    public enum Keys 
    {
        resolutionX,
        resolutionY,
        videoMode,
        vSync,
        targetFPS,
        graphics,
        quality,
        language,
        volume,
        logDirectory,
    }

    private static bool FirstRead() 
    {
        if (File.Exists(path)) 
        {
            using (StreamReader sr = new StreamReader(path)) 
            {
                string line;
                string section = "";
                string key = "";
                string value = "";
                while (!string.IsNullOrEmpty(line = sr.ReadLine())) 
                {
                    line.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        section = line.Substring(1, line.Length - 2);
                    }
                    else
                    {
                        string[] ln = line.Split(new char[] { '=' });
                        key = ln[0].Trim();
                        value = ln[1].Trim();
                    }
                    if (section == "" || key == "" || value == "")
                    {
                        continue;
                    }
                    PopulateIni(section, key, value);
                }
            }
        }
        return true;
    }
 
    private static void PopulateIni(string _Section, string _Key, string _Value) 
    {
        if (IniDictionary.ContainsKey(_Section)) 
        {
            if (IniDictionary[_Section].ContainsKey(_Key))
            {
                IniDictionary[_Section][_Key] = _Value;
            }
            else
            {
                IniDictionary[_Section].Add(_Key, _Value);
            }
        } 
        else 
        {
            Dictionary<string, string> newVal = new Dictionary<string, string>();
            newVal.Add(_Key.ToString(), _Value);
            IniDictionary.Add(_Section.ToString(), newVal);
        }
    }


    public static void IniWriteValue(string _Section, string _Key, string _Value) 
    {
        if (!Initialized)
        {
            FirstRead();
        }
        PopulateIni(_Section, _Key, _Value);
        WriteIni();
    }

    public static void IniWriteValue(Sections _Section, Keys _Key, string _Value) 
    {
        IniWriteValue(_Section.ToString(), _Key.ToString(), _Value);
    }
 
    private static void WriteIni() 
    {
        using (StreamWriter sw = new StreamWriter(path)) 
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> _Section in IniDictionary) 
            {
                sw.WriteLine("[" + _Section.Key.ToString() + "]");
                foreach (KeyValuePair<string, string> _Key in _Section.Value) 
                {
                    // Value must be in one line
                    string _Value = _Key.Value.ToString();
                    _Value = _Value.Replace(Environment.NewLine, " ");
                    _Value = _Value.Replace("\r\n", " ");
                    sw.WriteLine(_Key.Key.ToString() + " = " + _Value);
                }
            }
        }
    }

    public static string IniReadValue(Sections _Section, Keys _Key) 
    {
        if (!Initialized)
        {
            FirstRead();
        }
        return IniReadValue(_Section.ToString(), _Key.ToString());
    }

    public static string IniReadValue(string _Section, string _Key) 
    {
        if (!Initialized)
        {
            FirstRead();
        }
        if (IniDictionary.ContainsKey(_Section))
        {
            if (IniDictionary[_Section].ContainsKey(_Key))
            {
                return IniDictionary[_Section][_Key];
            }
        }
        return null;
    }
}