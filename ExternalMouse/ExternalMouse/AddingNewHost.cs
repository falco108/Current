﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace ExternalMouse
{
    delegate void ProcessInvitationBroadcastResponse(byte[] data, IPEndPoint ep);
    public partial class AddingNewHost : Form
    {
        public AddingNewHost()
        {
            InitializeComponent();
        }
        public void _ProcessInvitationBroadcastResponse(byte[] data, IPEndPoint ep)
        {
            byte bt;
            string str;
            Program.PostLog("Processing INVITATION_RESPONSE");
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                bt = br.ReadByte();
                str = br.ReadString();
            }
            ListViewItem lvi = new ListViewItem();
            lvi.Text = str.Split('|')[0];
            lvi.Tag = str;
            lvi.SubItems.Add(ep.Address.ToString());
            listView1.Invoke(new MethodInvoker(() => { listView1.Items.Add(lvi); }));
        }

        private void button_SendBroadcast_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            Program.udpConnector.SendInvitationBroadcast(_ProcessInvitationBroadcastResponse);
        }

        public static byte[] MakeResponseOnInvitationBroadcast(byte[] data)
        {
            string str = Program.pairedHosts.GetLocalHostInfo();
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(UdpConnector.INVITATION_RESPONSE);
                bw.Write(str);
                return ms.ToArray();
            }
            return Encoding.Unicode.GetBytes(str);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1) return;
            string ipaddress = listView1.SelectedItems[0].SubItems[1].Text;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(UdpConnector.SHOW_PASSKEY);
                Program.udpConnector.Send(ipaddress, ms.ToArray(), true);
            }

            CodewordInputDialog codewordInputDialog = new CodewordInputDialog();
            codewordInputDialog.ShowDialog();
        }
    }
}