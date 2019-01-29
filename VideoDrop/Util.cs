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

    /// <summary>
    /// Takes a directory path and ensures it ends with '\'.
    /// </summary>
    /// <param name="dInfo"></param>
    /// <returns></returns>
    public static string DirectoryToString(System.IO.DirectoryInfo dInfo)
    {
        return DirectoryToString(dInfo.FullName);
    }

    /// <summary>
    /// Takes a directory path and ensures it ends with '\'.
    /// </summary>
    /// <param name="dInfo"></param>
    /// <returns></returns>
    public static string DirectoryToString(string dInfo)
    {
        string retStr = dInfo;
        if (!retStr.EndsWith(@"\"))
            retStr = retStr + @"\";
        return retStr;
    }

    /// <summary>
    /// Checks if the provided DirectoryInfo path exists.
    /// </summary>
    /// <param name="dInfo">Directory to check</param>
    /// <returns>True if the directory exists, false if it doesn't or the DirectoryInfo is null</returns>
    public static bool DirectoryExists(System.IO.DirectoryInfo dInfo)
    {
        if (dInfo != null)
        {
            dInfo.Refresh();
            return dInfo.Exists;
        }
        else
            return false;
    }

    /// <summary>
    /// Checks if the directory at the provided path exists.
    /// </summary>
    /// <param name="dInfo">Directory path to check.</param>
    /// <returns>True if the directory exists, false if it doesn't or the string is null</returns>
    public static bool DirectoryExists(string dInfo)
    {
        if (dInfo != null)
            return DirectoryExists(new System.IO.DirectoryInfo(dInfo));
        else
            return false;
    }

    /// <summary>
    /// Checks if the file defined by the provided FileInfo exists.
    /// </summary>
    /// <param name="fInfo"></param>
    /// <returns>True if the file exists, false if it doesn't or the FileInfo is null</returns>
    public static bool FileExists(System.IO.FileInfo fInfo)
    {
        if (fInfo != null)
        {
            fInfo.Refresh();
            return fInfo.Exists;
        }
        else
            return false;
    }

    /// <summary>
    /// Checks if the file at the provided path exists.
    /// </summary>
    /// <param name="fInfo"></param>
    /// <returns>True if the file exists, false if it doesn't or the string is null</returns>
    public static bool FileExists(string fInfo)
    {
        if (fInfo != null)
            return FileExists(new System.IO.FileInfo(fInfo));
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

    /// <summary>
    /// Takes a path string and surrounds it with quotes if it isn't already.
    /// </summary>
    /// <param name="path">Path to quote</param>
    /// <returns>A path with quotes around it</returns>
    public static string QuotePath(string path)
    {
        path = path.StartsWith("\"") ? path : "\"" + path;
        path = path.EndsWith("\"") ? path : path + "\"";
        return path;
    }

    /// <summary>
    /// Writes a message to the console if the message's DEBUGLEVEL is >= the set debug level.
    /// </summary>
    /// <param name="src">Source of the message</param>
    /// <param name="msg">Message to write</param>
    /// <param name="level">Debug level of the message</param>
    /// <returns>The string that would be outputted to the console</returns>
    public static string WriteInfo(string src, string msg, DEBUGLEVEL level)
    {
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
        if (VideoDropDebugLevel >= level)
            Console.WriteLine(toWrite);
        return toWrite;
    }

    /// <summary>
    /// Joins two parts of a path together to correct for whether or not the parts end/start with "\"
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns>A string path from the combined parts</returns>
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

    /// <summary>
    /// Gets the name of the method that called this function.
    /// </summary>
    /// <param name="memberName"></param>
    /// <returns>Name of the calling method</returns>
    public static string GetMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = null)
    {
        return memberName;
    }
}
