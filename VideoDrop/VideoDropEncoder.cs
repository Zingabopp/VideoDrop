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
    public class VideoDropEncoder : IDisposable
    {
        protected string _executablePath;
        protected bool _validExePath = false;
        protected bool _validInputPath = false;
        protected string _outputFile = "";
        protected string _inputFile = "";
        protected string _workingFolder = "";
        protected ProcessStartInfo _psi;
        protected Process _proc;
        protected Dictionary<string, string> _encodeSettings;
        protected string _arguments;
        // Private _currentJob As VideoDropJob

        public static VideoDropEncoder CreateEncoder(string encoderType, string inputFile, string outputFile, string workingFolder, Dictionary<string, string> encoderSettings)
        {
            switch (encoderType.ToLower())
            {
                case "x264":
                    {
                        return new Encoders.x264(inputFile, outputFile, workingFolder, encoderSettings);
                    }

                case "x265":
                    {
                        return new Encoders.x265(inputFile, outputFile, workingFolder, encoderSettings);
                    }

                default:
                    {
                        WriteInfo("VideoDropEncoder." + GetMethodName(), "Encoder type not recognized: " + encoderType, DEBUGLEVEL.ERROR);
                        return new VideoDropEncoder(inputFile, outputFile, workingFolder, encoderSettings);
                    }
            }
        }

        protected VideoDropEncoder(Dictionary<string, string> encoderSettings)
        {
            _encodeSettings = encoderSettings;
            var keys = _encodeSettings.Keys.ToList();
            foreach (var k in keys)
            {
                foreach (var t in GetTags(_encodeSettings[k]).Distinct())
                {
                    if (_encodeSettings.ContainsKey(t))
                        _encodeSettings[k] = _encodeSettings[k].Replace("<" + t + ">", _encodeSettings[t]);
                }
            }
        }

        public VideoDropEncoder(string inputFile, string outputFile, string workingFolder, Dictionary<string, string> encoderSettings) : this(encoderSettings)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            _workingFolder = workingFolder;
        }

        public virtual string EncoderType
        {
            get
            {
                return _encodeSettings["name"];
            }
        }
        public virtual string Arguments
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public virtual List<string> OutputTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public virtual string BuildEncoderArguments()
        {
            string args = "";

            return args;
        }
        public virtual Dictionary<string, string> EncodeSettings
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public virtual string OutputType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public virtual bool ProcessReady
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string OutputFile
        {
            get
            {
                return _outputFile;
            }
        }

        // Public Property CurrentJob() As VideoDropJob
        // Get
        // Return _currentJob
        // End Get
        // Set(ByVal value As VideoDropJob)
        // _currentJob = value
        // End Set
        // End Property

        public virtual string ExecutablePath
        {
            get
            {
                return _encodeSettings["EncoderPath"];
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

        public string EncoderPath
        {
            get
            {
                return _encodeSettings["EncoderPath"];
            }
        }

        public ProcessStartInfo EncodingProcess
        {
            get
            {
                return _psi;
            }
        }
        private void CreateProcess()
        {
            _psi = new ProcessStartInfo(QuotePath(ExecutablePath), Arguments);
            _psi.WorkingDirectory = _workingFolder;
            Console.WriteLine("VideoDropEncoder: Creating Process:");
            Console.WriteLine("             Exe: " + ExecutablePath);
            Console.WriteLine("       Arguments: " + Arguments);
        }
        private void CreateProcess(string filePath)
        {
            ExecutablePath = filePath;
            CreateProcess();
        }

        public bool StartProcess()
        {
            if ((true))
            {
                CreateProcess();
                Console.WriteLine("Starting Process...");
                Console.WriteLine(_psi.FileName + " " + _psi.Arguments);
                _psi.RedirectStandardOutput = true;
                _psi.UseShellExecute = false;
                _proc = new Process();
                _proc.StartInfo = _psi;

                _proc.Start();
                Console.WriteLine(_proc.StandardOutput.ReadToEnd());

                return true;
            }
            else
                return false;
        }

        protected List<string> GetTags(string line, string[] tagBounds = null)
        {
            if (tagBounds == null)
                tagBounds = new string[] { "<", ">" };
            List<string> tagList = new List<string>();
            string str = line;
            var startTag = tagBounds[0];
            var endTag = tagBounds[1];
            while (str.IndexOf(startTag) >= 0)
            {
                str = str.Substring(str.IndexOf(startTag));
                var endTagIndex = str.IndexOf(endTag);
                if (endTagIndex > 1)
                {
                    string tag = str.Substring(1, endTagIndex - 1);
                    if (!tag.Contains(" "))
                        tagList.Add(tag);
                    str = str.Substring(endTagIndex);
                }
                else
                    // No more end tags, get out of loop
                    str = "";
            }
            return tagList;
        }

        public override string ToString()
        {
            return ExecutablePath + " " + Arguments;
        }

        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    // TODO: dispose managed state (managed objects).
                    _proc.Dispose();

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                _psi = null;
                _encodeSettings = null;
            }
            disposedValue = true;
        }

        // TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        // Protected Overrides Sub Finalize()
        // ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
        }
    }
}