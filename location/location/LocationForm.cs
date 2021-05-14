using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace location
{
    /// <summary>
    /// Current class gather all the values and send to the location.cs to process 
    /// info and show in UI
    /// </summary>
    public partial class LocationForm : Form
    {
        //below variables are created to store user inputs from UI
        string serverNameInput;
        string portNumberInput;
        string userNameInput;
        string locationInput;
        string protocolInput;
        string timeoutInput;
        string debug;

        public LocationForm()
        {
            InitializeComponent();
            serverNameTextBox.Text = "localhost";//defaulr value for current txt box
            portNumberTextBox.Text = "43";//defaulr value for current txt box
            timeoutTextBox.Text = "1000";//defaulr value for current txt box
            respomseTextBox.ReadOnly = true;//The responde text box can be readed only
        }

        private void serverNameTextBox_TextChanged(object sender, EventArgs e)
        {
            serverNameInput = serverNameTextBox.Text;//server name input text box
        }

        private void portNumberTextBox_TextChanged(object sender, EventArgs e)
        {
            portNumberInput = portNumberTextBox.Text;//port number input text box
        }

        private void userNameTextBox_TextChanged(object sender, EventArgs e)
        {
            userNameInput = userNameTextBox.Text;//user name input text box
        }

        private void locationTextBox_TextChanged(object sender, EventArgs e)
        {
            //When location text box is emty then program does the look uo
            if (locationTextBox.Text == "")
            {
                locationInput = null;
            }
            else
            {
                locationInput = locationTextBox.Text;//location input text box
            }

        }

        private void protocolListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //List of the protocols can be choosen
            switch (protocolListBox.SelectedItem)
            {
                case ("WHOIS"):
                    protocolInput = "wre@n";
                    break;
                case ("HTTP/0.9"):
                    protocolInput = "-h9";
                    break;
                case ("HTTP/1.0"):
                    protocolInput = "-h0";
                    break;
                case ("HTTP/1.1"):
                    protocolInput = "-h1";
                    break;
                default:
                    protocolInput ="";
                    break;
            }
        }
        private void timeoutTextBox_TextChanged(object sender, EventArgs e)
        {
            //When timeout text box is emty value of 0 will be set
            if (timeoutTextBox.Text == "")
            {
                timeoutInput = "0";
            }
            else
            {
                timeoutInput = timeoutTextBox.Text;//timeout input text box
            }
            
        }
        private void debugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //Debugging option can be ticked or unticked as user want
            if (debugCheckBox.Checked)
            {
                debug = "-d";
            }
            else
            {
                debug = null;
            }
        }
        private void sendButton_Click(object sender, EventArgs e)
        {
            //These aray gonna contain all the value need to sent to location.cs to process it
            string[] myargs = { "-h", serverNameInput, "-p", portNumberInput, "-t", timeoutInput, debug, protocolInput, userNameInput, locationInput };
            Whois.Main(myargs);//this send the array of value to the location.cs to WHois protocol
            if (debugCheckBox.Checked)//When debugging options is ticked
            {
                respomseTextBox.Text = Whois.response + "\r\n" + Whois.debugResponse;//Print response with debug response
            }
            else
            {
                respomseTextBox.Text = Whois.response;//The server output
            }
            
            
        }

        private void respomseTextBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        
    }
}
