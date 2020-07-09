using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace WinAutoMessenger
{
    public partial class FormMain : Form
    {
        private readonly ConnectionHelper m_connection;
        private readonly Shell m_shell;

        private readonly String m_initstate;
        private void set_state(String state)
        {
            this.Text = state != null ? state : m_initstate;
        }

        public FormMain()
        {
            InitializeComponent();
            this.m_initstate = this.Text;

            m_connection = new ConnectionHelper();
            m_shell = new Shell();

            //CommandManager.get_cpu_info();
            //CommandManager.to_json(CommandManager.get_storage_info().Value);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.pnlConIO.Enabled = false;
        }

        private void BtnListen_Click(object sender, EventArgs e)
        {
            if (m_connection.IsListening == false && m_connection.IsConnected == false)
            {
                //listen
                this.Tag = new Thread((x) =>
                {
                    m_connection.Start(decimal.ToInt32(this.numPort.Value));
                }
                );

                ((Thread)this.Tag).Start();

                if (!this.m_shell.IsActive)
                    this.m_shell.Run();
            }
            else
            {
                //end
                m_connection.Close();
            }
        }

        private void BtnWrite_Click(object sender, EventArgs e)
        {
            String data = txtSendData.Text;
            if (String.IsNullOrEmpty(data) == false)
            {
                if(m_connection.SendString(data))
                {
                    txtSendData.Text = "";
                }
                else
                {
                    MessageBox.Show("Failed to send data!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No data to send!", "No data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void TmrUpdate_Tick(object sender, EventArgs e)
        {
            if (m_connection == null)
                return;

            if(m_connection.IsConnected == false)
            {
                if (m_connection.IsListening)
                {
                    this.set_state("Listening...");
                    this.btnListen.Text = "End Listen";
                    this.numPort.Enabled = false;
                    this.pnlConIO.Enabled = false;

                }
                else
                {
                    this.set_state(null);
                    this.btnListen.Text = "Start Listen";
                    this.numPort.Enabled = true;
                    this.pnlConIO.Enabled = false;
                }
            }
            else
            {
                this.set_state("Connected!");
                this.btnListen.Text = "Close Connection";
                this.numPort.Enabled = false;
                this.pnlConIO.Enabled = true;



                //IO Tick

                //shell interface

                string inp = m_connection.ReceiveString();

                if (m_shell.IsActive)
                {
                    string shell_out = m_shell.Read();

                    if (shell_out != null)
                    {
                        this.m_connection.SendString(shell_out);

                        txtData.AppendText(Environment.NewLine + shell_out);


                    }
                    if (inp != null)
                    {
                        this.m_shell.StandardInput.Write(inp);
                    }
                }
                else
                {
                    string resp = CommandManager.RunCommand(inp);

                    if (resp != null)
                    {
                        this.m_connection.SendString(Environment.NewLine + resp);
                        txtData.AppendText(Environment.NewLine + resp);
                    }
                }
                
            }
        }

        
    }
}
