using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;
using Tools;

namespace FFmpeg
{
    public class handler
    {
        public handler() { }
        public handler(BetterEventHandler _Exited) { Exited = _Exited; }
        public handler(BetterEventHandler _Exited, object userToken) { Exited = _Exited; UserToken = userToken; }
        public handler(BetterEventHandler _Exited, DataReceivedEventHandler _OutputDataReceived) { Exited = _Exited; OutputDataReceived = _OutputDataReceived; }

        public BetterEventHandler Exited = null;
        public DataReceivedEventHandler OutputDataReceived = null;
        public object UserToken = null;
    }
    public class FFmpegAPI : MonoBehaviour
    {
        public static void Convert(string inputFile, string outputFile, handler _handler)
        {
            string opt = "-y -i \"" + inputFile + "\" -map 0:0 -acodec libvorbis \"" + outputFile + "\"";

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(FFmpegOut.FFmpegConfig.BinaryPath, opt)
            {
                /*UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true*/
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = _handler.OutputDataReceived != null
            };
            process.EnableRaisingEvents = true;
            if (_handler.Exited != null) process.Exited += (sender, args) => _handler.Exited(sender, new BetterEventArgs(_handler.UserToken));
            if (_handler.OutputDataReceived != null) process.OutputDataReceived += _handler.OutputDataReceived;
            process.OutputDataReceived += (s, e) => Console.Out.WriteLine(e.Data);

            process.Start();
            if (_handler.OutputDataReceived != null) process.BeginOutputReadLine();
        }
    }
}