using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Util;

namespace VideoDrop
{

    public class VideoDropSettings
    {
        private Dictionary<string, string> _globalSettings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        // Dictionary where the key is the profile name and the value is a Dictionary of the settings associated with that profile
        private Dictionary<string, Dictionary<string, string>> _profileSettings = new Dictionary<string, Dictionary<string, string>>(System.StringComparer.OrdinalIgnoreCase);
        private string _currentProfile = "";

        public Dictionary<string, string> GlobalSettings
        {
            get
            {
                return _globalSettings;
            }
        }

        /// <summary>
        ///     ''' Used to set the profile to use for returning settings.
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public string CurrentProfile
        {
            get
            {
                return _currentProfile;
            }
            set
            {
                if (ActiveProfiles.Contains(value))
                    _currentProfile = value;
                else
                    _currentProfile = "";
            }
        }

        public VideoDropSettings(ref Dictionary<string, string> globalSettings)
        {
            foreach (string i in globalSettings.Keys)
                ModifyDictionary(i, ReplacePathArgs(globalSettings[i]), ref _globalSettings);
            var keys = _globalSettings.Keys.ToList();
            foreach (var k in keys)
            {
                foreach (var t in GetTags(_globalSettings[k]).Distinct())
                {
                    if (_globalSettings.ContainsKey(t))
                        _globalSettings[k] = _globalSettings[k].Replace("<" + t + ">", _globalSettings[t]);
                }
            }
            populateProfiles();
        }

        public VideoDropSettings(ref VideoDropSettings vdSettings)
        {
            foreach (string i in vdSettings._globalSettings.Keys)
                ModifyDictionary(i, vdSettings._globalSettings[i], ref _globalSettings);
            var keys = _globalSettings.Keys.ToList();
            foreach (var k in keys)
            {
                foreach (var t in GetTags(_globalSettings[k]).Distinct())
                {
                    if (_globalSettings.ContainsKey(t))
                        _globalSettings[k] = _globalSettings[k].Replace("<" + t + ">", _globalSettings[t]);
                }
            }
            populateProfiles();
        }

        /// <summary>
        ///     ''' Create a new _profileSettings from _globalSettings
        ///     ''' </summary>
        private void populateProfiles()
        {
            // Dictionary where key is the profile name and value is the profileID
            Dictionary<string, string> profList = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            // Get all profiles that have a name (profiles without a profile#_name=xxx are not recognized)
            foreach (var i in _globalSettings.Keys)
            {
                string curSetting = i.ToLower();
                if (curSetting.StartsWith("profile") && curSetting.EndsWith("_name"))
                    ModifyDictionary(_globalSettings[i], i.Substring(0, curSetting.IndexOf("_name")), ref profList);
            }

            // Loop through each possible profile
            foreach (var i in profList.Keys)
            {
                Dictionary<string, string> profileSettingsGroup = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
                if (!_profileSettings.ContainsKey(i))
                    // Add a new key/dictionary pair to _profileSettings if this valid profile name doesn't exist
                    _profileSettings.Add(i, new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase));
                // Loop through all settings to find the settings applicable to profile(i)
                foreach (var j in _globalSettings.Keys)
                {
                    string settingKey = profList[i] + "_"; // Setting key without the profile prefix
                                                           // Check if this setting applies to profile(i)
                    if (j.StartsWith(settingKey))
                    {
                        string profSettingKey = j.Substring(settingKey.Length); // Setting key without the profileID portion
                        if (_profileSettings[i].ContainsKey(settingKey))
                            // If key already exists, update it
                            _profileSettings[i][profSettingKey] = _globalSettings[j];
                        else
                            _profileSettings[i].Add(profSettingKey, _globalSettings[j]);
                    }
                }
            }
        }

        /// <summary>
        ///     ''' Returns a list of profiles to run on each video file or an empty list if there aren't any valid profiles selected.
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public List<string> ActiveProfiles
        {
            get
            {
                var profiles = GetSetting("profiles");
                List<string> profList = new List<string>();
                if (!(profiles == null))
                {
                    List<string> initialList = profiles.Split(',').ToList();
                    profList = (from p in initialList
                                where _profileSettings.ContainsKey(p)
                                select p).ToList();
                    foreach (var i in initialList.Except(profList))
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "Selected profile doesn't have any settings: " + i, DEBUGLEVEL.ERROR);
                }

                return profList;
            }
        }

        /// <summary>
        ///     ''' Gets a list of encoder types that have a path defined in the settings.
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public List<string> GetEncoderTypes()
        {
            string keySuffix = "_EncoderPath";
            List<string> encTypes = new List<string>();
            encTypes.AddRange(from s in _globalSettings.Keys
                              where s.ToLower().EndsWith(keySuffix.ToLower())
                              select s.Substring(0, s.IndexOf(keySuffix)));
            return encTypes;
        }

        /// <summary>
        ///     ''' Gets the global encoder settings for a given encoder name.
        ///     ''' </summary>
        ///     ''' <param name="encoderName"></param>
        ///     ''' <returns></returns>
        public Dictionary<string, string> GetEncoderSettings(string encoderName)
        {
            Dictionary<string, string> encSettings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            string keyPrefix = encoderName + "_";
            // Do global encoder settings
            List<string> keys = (from s in _globalSettings.Keys
                                 where s.ToLower().StartsWith(keyPrefix.ToLower())
                                 select s).ToList();
            encoderName = keys.First().Substring(0, encoderName.Length);
            foreach (var k in keys)
                encSettings.Add(k.Substring(keyPrefix.Length), _globalSettings[k]);
            if (!(CurrentProfile == ""))
            {
                // Get settings for CurrentProfile
                // Add profile settings to dictionary, overwriting the ones from the global settings
                var profSettings = GetProfileEncoderSettings(encoderName, CurrentProfile);
                foreach (var k in profSettings.Keys)
                    ModifyDictionary(k, profSettings[k], ref encSettings);
            }
            if (!encSettings.ContainsKey("name"))
                ModifyDictionary("name", encoderName, ref encSettings);
            return encSettings;
        }

        /// <summary>
        ///     ''' Gets the encoder settings for a given encoder name and profile name.
        ///     ''' </summary>
        ///     ''' <param name="encoderName"></param>
        ///     ''' <param name="profileName"></param>
        ///     ''' <returns></returns>
        public Dictionary<string, string> GetEncoderSettings(string encoderName, string profileName)
        {
            var encSettings = GetEncoderSettings(encoderName);

            if (!(CurrentProfile == ""))
                // CurrentProfile exists, so GetEncoderSettings already has these.
                return encSettings;

            // Add profile settings to dictionary, overwriting the ones from the global settings
            var profSettings = GetProfileEncoderSettings(encoderName, profileName);
            foreach (var k in profSettings.Keys)
                ModifyDictionary(k, profSettings[k], ref encSettings);

            return encSettings;
        }

        /// <summary>
        ///     ''' Get the encoder settings specific to the given profile.
        ///     ''' </summary>
        ///     ''' <param name="encoderName"></param>
        ///     ''' <param name="profileName"></param>
        ///     ''' <returns></returns>
        private Dictionary<string, string> GetProfileEncoderSettings(string encoderName, string profileName)
        {
            Dictionary<string, string> encSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var profSettings = GetProfileSettings(profileName);
            string keyPrefix = encoderName + "_";
            List<string> keys = (from s in profSettings.Keys
                                 where s.ToLower().StartsWith(keyPrefix.ToLower())
                                 select s).ToList();
            foreach (var k in keys)
            {
                string key = k.Substring(keyPrefix.Length);
                var value = profSettings[k];
                ModifyDictionary(key, value, ref encSettings);
            }
            return encSettings;
        }
        /// <summary>
        ///     ''' Get a specific global setting for the specified encoder.
        ///     ''' </summary>
        ///     ''' <param name="encoderName"></param>
        ///     ''' <param name="setting"></param>
        ///     ''' <returns></returns>
        public string GetEncoderSetting(string encoderName, string setting)
        {
            var settings = GetEncoderSettings(encoderName);
            string encSet = null;
            if (settings.Count == 0)
                return null;
            if (settings.ContainsKey(setting))
                encSet = GetEncoderSettings(encoderName)[setting];

            return encSet;
        }

        /// <summary>
        ///     ''' Get a specific encoder setting for the specified profile. If the setting isn't associated with the profile,
        ///     ''' attempts to get the global setting.
        ///     ''' </summary>
        ///     ''' <param name="encoderName"></param>
        ///     ''' <param name="setting"></param>
        ///     ''' <param name="profile"></param>
        ///     ''' <returns></returns>
        public string GetEncoderSetting(string encoderName, string setting, string profile)
        {
            string encSet = null;
            if (_profileSettings.ContainsKey(profile))
            {
                var settings = GetEncoderSettings(encoderName, profile);
                if (!settings.ContainsKey(setting))
                    return null;
                encSet = settings[setting];
            }
            if (encSet == null)
                encSet = GetEncoderSetting(encoderName, setting);
            return encSet;
        }

        /// <summary>
        ///     ''' Returns a list of profiles with the profile name as the key, and profileID as the value.
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public Dictionary<string, string> ProfilesByName()
        {
            Dictionary<string, string> profList = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var i in _globalSettings.Keys)
            {
                string curSetting = i.ToLower();
                if (curSetting.StartsWith("profile") && curSetting.EndsWith("name"))
                    ModifyDictionary(_globalSettings[i], i, ref profList);
            }
            return profList;
        }

        /// <summary>
        ///     ''' Returns a list of profiles with the profileID as the key, and profile name as the value.
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public Dictionary<string, string> ProfilesByID()
        {
            Dictionary<string, string> profList = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> profByNameList = ProfilesByName();
            foreach (var i in profByNameList.Keys)
                profList.Add(profByNameList[i], i);
            return profList;
        }

        /// <summary>
        ///     ''' Gets the profile key fromm the global settings dictionary.
        ///     ''' </summary>
        ///     ''' <param name="profileName"></param>
        ///     ''' <returns></returns>
        public string GetProfileKey(string profileName)
        {
            if (ProfilesByName().ContainsKey(profileName))
                return ProfilesByName()[profileName];
            else
                return "";
        }

        /// <summary>
        ///     ''' Returns the list of settings for the given profile name.
        ///     ''' </summary>
        ///     ''' <param name="profileName"></param>
        ///     ''' <returns></returns>
        public Dictionary<string, string> GetProfileSettings(string profileName)
        {
            // Dim profileSettings As Dictionary(Of String, String) = New Dictionary(Of String, String)(System.StringComparer.OrdinalIgnoreCase)
            // Dim subStr As String = "_name=" & profileName
            // Dim profileID As String = GetProfileKey(profileName)
            if (_profileSettings.ContainsKey(profileName))
                return _profileSettings[profileName];
            else
                return new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     ''' Gets the specified setting from the given profile. If a profile specific setting doesn't exist,
        ///     ''' tries to return the global setting.
        ///     ''' </summary>
        ///     ''' <param name="setting"></param>
        ///     ''' <param name="profile"></param>
        ///     ''' <returns>The setting value as a string, Nothing if setting doesn't exist</returns>
        public string GetSetting(string setting, string profile = "")
        {
            string sett = null;
            profile = CurrentProfile;
            if ((_globalSettings.ContainsKey(setting)))
                sett = _globalSettings[setting];
            if (!(profile == "") && _profileSettings.ContainsKey(profile))
            {
                if (_profileSettings[profile].ContainsKey(setting))
                    sett = _profileSettings[profile][setting];
            }
            return sett;
        }

        /// <summary>
        /// Set of 'variables' to replace if found in a setting. This is used when VideoDropSettings is created,
        ///  will not update for settings that are changed after creation.
        ///  TODO: Use this function when retrieving the setting instead of storing the replacement?
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public string ReplacePathArgs(string setVal)
        {
            // exe path
            // TODO: Currently only works if the ".\" is at the start of the setting value. Any reason to change so it works if it comes later (maybe if it's used as part of a custom parameter)?
            if (setVal.StartsWith(@".\"))
                setVal = ReplacePathArgs(setVal, @".\", DirectoryToString(GetSetting("workingFolder")));
            setVal = ReplacePathArgs(setVal, "%EXEPATH%", DirectoryToString(Directory.GetCurrentDirectory()));
            return setVal;
        }

        /// <summary>
        /// Replace path arguments in the provided string.
        /// </summary>
        /// <param name="setVal">String to check for arguments</param>
        /// <param name="identifier">Argument identifier</param>
        /// <param name="replacement">String to replace the argument with</param>
        /// <returns></returns>
        public static string ReplacePathArgs(string setVal, string identifier, string replacement)
        {
            var startIndex = setVal.ToLower().IndexOf(identifier.ToLower());
            if (startIndex >= 0)
            {
                var first = JoinPaths(setVal.Substring(0, startIndex), replacement);
                return JoinPaths(first, setVal.Substring(startIndex + identifier.Length));
            }
            else
                return setVal;
        }
        
        /// <summary>
        /// Returns a list of the tags detected in the provided string.
        /// </summary>
        /// <param name="line">String to check for tags</param>
        /// <param name="tagBounds">String array {startBound, endBound}, default is {"<",">"}</param>
        /// <returns>List<string> of tags</returns>
        public static List<string> GetTags(string line, string[] tagBounds = null)
        {
            if (tagBounds == null)
                tagBounds = new string[] { "<", ">" };
            List<string> tagList = new List<string>();
            string str = line;
            string startTag = tagBounds[0];
            string endTag = tagBounds[1];
            while (str.IndexOf(startTag) >= 0)
            {
                int startTagIndex = str.IndexOf(startTag);
                str = str.Substring(startTagIndex + 1);
                int endTagIndex = str.IndexOf(endTag);
                if (endTagIndex > 1)
                {
                    string tag = str.Substring(0, endTagIndex);
                    if (!tag.Contains(" ")) // If it contains spaces, not a valid tag
                        tagList.Add(tag);
                    else
                        endTagIndex = 0; // Wasn't a valid tag, so start the next check after the failed startTag
                    str = str.Substring(endTagIndex + 1);
                }
                else
                    // No more end tags, get out of loop
                    str = "";
            }
            return tagList;
        }

        /// <summary>
        /// Sets the debug level variable according to the setting, default is ERROR.
        /// </summary>
        public void SetDebugLevel()
        {
            string level = GetSetting("debugLevel");
            if (!(level == null))
            {
                switch (level.ToLower())
                {
                    case "disabled":
                        {
                            VideoDropDebugLevel = DEBUGLEVEL.DISABLED;
                            break;
                        }

                    case "error":
                        {
                            VideoDropDebugLevel = DEBUGLEVEL.ERROR;
                            break;
                        }

                    case "warning":
                        {
                            VideoDropDebugLevel = DEBUGLEVEL.WARNING;
                            break;
                        }

                    case "info":
                        {
                            VideoDropDebugLevel = DEBUGLEVEL.INFO;
                            break;
                        }

                    case "debug":
                        {
                            VideoDropDebugLevel = DEBUGLEVEL.DEBUG;
                            break;
                        }

                    default:
                        {
                            VideoDropDebugLevel = DEBUGLEVEL.ERROR;
                            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Invalid debug level setting: " + level + ", using level: Error.", DEBUGLEVEL.ERROR);
                            break;
                        }
                }
                Console.WriteLine("Debug level set to: " + VideoDropDebugLevel);
            }
            else
                Console.WriteLine("Debug level is null");
        }
    }

}
