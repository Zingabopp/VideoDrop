using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

public static class Util
{
    // #Const INFOSOURCE = "Me.GetType().Name & ""."" & GetMethodName()"
    public enum DEBUGLEVEL
    {
        DISABLED = 0,
        ERROR = 1,
        WARNING = 2,
        INFO = 3,
        DEBUG = 4
    }

    public static DEBUGLEVEL VideoDropDebugLevel = DEBUGLEVEL.DEBUG;

    /// <summary>
    /// Attempts to add a key/value pair to the dictionary. If the dictionary already contains the given key,
    /// then the existing key is modified to the new value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="dict"></param>
    /// <returns>The old value if the key already existed in the dictionary, otherwise an empty string</returns>
    public static string ModifyDictionary(string key, string value, ref Dictionary<string, string> dict)
    {
        if (dict.ContainsKey(key))
        {
            string oldKey = dict[key];
            dict[key] = value;
            return oldKey;
        }
        else
        {
            dict.Add(key, value);
            return "";
        }
    }

    public static string DirectoryToString(System.IO.DirectoryInfo dInfo)
    {
        return DirectoryToString(dInfo.FullName);
    }

    public static string DirectoryToString(string dInfo)
    {
        string retStr = dInfo;
        if (!retStr.EndsWith(@"\"))
            retStr = retStr + @"\";
        return retStr;
    }

    public static bool DirectoryExists(System.IO.DirectoryInfo dInfo)
    {
        if (dInfo != null && dInfo.Exists)
            return true;
        else
            return false;
    }

    public static bool FileExists(System.IO.FileInfo fInfo)
    {
        if (fInfo != null && fInfo.Exists)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Create a folder at the specified path.
    /// </summary>
    /// <param name="dInfo"></param>
    /// <returns></returns>
    public static bool CreateFolder(System.IO.DirectoryInfo dInfo)
    {
        // TODO: Create parent directories if they don't exist
        bool folderCreated = false;
        if (dInfo != null)
        {
            if (!dInfo.Exists)
            {
                dInfo.Create();
                dInfo.Refresh();
                folderCreated = dInfo.Exists;
                Console.WriteLine(folderCreated ? "INFO: Util: Creating folder: " : "INFO: Util: Failed to create folder: "  + dInfo.FullName);
            }
        }
        return folderCreated;
    }

    public static string QuotePath(string path)
    {
        path = path.StartsWith("\"") ? path : "\"" + path;
        path = path.EndsWith("\"") ? path : path + "\"";
        return path;
    }

    public static string WriteInfo(string src, string msg, DEBUGLEVEL level)
    {
        if (!(VideoDropDebugLevel >= level))
            return "";
        string toWrite = "";
        switch (level)
        {
            case DEBUGLEVEL.ERROR:
                {
                    toWrite = toWrite + "ERROR: ";
                    break;
                }

            case DEBUGLEVEL.WARNING:
                {
                    toWrite = toWrite + "WARNING: ";
                    break;
                }

            case DEBUGLEVEL.INFO:
                {
                    toWrite = toWrite + "INFO: ";
                    break;
                }
        }
        toWrite = toWrite + src + ": ";
        toWrite = toWrite + msg;
        Console.WriteLine(toWrite);
        return toWrite;
    }

    /// <summary>
    /// Joins two parts of a path together to correct for whether or not the parts end/start with "\"
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static string JoinPaths(string first, string second)
    {
        if (first == "" | second == "")
            return first + second;
        string newPath = first;
        var endsWithSlash = newPath.EndsWith(@"\");
        if (endsWithSlash)
        {
            // First part ends with "\"
            if (second.StartsWith(@"\"))
                newPath = newPath + second.Substring(1);
            else
                newPath = newPath + second;
        }
        else
            // First part doesn't end with "\"
            if (second.StartsWith(@"\"))
            newPath = newPath + second;
        else
            newPath = newPath + @"\" + second;
        return newPath;
    }

    public static string GetMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = null)
    {
        return memberName;
    }
}
