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

namespace VideoDrop
{
    public abstract class IVideoContainer
    {
        protected string _exePath = "";
        protected Dictionary<string, string> _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // Private _videoFile As IO.FileInfo
        public abstract string FileExtension { get; }
        public abstract string pathKey { get; }
        public abstract List<string> SettingsUsed { get; }

        // Public Property VideoFile As IO.FileInfo
        // Get
        // Return _videoFile
        // End Get
        // Set(value As IO.FileInfo)
        // _videoFile = value
        // End Set
        // End Property

        public abstract  bool ExtractAudio(string sourceVid, string destAudio, string tempFolder);
        public abstract  bool MuxAudio(string tempVid, string sourceAudio, string outputVid, string tempFolder);
        protected abstract void SetPath(string exePath);
        public abstract bool CheckSettings();

        public  static IVideoContainer GetVideoContainer(string video, VideoDropSettings settings)
        {
            return GetVideoContainer(new System.IO.FileInfo(video), settings);
        }

        public IVideoContainer(VideoDropSettings settings)
        {
            foreach (var i in SettingsUsed)
            {
                string value = settings.GetSetting(i);
                if (!(value == null))
                    ModifyDictionary(i, value, ref _settings);
            }
            SetPath(_settings[pathKey]);
        }

        public static IVideoContainer GetVideoContainer(System.IO.FileInfo video, VideoDropSettings settings)
        {
            string fileType = video.Extension;
            IVideoContainer vidContainer;
            // TODO: Pass only the relevant settings using Property SettingsUsed (Or just copy the relevant settings during the constructor).
            switch (fileType.ToLower())
            {
                case ".mkv":
                    {
                        vidContainer = new MKVContainer(settings);
                        break;
                    }

                case ".mp4":
                    {
                        vidContainer = new MP4Container(settings);
                        break;
                    }

                default:
                    {
                        WriteInfo("IVideoContainer" + "." + GetMethodName(), "Invalid video container type: " + video.FullName, DEBUGLEVEL.ERROR);
                        return null;
                    }
            }
            // vidContainer.VideoFile = video
            return vidContainer;
        }
    }
}