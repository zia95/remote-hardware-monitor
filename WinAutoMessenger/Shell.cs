using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace WinAutoMessenger
{
    public class Shell
    {
        public Process Process { get; private set; }
        public StreamWriter StandardInput { get { return this.Process?.StandardInput; } }
        public bool IsActive { get { return (this.Process != null && this.Process.HasExited == false); } }

        private StringBuilder m_out_data;
        //private bool m_out_clear;

        public String Read()
        {
            if(m_out_data != null)
            {
                String __str = m_out_data.ToString();
                m_out_data = null;
                return __str;
            }
            return null;
        }

        private void data_received_handler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                if (m_out_data == null)
                    m_out_data = new StringBuilder();

                m_out_data.Append($"{Environment.NewLine}{outLine.Data}");
            }
        }

        public void Run(String process = "cmd.exe", String args = null)
        {
            if (this.IsActive)
                return;

            this.Process = new Process();
            this.Process.StartInfo.FileName = process;
            this.Process.StartInfo.Arguments = args;
            this.Process.StartInfo.CreateNoWindow = true;
            this.Process.StartInfo.UseShellExecute = false;
            this.Process.StartInfo.RedirectStandardOutput = true;
            this.Process.StartInfo.RedirectStandardInput = true;
            this.Process.StartInfo.RedirectStandardError = true;
            this.Process.OutputDataReceived += new DataReceivedEventHandler(data_received_handler);
            this.Process.ErrorDataReceived += new DataReceivedEventHandler(data_received_handler);

            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
        }
        public void  Reset()
        {
            if (this.Process == null)
                return;

            this.Process.Dispose();
            this.Process = null;
            this.m_out_data = null;
        }
        
    }
}
