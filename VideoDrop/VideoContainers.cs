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
    public class MKVContainer : IVideoContainer
    {
        public override List<string> SettingsUsed
        {
            get
            {
                return new List<string>() { "mkvToolsPath" };
            }
        }

        public override string FileExtension
        {
            get
            {
                return ".mkv";
            }
        }

        public override string pathKey
        {
            get
            {
                return "mkvToolsPath";
            }
        }

        public MKVContainer(VideoDropSettings settings) : base(settings)
        {
        }

        /// <summary>
        ///     ''' Sets the path to the executable. Set _exePath to "" if path is invalid
        ///     ''' </summary>
        ///     ''' <param name="exePath"></param>
        protected override void SetPath(string exePath)
        {
            if (!exePath.EndsWith(@"\"))
                exePath = exePath + @"\";
            
            _exePath = (File.Exists(exePath + "mkvmerge.exe") && File.Exists(exePath + "mkvextract.exe")) ? exePath : "";
            if (_exePath == "")
                WriteInfo(this.GetType().Name + GetMethodName(), "ERROR: Invalid MKV tools path: " + exePath, DEBUGLEVEL.ERROR);
        }

        public override bool ExtractAudio(string sourceVid, string destAudio, string tempFolder)
        {
            if (!CheckSettings())
                return false;
            if (!sourceVid.ToLower().EndsWith(".mkv".ToLower()))
            {
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Source video is not an mkv file: " + sourceVid, DEBUGLEVEL.ERROR);
                return false;
            }
            ProcessStartInfo extProcInfo = new ProcessStartInfo();

            {
                var withBlock = extProcInfo;
                withBlock.FileName = QuotePath(_exePath + "mkvextract.exe");
                withBlock.Arguments = QuotePath(sourceVid) + " tracks 1:" + QuotePath(destAudio);
                withBlock.UseShellExecute = false;
                withBlock.RedirectStandardOutput = true;
            }
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Extracting Audio:", DEBUGLEVEL.INFO);
            Console.WriteLine("   " + extProcInfo.FileName + " " + extProcInfo.Arguments);
            Process _proc = new Process();
            _proc.StartInfo = extProcInfo;
            _proc.Start();
            Console.WriteLine(_proc.StandardOutput.ReadToEnd());
            return System.IO.File.Exists(destAudio);
        }

        public override bool MuxAudio(string tempVid, string sourceAudio, string outputVid, string tempFolder)
        {
            if (!CheckSettings())
                return false;
            string outputName = outputVid;
            if (!outputName.ToLower().EndsWith(".mkv"))
            {
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Output video is not an mkv file: " + outputName, DEBUGLEVEL.ERROR);
                return false;
            }
            ProcessStartInfo muxProcInfo = new ProcessStartInfo();
            {
                var withBlock = muxProcInfo;
                withBlock.FileName = QuotePath(_exePath + "mkvmerge.exe");
                withBlock.Arguments = "-o " + QuotePath(outputName) + " -A " + QuotePath(tempVid) + " " + QuotePath(sourceAudio);
                withBlock.UseShellExecute = false;
                withBlock.RedirectStandardOutput = true;
            }
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Muxing Audio:", DEBUGLEVEL.INFO);
            Console.WriteLine("   " + muxProcInfo.FileName + " " + muxProcInfo.Arguments);
            var _proc = new Process();
            _proc.StartInfo = muxProcInfo;
            _proc.Start();
            Console.WriteLine(_proc.StandardOutput.ReadToEnd());
            return System.IO.File.Exists(outputName);
        }

        public override bool CheckSettings()
        {
            bool validSettings = true;

            if (_settings.ContainsKey("mkvToolsPath"))
            {
                System.IO.DirectoryInfo path = new System.IO.DirectoryInfo(_exePath);
                if (path.Exists)
                {
                    System.IO.FileInfo mkvMerge = new System.IO.FileInfo(DirectoryToString(path.FullName) + "mkvmerge.exe");
                    System.IO.FileInfo mkvExtract = new System.IO.FileInfo(DirectoryToString(path.FullName) + "mkvextract.exe");

                    if (!mkvMerge.Exists)
                    {
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "Cannot find mkvmerge.exe in " + path.FullName, DEBUGLEVEL.ERROR);
                        validSettings = false;
                    }
                    if (!mkvExtract.Exists)
                    {
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "Cannot find mkvextract.exe in " + path.FullName, DEBUGLEVEL.ERROR);
                        validSettings = false;
                    }
                }
                else
                    validSettings = false;
            }
            else
                validSettings = false;
            return validSettings;
        }
    }

    public class MP4Container : IVideoContainer
    {
        public override List<string> SettingsUsed
        {
            get
            {
                return new List<string>() { "mp4boxPath" };
            }
        }

        public override string FileExtension
        {
            get
            {
                return ".mp4";
            }
        }

        public override string pathKey
        {
            get
            {
                return "mp4boxPath";
            }
        }

        public MP4Container(VideoDropSettings settings) : base(settings)
        {
        }

        protected override void SetPath(string exePath)
        {
            _exePath = File.Exists(exePath) ? exePath : "";
            if (_exePath == "")
                WriteInfo(this.GetType().Name + GetMethodName(), "ERROR: Invalid mp4boxPath: " + exePath, DEBUGLEVEL.ERROR);
        }

        public override bool ExtractAudio(string sourceVid, string destAudio, string tempFolder)
        {
            if (!CheckSettings())
                return false;
            if (!sourceVid.ToLower().EndsWith(".mp4".ToLower()))
            {
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Source video is not an mp4 file: " + sourceVid, DEBUGLEVEL.ERROR);
                return false;
            }
            ProcessStartInfo extProcInfo = new ProcessStartInfo();
            // mp4box can't seem to handle a temp folder path that ends with '\'
            if (tempFolder.EndsWith(@"\"))
                tempFolder = tempFolder.Substring(0, tempFolder.LastIndexOf(@"\"));
            {
                var withBlock = extProcInfo;
                withBlock.FileName = QuotePath(_exePath);
                // mp4box -raw 2 -tmp <temp_folder> <source_video> -out <source_audio)
                withBlock.Arguments = "-tmp " + QuotePath(tempFolder) + " -raw 2 " + QuotePath(sourceVid) + " -out " + QuotePath(destAudio);
                withBlock.UseShellExecute = false;
                withBlock.RedirectStandardOutput = true;
            }
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Extracting Audio:", DEBUGLEVEL.INFO);
            Console.WriteLine("   " + extProcInfo.FileName + " " + extProcInfo.Arguments);
            Process _proc = new Process();
            _proc.StartInfo = extProcInfo;
            _proc.Start();
            Console.WriteLine(_proc.StandardOutput.ReadToEnd());

            return System.IO.File.Exists(destAudio);
        }

        public override bool MuxAudio(string tempVid, string sourceAudio, string outputVid, string tempFolder)
        {
            if (!CheckSettings())
                return false;
            // Check that the video we want to output is a .mp4
            if (!outputVid.ToLower().EndsWith(".mp4"))
            {
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Output video is not an mp4 file: " + outputVid, DEBUGLEVEL.ERROR);
                return false;
            }
            ProcessStartInfo muxProcInfo = new ProcessStartInfo();
            // mp4box can't seem to handle a temp folder path that ends with '\'
            if (tempFolder.EndsWith(@"\"))
                tempFolder = tempFolder.Substring(0, tempFolder.LastIndexOf(@"\"));
            {
                var withBlock = muxProcInfo;
                // mp4box -flat -add <video> -add <audio> <outputfile>.mp4
                withBlock.FileName = QuotePath(_exePath);
                withBlock.Arguments = "-tmp " + QuotePath(tempFolder) + " -add " + QuotePath(tempVid) + " -add " + QuotePath(sourceAudio) + " " + QuotePath(outputVid);
                withBlock.UseShellExecute = false;
                withBlock.RedirectStandardOutput = true;
            }
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "MP4Container: Muxing Audio:", DEBUGLEVEL.INFO);
            Console.WriteLine("   " + muxProcInfo.FileName + " " + muxProcInfo.Arguments);
            var _proc = new Process();
            _proc.StartInfo = muxProcInfo;
            _proc.Start();
            Console.WriteLine(_proc.StandardOutput.ReadToEnd());
            return System.IO.File.Exists(outputVid);
        }

        public override bool CheckSettings()
        {
            bool validSettings = true;

            if (_settings.ContainsKey("mp4boxPath"))
            {
                
                FileInfo mp4box = new System.IO.FileInfo(_settings["mp4boxPath"]);

                if (!mp4box.Exists)
                {
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Cannot find mp4box.exe at " + mp4box.FullName, DEBUGLEVEL.ERROR);
                    validSettings = false;
                }
            }
            else
                validSettings = false;
            return validSettings;
        }
    }
}