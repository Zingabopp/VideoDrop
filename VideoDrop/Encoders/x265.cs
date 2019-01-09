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
using static Util;

namespace VideoDrop.Encoders
{

    public class x265 : VideoDropEncoder
    {
        private x265_Options _currentSettings;

        public override string Arguments
        {
            get
            {
                return _arguments;
            }
            set
            {
                _arguments = value;
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Parameters for x265: " + value, DEBUGLEVEL.INFO);
            }
        }

        public override string EncoderType
        {
            get
            {
                return "x265";
            }
        }

        public static List<string> ValidTypes
        {
            get
            {
                return new List<string>() { ".h265", ".mkv", ".mp4" };
            }
        }

        public override Dictionary<string, string> EncodeSettings
        {
            get
            {
                return _currentSettings.Settings;
            }
            set
            {
                _currentSettings.Settings = value;
            }
        }

        public override string OutputType
        {
            get
            {
                return _currentSettings.OutputType;
            }
            set
            {
                // Check if value is a valid output type, if it's not, use .h265 as default
                _currentSettings.OutputType = OutputTypes.Contains(value) ? value : ".h265";
            }
        }

        public override List<string> OutputTypes
        {
            get
            {
                return ValidTypes;
            }
        }

        public override string ExecutablePath
        {
            get
            {
                return _encodeSettings["avs4x26xPath"];
            }
            set
            {
                if (File.Exists(value))
                {
                    _executablePath = value;
                    _validExePath = true;
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Executable path seems valid.", DEBUGLEVEL.INFO);
                }
                else
                {
                    _validExePath = false;
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid executable path. -> " + value, DEBUGLEVEL.ERROR);
                }
            }
        }

        public override bool ProcessReady
        {
            get
            {
                bool isReady = true;
                if (!_validExePath)
                {
                    isReady = false;
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Encoder exe path not valid.", DEBUGLEVEL.ERROR);
                }
                if (!_validInputPath)
                {
                    isReady = false;
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Input file path not valid.", DEBUGLEVEL.ERROR);
                }
                if (Arguments == "")
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Empty arguments, defaults will be used.", DEBUGLEVEL.WARNING);
                if (isReady)
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Process ready", DEBUGLEVEL.INFO);
                return isReady;
            }
        }

        public x265(Dictionary<string, string> encoderSettings) : base(encoderSettings)
        {
            _currentSettings.Settings = encoderSettings;
        }

        public x265(string inputFile, string outputFile, string workingFolder, Dictionary<string, string> encoderSettings) : base(inputFile, outputFile, workingFolder, encoderSettings)
        {
            _currentSettings = new x265_Options();
            _currentSettings.Settings = encoderSettings;
            _outputFile = outputFile.Substring(0, outputFile.LastIndexOf(".")) + _currentSettings.OutputType;
            BuildEncoderArguments();
        }

        public override string BuildEncoderArguments()
        {
            string args = "";
            // CurrentJob = job
            var InputFile = QuotePath(_inputFile);
            // TODO: probably a dumb way to do this check.
            _validInputPath = File.Exists(InputFile);
            var OutputFile = QuotePath(_outputFile);
            // Add options to args
            args = _currentSettings.ToString();

            // Add in/out files to args
            args = "--x26x-binary " + QuotePath(EncoderPath) + " " + _currentSettings.ToString() + " " + InputFile + " -o " + OutputFile;
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Encoder arguments: " + args, DEBUGLEVEL.INFO);
            Arguments = args;
            return args;
        }
    }

    public class x265_Options
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> SettingPrefixes
        {
            get
            {
                return new Dictionary<string, string>() { { "preset", "--preset" }, { "crf", "--crf" }, { "custom", "" } };
            }
        }

        public Dictionary<string, string> Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                // Update settings dictionary
                foreach (var key in value.Keys)
                    ModifyDictionary(key, value[key], ref _settings);
            }
        }

        public string EncoderPath
        {
            get
            {
                string encPath = "";
                if (_settings.ContainsKey("EncoderPath"))
                {
                    if (System.IO.File.Exists(_settings["EncoderPath"]))
                    {
                        if (_settings["EncoderPath"].EndsWith("x265.exe"))
                            encPath = _settings["EncoderPath"];
                        else
                            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid filename for encoder: " + _settings["EncoderPath"], DEBUGLEVEL.ERROR);
                    }
                    else
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "No file at " + _settings["EncoderPath"], DEBUGLEVEL.ERROR);
                }
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "No encoder path found in _settings", DEBUGLEVEL.ERROR);
                return encPath;
            }
            set
            {
                if (System.IO.File.Exists(value))
                {
                    if (value.ToLower().EndsWith("x265.exe"))
                        ModifyDictionary("EncoderPath", value, ref _settings);
                    else
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid filename for encoder: " + value, DEBUGLEVEL.ERROR);
                }
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "No file at " + value, DEBUGLEVEL.ERROR);
            }
        }

        public string Avs4x26xPath
        {
            get
            {
                var settingKey = "avs4x26xPath";
                var fileName = "avs4x26x.exe";
                string encPath = "";
                if (_settings.ContainsKey(settingKey))
                {
                    if (System.IO.File.Exists(_settings[settingKey]))
                    {
                        if (_settings[settingKey].EndsWith(fileName))
                            encPath = _settings[settingKey];
                        else
                            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid filename for encoder: " + _settings[settingKey], DEBUGLEVEL.ERROR);
                    }
                    else
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "No file at " + _settings[settingKey], DEBUGLEVEL.ERROR);
                }
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "No encoder path found in _settings", DEBUGLEVEL.ERROR);
                return encPath;
            }
            set
            {
                var settingKey = "avs4x26xPath";
                var fileName = "avs4x26x.exe";
                if (System.IO.File.Exists(value))
                {
                    if (value.ToLower().EndsWith(fileName))
                        ModifyDictionary(settingKey, value, ref _settings);
                    else
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid filename for encoder: " + value, DEBUGLEVEL.ERROR);
                }
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "No file at " + value, DEBUGLEVEL.ERROR);
            }
        }

        public string OutputType
        {
            get
            {
                if (_settings.ContainsKey("outputType"))
                {
                    if (!_settings["outputType"].StartsWith("."))
                        _settings["outputType"] = "." + _settings["outputType"];
                    return _settings["outputType"];
                }
                else
                    return ".h265";
            }
            set
            {
                if (!value.StartsWith("."))
                    value = "." + value;
                if (x265.ValidTypes.Contains(value))
                    ModifyDictionary("outputType", value, ref _settings);
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid output type for x265: " + value, DEBUGLEVEL.ERROR);
            }
        }
        public List<string> ValidPresets
        {
            get
            {
                return new List<string>() { "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow", "placebo" };
            }
        }
        public string cpuPreset
        {
            get
            {
                if (_settings.ContainsKey("preset"))
                {
                    cpuPreset = _settings["preset"];
                    return _settings["preset"];
                }
                else
                {
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "No preset found for x265, using medium", DEBUGLEVEL.ERROR);
                    return "medium";
                }
            }
            set
            {
                if (x265.ValidTypes.Contains(value))
                    ModifyDictionary("preset", value, ref _settings);
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid preset for x265: " + value, DEBUGLEVEL.ERROR);
            }
        }
        public int crf
        {
            get
            {
                if (_settings.ContainsKey("crf"))
                    return int.Parse(_settings["crf"]);
                else
                    return -1;
            }
            set
            {
                if ((0 <= value) && (value <= 51))
                    ModifyDictionary("crf", value.ToString(), ref _settings);
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid crf value (0-51): " + value, DEBUGLEVEL.ERROR);
            }
        }

        public string Custom
        {
            get
            {
                if (_settings.ContainsKey("custom"))
                    return _settings["custom"];
                else
                    return "";
            }
            set
            {
                ModifyDictionary("custom", value, ref _settings);
            }
        }

        public override string ToString()
        {
            string retStr = "";
            foreach (var setKey in _settings.Keys)
            {
                if (SettingPrefixes.ContainsKey(setKey))
                    retStr = retStr + SettingPrefixes[setKey] + " " + _settings[setKey] + " ";
            }
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "x265_Options ToString(): " + retStr.Trim(), DEBUGLEVEL.INFO);
            return retStr.Trim();
        }
    }

}
