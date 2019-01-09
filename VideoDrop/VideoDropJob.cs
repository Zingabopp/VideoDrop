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
    public class VideoDropJob
    {

        // Encoder class (x264), contains executable, parameters, in/out format, process
        // Input file
        // Output file
        // AVS file
        // Audio file
        // Working folder
        // Output folder
        // Option to move original file
        private VideoDropSettings _settings;
        private FileInfo _sourceVideo;
        private FileInfo _sourceAVS;
        private FileInfo _sourceAudio;
        private DirectoryInfo _tempFolder;
        private FileInfo _tempVideo;
        private DirectoryInfo _workingFolder;
        private DirectoryInfo _outputFolder;
        private DirectoryInfo _archiveFolder;
        private string _videoExtension = ".mkv";
        private string _audioExtension = ".aac";
        private bool _useTempFolder = false;
        private bool _useArchiveFolder = false;
        private bool _tempFolderCreated = false;
        private bool _generatedAVS = false;
        private FileInfo _tempAVS;
        private FileInfo _avsTemplate;
        private FileInfo _outputVideo;
        private IVideoContainer _inputContainer;
        private IVideoContainer _outputContainer;
        private VideoDropEncoder _encoder;
        private bool _pathsGenerated = false;
        private bool _encoderReady = false;


        public string OutputType
        {
            get
            {
                return _videoExtension;
            }
            set
            {
                if (!value.StartsWith("."))
                    value = "." + value;
                _videoExtension = value;
            }
        }

        public string OutputAudioType
        {
            get
            {
                return _audioExtension;
            }
            set
            {
                if (!value.StartsWith("."))
                    value = "." + value;
                _audioExtension = value;
            }
        }
        public bool JobReady
        {
            get
            {
                bool ready = _pathsGenerated && _encoderReady;
                if (ready)
                    return true;
                else
                {
                    if (!_pathsGenerated)
                        GeneratePaths();
                    if (!_encoderReady)
                        SetupEncoder();
                    ready = _pathsGenerated && _encoderReady;
                    return ready;
                }
            }
        }

        /// <summary>
        ///     ''' Copies the values from the passed Dictionary into a new Dictionary of settings
        ///     ''' </summary>
        public VideoDropSettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = new VideoDropSettings(ref value);
            }
        }

        // Public ReadOnly Property SpecificSettings As Dictionary(Of String, Dictionary(Of String, String))
        // Get
        // Return _specificSettings
        // End Get
        // End Property

        public string SourceVideo
        {
            get
            {
                string retStr = "";
                if (FileExists(_sourceVideo))
                    retStr = _sourceVideo.FullName;
                return retStr;
            }
            set
            {
                _sourceVideo = new FileInfo(value);

                if (_sourceVideo.Exists && WorkingFolder == "")
                    WorkingFolder = _sourceVideo.DirectoryName;
            }
        }

        public string SourceAVS
        {
            get
            {
                string retStr = "";
                if (FileExists(_sourceAVS))
                    retStr = _sourceAVS.FullName;
                return retStr;
            }
            set
            {
                _sourceAVS = new FileInfo(value);

                if (_sourceAVS.Exists && WorkingFolder == "")
                    WorkingFolder = _sourceAVS.DirectoryName;
            }
        }

        public string AVSTemplate
        {
            get
            {
                string retStr;
                if (FileExists(_avsTemplate))
                    retStr = _avsTemplate.FullName;
                else
                    retStr = "";
                return retStr;
            }
            set
            {
                _avsTemplate = new FileInfo(value);
            }
        }

        public string WorkingFolder
        {
            get
            {
                if (DirectoryExists(_workingFolder))
                    return DirectoryToString(_workingFolder);
                else
                    return @".\";
            }
            set
            {
                _workingFolder = new DirectoryInfo(value);
            }
        }

        public string SourceAudio
        {
            get
            {
                return _sourceAudio.FullName;
            }
            set
            {
                _sourceAudio = new FileInfo(value);
            }
        }

        public string TempFolder
        {
            get
            {
                string retStr = @".\";
                if (_tempFolder != null)
                    retStr = DirectoryToString(_tempFolder);
                return retStr;
            }
            set
            {
                _tempFolder = new DirectoryInfo(value);
                _useTempFolder = true;
            }
        }

        public string TempVideo
        {
            get
            {
                return _tempVideo.FullName;
            }
            set
            {
                // If Not value.EndsWith(OutputType) Then
                // value = value.Substring(0, value.LastIndexOf(".")) & Encoder.OutputType
                // End If
                _tempVideo = new FileInfo(value);
            }
        }

        public string OutputFolder
        {
            get
            {
                string retStr = @".\";
                if (DirectoryExists(_outputFolder))
                    retStr = DirectoryToString(_outputFolder);
                return retStr;
            }
            set
            {
                if (!(value == null))
                {
                    if (value.StartsWith(@"\"))
                        value = JoinPaths(WorkingFolder, value);
                    _outputFolder = new DirectoryInfo(value);
                }
                else
                    _outputFolder = null;
            }
        }

        public string OutputVideo
        {
            get
            {
                return _outputVideo.FullName;
            }
            set
            {
                if (!value.EndsWith(OutputType))
                    value = value.Substring(0, value.LastIndexOf(".")) + OutputType;
                _outputVideo = new FileInfo(value);
            }
        }

        public string ArchiveFolder
        {
            get
            {
                string retStr = "";
                if (DirectoryExists(_archiveFolder))
                    retStr = DirectoryToString(_archiveFolder);
                return retStr;
            }
            set
            {
                _archiveFolder = new DirectoryInfo(value);
                _useArchiveFolder = true;
            }
        }

        public string SourceFolder
        {
            get
            {
                string retStr = "";
                if (FileExists(_sourceVideo))
                    retStr = DirectoryToString(_sourceVideo.DirectoryName);
                else
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Source file does not exist", DEBUGLEVEL.ERROR);
                return retStr;
            }
        }

        public IVideoContainer InputContainer
        {
            get
            {
                return _inputContainer;
            }
        }

        public IVideoContainer OutputContainer
        {
            get
            {
                return _outputContainer;
            }
        }

        public VideoDropEncoder Encoder
        {
            get
            {
                return _encoder;
            }
            set
            {
                _encoder = value;
            }
        }

        public string EncoderType
        {
            get
            {
                return Settings.GetSetting("encoder");
            }
        }

        public string ActiveProfile
        {
            get
            {
                return Settings.CurrentProfile;
            }
            set
            {
                Settings.CurrentProfile = value;
            }
        }
        public VideoDropJob(string sVid)
        {
            SourceVideo = sVid;
        }

        public VideoDropJob(string sVid, VideoDropSettings iniSettings)
        {
            _settings = iniSettings;
            SourceVideo = sVid;
        }

        public VideoDropJob(string sVid, VideoDropSettings iniSettings, string curProfile) : this(sVid, iniSettings)
        {
            ActiveProfile = curProfile;
        }

        public void PrintPaths()
        {
            if (_pathsGenerated)
            {
                CreateMissingDirectories();
                Console.WriteLine("       Source Folder: " + SourceFolder);
                Console.WriteLine("         SourceVideo: " + SourceVideo);
                Console.WriteLine("          Source AVS: " + SourceAVS);
                if (_generatedAVS)
                    Console.WriteLine("       Generated AVS: " + _tempAVS.FullName);
                Console.WriteLine("       WorkingFolder: " + WorkingFolder);
                Console.WriteLine("         Temp Folder: " + TempFolder);
                Console.WriteLine("         SourceAudio: " + SourceAudio);
                Console.WriteLine("          Temp Video: " + TempVideo);
                Console.WriteLine("       Output Folder: " + OutputFolder);
                Console.WriteLine("        Output Video: " + OutputVideo);
                Console.WriteLine("      Archive Folder: " + ArchiveFolder);
            }
            else
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Unable to print paths, paths not generated.", DEBUGLEVEL.ERROR);
        }

        public void GeneratePaths()
        {
            if (FileExists(_sourceVideo))
            {
                WorkingFolder = Settings.GetSetting("workingFolder");
                OutputType = Settings.GetSetting("outputType");
                _inputContainer = IVideoContainer.GetVideoContainer(_sourceVideo, Settings);
                string sourceName = _sourceVideo.Name.Substring(0, _sourceVideo.Name.LastIndexOf("."));
                TempFolder = WorkingFolder + "Temp";
                SourceAudio = TempFolder + sourceName + OutputAudioType;
                TempVideo = TempFolder + "Temp-" + _sourceVideo.Name;
                if (!(_sourceVideo.Extension == ".avs"))
                {
                    AVSTemplate = Settings.GetSetting("avsTemplate");
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Generating AVS file from " + AVSTemplate, DEBUGLEVEL.INFO);
                    _sourceAVS = new FileInfo(GenerateAVS(_sourceVideo.FullName));
                    _tempAVS = _sourceAVS;
                }
                else
                    SourceAVS = SourceVideo;// SourceVideo is the AVS file
                OutputFolder = Settings.GetSetting("outputFolder");
                if (_outputFolder == null)
                    OutputFolder = WorkingFolder;
                OutputVideo = OutputFolder + _sourceVideo.Name.Replace(_sourceVideo.Extension, OutputType);
                int count = 2;
                while (_outputVideo.Exists)
                {
                    OutputVideo = OutputFolder + _sourceVideo.Name.Replace(_sourceVideo.Extension, "(" + count + ")") + OutputType;
                    _outputVideo.Refresh();
                    count = count + 1;
                }
                _outputContainer = IVideoContainer.GetVideoContainer(_outputVideo, Settings);
                ArchiveFolder = WorkingFolder + "Archive";
                _pathsGenerated = true;
                SetupEncoder();
            }
            else
            {
                string failedSource = "";
                if (_sourceVideo != null)
                    failedSource = _sourceVideo.FullName;
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "No valid source video: " + failedSource, DEBUGLEVEL.ERROR);
                _pathsGenerated = false;
            }
        }

        public void CreateMissingDirectories()
        {
            if (_useTempFolder)
                _tempFolderCreated = CreateFolder(_tempFolder);
            CreateFolder(_outputFolder);
            if (_useArchiveFolder)
                CreateFolder(_archiveFolder);
        }

        public void CleanUp()
        {
            if (_generatedAVS)
                _tempAVS.Delete();
            _sourceAudio.Refresh();
            if (FileExists(_sourceAudio))
                _sourceAudio.Delete();
            if (File.Exists(SourceVideo + ".lwi"))
                File.Delete(SourceVideo + ".lwi");
            if (File.Exists(SourceVideo + ".ffindex"))
                File.Delete(SourceVideo + ".ffindex");
            _tempVideo.Refresh();
            if (FileExists(_tempVideo))
                _tempVideo.Delete();
            if (_tempFolderCreated)
                _tempFolder.Delete(true);
        }

        public string GenerateAVS(string sourceVid)
        {
            FileInfo sourceFile = new FileInfo(sourceVid);
            FileInfo avsTemp = new FileInfo(AVSTemplate);
            string outputAVS = DirectoryToString(TempFolder) + sourceFile.Name.Replace(sourceFile.Extension, ".avs");
            _tempAVS = new FileInfo(outputAVS);
            WriteInfo(this.GetType().Name + "." + GetMethodName(), "Generating AVS file to " + _tempAVS.FullName, DEBUGLEVEL.INFO);
            StreamReader reader = new StreamReader(avsTemp.OpenRead());
            string vidReplace = "--VideoFile--";
            StreamWriter writer = new StreamWriter(outputAVS, false, System.Text.Encoding.Default);

            string line = null;
            while ((reader.Peek() != -1))
            {
                line = reader.ReadLine().Replace(vidReplace, sourceFile.FullName);
                writer.WriteLine(line);
            }
            reader.Close();
            writer.Close();
            SourceAVS = outputAVS;
            _sourceAVS.Refresh();
            _tempAVS.Refresh();
            if (!_tempAVS.Exists)
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "AVS we just generated doesn't exist?", DEBUGLEVEL.ERROR);
            else
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Generated AVS: " + outputAVS, DEBUGLEVEL.INFO);
            _generatedAVS = true;
            return outputAVS;
        }

        /// <summary>
        ///     ''' Performs the whole job.
        ///     ''' </summary>
        ///     ''' <returns>True if job was successful</returns>
        public bool Run()
        {
            bool successful = false;
            if (JobReady)
            {
                Console.WriteLine("-------------Starting Job...-------------");
                // Extract Audio
                successful = InputContainer.ExtractAudio(SourceVideo, SourceAudio, TempFolder);
                if (!successful)
                {
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "ExtractAudio was unsuccessful.", DEBUGLEVEL.ERROR);
                    return false;
                }
                Console.WriteLine("-------------Finished audio extraction...-------------");
                // Encode video
                _encoder.StartProcess();
                successful = _tempVideo.Exists;
                if (!successful)
                {
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "Video encoding was unsuccessful.", DEBUGLEVEL.ERROR);
                    return false;
                }
                Console.WriteLine("-------------Finished video encoding...-------------");
                // Mux audio
                successful = OutputContainer.MuxAudio(TempVideo, SourceAudio, OutputVideo, TempFolder);
                if (!successful)
                {
                    WriteInfo(this.GetType().Name + "." + GetMethodName(), "MuxAudio was unsuccessful.", DEBUGLEVEL.ERROR);
                    return false;
                }
                Console.WriteLine("-------------Finished audio mux...-------------");
                // Archive source video
                Console.WriteLine("-------------Moving source video...-------------");
                // Clean up temp files
                Console.WriteLine("Cleaning up temp files...");
                CleanUp();
            }
            else
            {
                string notReadyReason = "";
                if (!_encoderReady)
                    notReadyReason = "encoder isn't ready, ";
                if (!_pathsGenerated)
                    notReadyReason = "paths not generated, ";
                notReadyReason = notReadyReason.Substring(0, notReadyReason.LastIndexOf(", ")) + ".";
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Job could not start because " + notReadyReason, DEBUGLEVEL.ERROR);
            }
            return successful;
        }

        public bool ArchiveSource()
        {
            var successful = false;
            if (!(ArchiveFolder == ""))
            {
                FileInfo sVid = new FileInfo(SourceVideo);

                DirectoryInfo destFolder = new DirectoryInfo(ArchiveFolder);

                if (destFolder.Exists)
                {
                    FileInfo newFile = new FileInfo(DirectoryToString(ArchiveFolder) + sVid.Name);
                    if (sVid.FullName == newFile.FullName)
                    {
                        WriteInfo(this.GetType().Name + "." + GetMethodName(), "Source video is already in the archive folder.", DEBUGLEVEL.INFO);
                        return true;
                    }
                    File.Move(sVid.FullName, newFile.FullName);
                    newFile.Refresh();
                    successful = newFile.Exists;
                }
            }
            if (!successful)
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Source video not found in archive folder.", DEBUGLEVEL.ERROR);
            return successful;
        }

        public bool SetupEncoder()
        {
            if (!_pathsGenerated)
            {
                _encoderReady = false;
                WriteInfo(this.GetType().Name + "." + GetMethodName(), "Unable to SetupEncoder, paths not generated.", DEBUGLEVEL.ERROR);
                return false;
            }
            Encoder = VideoDropEncoder.CreateEncoder(EncoderType, SourceAVS, TempVideo, WorkingFolder, Settings.GetEncoderSettings(EncoderType));
            TempVideo = Encoder.OutputFile;
            if (Encoder == null)
            {
                _encoderReady = false;
                return false;
            }
            _encoderReady = true;
            return true;
        }
    }
}