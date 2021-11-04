using SimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SimpleTcpClient client;

        private void btConnect_Click(object sender, EventArgs e)
        {
            //conecta o cliente com o servidor
            try
            {
                client.Connect();
                btSend.Enabled = true;
                btConnect.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btDisconnect_Click(object sender, EventArgs e)
        {
            //Desconecta o cliente do servidor
            if (client.IsConnected)
            {
                try
                {
                    client.Disconnect();
                    btSend.Enabled = false;
                    btConnect.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else {
                    MessageBox.Show("Precisa estar conectado para desconectar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
        }

        private void btSend_Click(object sender, EventArgs e)
        {
            //Envia a mensagem para o servidor e ele propaga para todos os clientes
            if (client.IsConnected)
            {
                if (!string.IsNullOrEmpty(txtMessage.Text))
                {
                    client.Send(txtMessage.Text);
                    txtInfo.Text += $"Eu {GetFormattedTimestamp()}: {txtMessage.Text}{Environment.NewLine}";
                    txtInfo.Text += string.Empty;
                    txtMessage.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show("Digite alguma mensagem para ser enviada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Algo deu errado ao se comunicar com o servidor.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // cria um novo cliente e os event handlers
            client = new SimpleTcpClient(txtIP.Text);
            client.Events.Connected += Events_Connected;
            client.Events.DataReceived += Events_DataReceived;
            client.Events.Disconnected += Events_Disconnected;
            btSend.Enabled = false;
        }

        private void Events_Disconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // dispara um aviso caso esse cliente seja desconectado do servidor
            this.Invoke((MethodInvoker)delegate
            {
                txtInfo.Text += $"Servidor Desconectado.{Environment.NewLine}";
                btConnect.Enabled = true;
            });
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            // quando esse cliente recebe dados do servidor
            this.Invoke((MethodInvoker)delegate
            {
                string[] receivedData = Encoding.UTF8.GetString(e.Data).Split("¨");
                txtInfo.Text += $"{receivedData[0]} {GetFormattedTimestamp()}: {receivedData[1]}{Environment.NewLine}";
            });
        }

        private void Events_Connected(object sender, ClientConnectedEventArgs e)
        {
            // quando é conectado com o servidor
            this.Invoke((MethodInvoker)delegate
            {
                txtInfo.Text += $"Servidor Conectado.{Environment.NewLine}";
            });
        }

        // metodo que gera um timestamp do dia e hora, só pra ficar mais parecido com um chat
        private string GetFormattedTimestamp() => $"em {DateTime.Now:dd/MM} ás {DateTime.Now:HH:mm}";

    }
}
