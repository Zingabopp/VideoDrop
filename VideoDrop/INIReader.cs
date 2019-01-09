using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using static System.Reflection.MethodBase;
using static Util;

public static class INIReader
{
    // INI file containing the defaults, should not be changed by the user.
    private static FileInfo _iniFile = new FileInfo(DirectoryToString(Directory.GetCurrentDirectory()) + "VideoDrop.ini");
    // Used for customizing the settings, these override settings in VideoDrop.ini
    private static FileInfo _userIniFile = new FileInfo(DirectoryToString(Directory.GetCurrentDirectory()) + "VideoDrop_local.ini");
    private static Dictionary<string, string> _iniDefaults = new Dictionary<string, string>()
    {
        {
            "EncoderPath",
            @"H:\MeGUI-64\tools\x264\x264.exe"
        },
        {
            "mkvToolsPath",
            @"H:\MeGUI-64\tools\mkvmerge"
        },
        {
            "avsTemplate",
            @".\Reencode.avs.template"
        },
        {
            "tempFolder",
            "Temp"
        },
        {
            "encoder",
            "x264"
        },
        {
            "outputType",
            "mkv"
        },
        {
            "x264_crf",
            "24"
        },
        {
            "x264_preset",
            "medium"
        },
        {
            "x265_crf",
            "20"
        },
        {
            "x265_preset",
            "medium"
        }
    };
    public static string iniPath
    {
        get
        {
            return _iniFile.FullName;
        }
        set
        {
            if (File.Exists(value) && value.EndsWith(".ini"))
            {
                WriteInfo("INIReader." + GetMethodName(), "Setting ini path to: " + value, DEBUGLEVEL.DEBUG);
                _iniFile = new FileInfo(value);
            }
            else
            {
                WriteInfo("INIReader." + GetMethodName(), "ini path invalid: " + value, DEBUGLEVEL.ERROR);
                GenerateINI();
            }
        }
    }

    public static string UserIniPath
    {
        get
        {
            return _userIniFile.FullName;
        }
        set
        {

            if (File.Exists(value) && value.EndsWith(".ini"))
            {
                WriteInfo("INIReader." + GetMethodName(), "Setting user ini path to: " + value, DEBUGLEVEL.DEBUG);
                _iniFile = new FileInfo(value);
            }
            else
                WriteInfo("INIReader." + GetMethodName(), " user ini path invalid: " + value, DEBUGLEVEL.WARNING);
        }
    }

    private static void GenerateINI()
    {
        WriteInfo("INIReader." + GetMethodName(), "Generating ini file at ", DEBUGLEVEL.WARNING);
        string fileName = DirectoryToString(Directory.GetCurrentDirectory()) + "VideoDrop.ini";
        if (!FileExists(new FileInfo(fileName)))
        {
            StreamWriter writer  = new StreamWriter(fileName, false);
            foreach (var key in _iniDefaults.Keys)
                writer.WriteLine(key + "=" + _iniDefaults[key]);
            writer.Close();
        }
        iniPath = fileName;
    }

    public static Dictionary<string, string> ReadINI()
    {
        Dictionary<string, string> settings = ReadINIFile(iniPath);

        if (File.Exists(UserIniPath))
        {
            var userSettings = ReadINIFile(UserIniPath);
            foreach (var key in userSettings.Keys)
                ModifyDictionary(key, userSettings[key], ref settings);
        }
        return settings;
    }

    public static Dictionary<string, string> ReadINIFile(string filePath)
    {
        TextFieldParser fileContents = new TextFieldParser(filePath);
        

        fileContents.TextFieldType = FieldType.Delimited;
        
        fileContents.SetDelimiters(new string[] { "=" });
        Dictionary<string, string> settings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        string[] currentRow;
        while (!fileContents.EndOfData)
        {
            try
            {
                currentRow = fileContents.ReadFields();
                if (ValidRow(currentRow))
                {
                    var oldKey = ModifyDictionary(currentRow[0], currentRow[1], ref settings);
                    if (oldKey == "")
                        WriteInfo("INIReader." + GetMethodName(), "Key: " + currentRow[0] + " added to settings.", DEBUGLEVEL.DEBUG);
                    else
                        WriteInfo("INIReader." + GetMethodName(), "Key: " + currentRow[0] + " changed from " + oldKey + " to " + currentRow[1], DEBUGLEVEL.DEBUG);
                }
            }
            catch (MalformedLineException ex)
            {
                WriteInfo("INIReader." + GetMethodName(), "Line " + ex.Message + "is not valid and will be skipped.", DEBUGLEVEL.WARNING);
            }
        }
        fileContents.Close();

        return settings;
    }

    /// <summary>
    ///     ''' Checks if the line read from the ini file is a valid setting.
    ///     ''' </summary>7
    ///     ''' <param name="currentRow"></param>
    ///     ''' <returns>True if valid</returns>
    public static bool ValidRow(string[] currentRow)
    {
        
        var isValid = true;
        if (currentRow[0].StartsWith("["))
            isValid = false;
        if (currentRow[0].StartsWith("#"))
            isValid = false;
        string validMsg = isValid ? "Row is valid: " : "Row is invalid: ";
        WriteInfo("INIReader." + GetMethodName(), validMsg + currentRow[0], DEBUGLEVEL.DEBUG);
        return isValid;
    }

    public static bool WriteINI(Dictionary<string, string> settings)
    {
        Dictionary<string, string> iniSettings = ReadINI();
        foreach (var setting in settings)
        {
            if (iniSettings.ContainsKey(setting.Key))
                iniSettings[setting.Key] = setting.Value;
            else
                iniSettings.Add(setting.Key, setting.Value);
        }
        StreamWriter writeStream = new StreamWriter(iniPath, false);
        
        foreach (var setting in settings)
            writeStream.WriteLine(setting.Key + "=" + setting.Value);
        writeStream.Close();
        return true;
    }
}
