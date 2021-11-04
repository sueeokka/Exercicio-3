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

        SimpleTcpServer server;

        private void btStart_Click(object sender, EventArgs e)
        {
            // Evento para iniciar o servidor TCP
            txtInfo.Text += $"Iniciando Servidor...{Environment.NewLine}";
            server.Start();
            btStart.Enabled = false;
            btSend.Enabled = true;
            txtInfo.Text += $"Servidor Iniciado.{Environment.NewLine}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Inicia o servidor e cria os event handlers
            btSend.Enabled = false;
            server = new SimpleTcpServer(txtIP.Text);
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            // Dados recebidos pelo servidor, ele replica para todos os clientes
            this.Invoke((MethodInvoker)delegate
            {
                string message = Encoding.UTF8.GetString(e.Data);
                txtInfo.Text += $"{e.IpPort} {GetFormattedTimestamp()}: {message}{Environment.NewLine}";

                foreach (var item in lstClientIP.Items)
                {
                    if (item.ToString() != e.IpPort)
                        server.Send(item.ToString(), $"{e.IpPort}¨{message}");
                }
            });
        }

        private void Events_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // Quando um cliente é desconectado, ele é removido da lista de IPs conectados
            this.Invoke((MethodInvoker)delegate
            {
                txtInfo.Text += $"{e.IpPort} Desconectado.{Environment.NewLine}";
                lstClientIP.Items.Remove(e.IpPort);
            });
        }

        private void Events_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            // Quando um cliente é conectado, ele é inserido na lista de IPs conectados
            this.Invoke((MethodInvoker)delegate
            {
                txtInfo.Text += $"{e.IpPort} Conectado.{Environment.NewLine}";
                lstClientIP.Items.Add(e.IpPort);
            });
        }

        private void btSend_Click(object sender, EventArgs e)
        {
            // Quando o servidor envia uma mensagem, ele envia para todos os clientes
            if (server.IsListening)
            {
                if (!string.IsNullOrEmpty(txtMessage.Text))
                {
                    foreach (var item in lstClientIP.Items)
                        server.Send(item.ToString(), $"Servidor¨{txtMessage.Text}");

                    txtInfo.Text += $"Servidor {GetFormattedTimestamp()}: {txtMessage.Text}{Environment.NewLine}";
                    txtMessage.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show("Digite alguma mensagem para ser enviada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Algo deu errado ao se comunicar com os clientes conectados.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // metodo que gera um timestamp do dia e hora, só pra ficar mais parecido com um chat
        private string GetFormattedTimestamp() => $"em {DateTime.Now:dd/MM} ás {DateTime.Now:HH:mm}";

        private void btDisconnectClient_Click(object sender, EventArgs e)
        {
            // Quando o servidor vai desconectar um cliente
            // ele remove o cliente da lista de IPs conectados
            if(lstClientIP.SelectedItem != null)
            {
                string itemIpSelected = lstClientIP.SelectedItem.ToString();
                server.DisconnectClient(itemIpSelected);
                lstClientIP.Items.Remove(itemIpSelected);
            }
            else
            {
                MessageBox.Show("Selecione um cliente para desconectar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
