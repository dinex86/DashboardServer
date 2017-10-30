using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DashboardTm1638
{
    public partial class MainForm : Form
    {
        private Tm1638Handler handler = new Tm1638Handler();
        
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            String[] ports = SerialPort.GetPortNames();
            bool hasPorts = ports.Length > 0;
            if (hasPorts)
            {
                cboPorts.Items.AddRange(ports);
                cboPorts.SelectedIndex = 0;
                //lblConn.Text = "Looking for a game...";
            }
            else
            {
                //lblConn.Text = "No Arduino found!";
            }

            cboPorts.Enabled = hasPorts;
            btnStartStop.Enabled = hasPorts;
            btnTest.Enabled = hasPorts;

            // Load connector.
            
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (!handler.IsConnected)
            {
                handler.Connect(cboPorts.SelectedItem.ToString());
                handler.Start();
                btnStartStop.Text = "Stop";
                //btnTest.Enabled = false;

                btnTest.Enabled = false;
            }
            else
            {
                handler.Stop();
                handler.Disconnect();
                btnStartStop.Text = "Start";
                //tmr.Enabled = false;
                //cmbSerial.Text = "Start";
                //btnTest.Enabled = true;

                btnTest.Enabled = true;
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (!handler.IsConnected)
            {
                handler.Connect(cboPorts.SelectedItem.ToString());
                handler.StartTest();
                btnTest.Text = "Stop";
                //btnTest.Enabled = false;

                btnStartStop.Enabled = false;
            }
            else
            {
                handler.StopTest();
                handler.Disconnect();
                btnTest.Text = "Test";
                //tmr.Enabled = false;
                //cmbSerial.Text = "Start";
                //btnTest.Enabled = true;

                btnStartStop.Enabled = true;
            }
        }

        private void cboPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnStartStop.Enabled = true;
            //btnTest.Enabled = true;
        }
    }
}
