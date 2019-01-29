using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using static Util;
using static INIReader;

namespace VideoDrop
{
    class ConsoleUI
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Starting video drop...");
            // TODO: Make fileList -> argList and then make fileList a List(Of FileInfo), that way other parameters can be passed
            // from the command line
            List<string> fileList = new List<string>(args);
            bool noArchive = false;
            WriteInfo(GetMethodName(), "Video files to convert:", DEBUGLEVEL.INFO);
            if (fileList.Count == 0)
            {
                fileList = new List<string>() { "test.mp4" };//, "hevc_test.mp4" };
                //noArchive = true;
            }
            string startTag = "StartTag";
            string middleTag = "MiddleTag";
            string endTag = "EndTag";
            var line = "<" + startTag + "> --colormatrix bt709 --range pc --input - range pc <" + middleTag + "^ --seek 2500 --frames 300 <" + endTag + ">";
            var tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });

            INIReader.iniPath = DirectoryToString(Directory.GetCurrentDirectory()) + "VideoDrop.ini";
            Dictionary<string, string> newSettings = ReadINI();

            VideoDropSettings vdSettings = new VideoDropSettings(ref newSettings);
            vdSettings.SetDebugLevel();
            WriteInfo("ConsoleUI", "Settings List:", DEBUGLEVEL.DEBUG);
            foreach (string key in vdSettings.GlobalSettings.Keys)
            {
                WriteInfo("   ConsoleUI", "Key: " + key + "=" + vdSettings.GlobalSettings[key], DEBUGLEVEL.DEBUG);
            }
            List<string> activeProfiles = vdSettings.ActiveProfiles;
            int totalVideos = activeProfiles.Count * fileList.Count;
            int numFinished = 0;
            int numFailed = 0;
            // Setup_Encoder()
            VideoDropJob vjob = null/* TODO Change to default(_) if this is not a reference type */;
            foreach (var arg in fileList)
            {
                bool successful = true;
                Console.WriteLine("   Current file: " + arg);
                foreach (var profile in activeProfiles)
                {
                    // Generate AVS and set paths
                    FileInfo file = new System.IO.FileInfo(arg);
                    Console.WriteLine("   Starting video #" + (numFinished + 1) + " of " + totalVideos);
                    
                    vjob = new VideoDropJob(arg, vdSettings, profile);

                    Console.WriteLine("   Using profile: " + vjob.ActiveProfile);
                    vjob.GeneratePaths();
                    vjob.PrintPaths();
                    if (!vjob.Run())
                    {
                        successful = false;
                        numFailed += 1;
                    }
                    
                    numFinished += 1;
                }
                
                // Done with all jobs for this video, archive it
                if (successful && !noArchive)
                    vjob.ArchiveSource();
                    
            }

            Console.WriteLine("Finished...");
            if (numFailed > 0)
                Console.WriteLine("   Failed " + numFailed + " jobs.");
            Console.Read();
        }
    }

}

