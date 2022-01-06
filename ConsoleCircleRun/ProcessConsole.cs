﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace ConsoleCircleRun
{
    public class ProcessConsole
    {
        public ProcessConsole(string fileName, string arguments, ILogger Log)
        {
            _fileName = fileName;
            _arguments = arguments;
            _Log = Log;
        }
        public virtual void Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(_fileName, _arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };
            _process = new Process { StartInfo = startInfo };
            _process.OutputDataReceived += OutputDataReceivedEventHandler;
            _process.ErrorDataReceived += ErrorDataReceivedEventHandler;
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _input = _process.StandardInput;
            Thread thread = new Thread(InputThread) { IsBackground = true };
            thread.Start();
        }

        public virtual void WaitForExit()
        {
            _process.WaitForExit();
        }

        public virtual int ExitCode
        {
            get
            {
                return _process.ExitCode;
            }
        }

        protected virtual void InputThread()
        {

            ConsoleKeyInfo keyInfo;
            while (true)
            {
                _input.Flush();
                keyInfo = Console.ReadKey(true);
                _input.Write(keyInfo.KeyChar);
                Console.WriteLine("Input :" + keyInfo.KeyChar);
            }
        }

        protected virtual void OutputDataReceivedEventHandler(Object sender, DataReceivedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Data) == false)
            {
                _output = e.Data;
                //Console.WriteLine("Output : " + _output);
                _Log.LogInformation(_output);
            }
        }

        protected virtual void ErrorDataReceivedEventHandler(Object sender, DataReceivedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Data) == false)
            {
                _error = (e.Data + Environment.NewLine);
                //$"Error : {_error}".ConsoleRed();
                _Log.LogError(_output);
            }
        }

        readonly string _fileName;
        readonly string _arguments;
        private readonly ILogger _Log;
        private Process _process;
        private string _output;
        private string _error;
        StreamWriter _input;
    }
}