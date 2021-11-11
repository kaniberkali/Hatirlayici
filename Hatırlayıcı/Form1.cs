using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

namespace Hatırlayıcı
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        OleDbConnection baglanti = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=veritabani.accdb");
        OleDbCommand komut = new OleDbCommand();
        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                baglanti.Open();
                baglanti.Close();
                Thread klavyethread = new Thread(klavyeoku); klavyethread.Start();
                Thread kopyahread = new Thread(kopyaoku); kopyahread.Start();
                verilerigetir();
                try
                {
                    RegistryKey uygulama = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    uygulama.SetValue("Hatirlayıcı", "\"" + Application.ExecutablePath + "\"");
                }
                catch { }
            }
            catch 
            {
                if (File.Exists("veritabani.accdb") == false)
                {
                    MessageBox.Show("Lütfen programın bulunduğu klasöre veritabanini atın. Eğer veritabanını bulamıyorsanız programı tekrar indirin.", "@kodzamani.tk");
                    Application.Exit();
                }
            }
        }

        [DllImport("User32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        private void klavyeoku()
        {
            for (; ; )
            {
                Thread.Sleep(50);
                if (this.Visible == false)
                {
                    int ctrl = GetAsyncKeyState(16);
                    int shift = GetAsyncKeyState(17);
                    int c = GetAsyncKeyState(67);
                    if (ctrl != 0 && shift != 0 && c != 0)
                        this.Show();
                }
            }
        }
        string gelenkopya = "";
        private void verilerigetir()
        {
            try
            {
                baglanti.Open();
                OleDbDataReader reader = null;
                komut = new OleDbCommand("select* from metinler", baglanti);
                reader = komut.ExecuteReader();
                while (reader.Read())
                {
                    listBox1.Items.Add(reader["Metin"].ToString());
                }
                baglanti.Close();
            }
            catch { }
        }
        private void kopyaoku()
        {
            for (; ; )
            {
                
                if (kopya!=""&&kopya != gelenkopya&& listBox1.Items.Contains(kopya)==false)
                {
                    gelenkopya = kopya;
                    try
                    {
                        komut = new OleDbCommand("INSERT into metinler (Tarih, Metin) Values('" + DateTime.Now + "','" + gelenkopya + "')");
                        komut.Connection = baglanti;
                        baglanti.Open();
                        komut.ExecuteNonQuery();
                        baglanti.Close();
                        listBox1.Items.Insert(0, gelenkopya);
                    }
                    catch { }
                }
                Thread.Sleep(1000);
            }
        }
        string kopya = "";
        private void gettext_Tick(object sender, EventArgs e)
        {
            try
            {
                kopya = Clipboard.GetText();
            }
            catch { }
        }


        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    if (listBox1.Text != ""&&listBox1.SelectedIndex!=-1)
                    {
                        Clipboard.SetText(listBox1.Text);
                    }
                }
                catch { }
            }
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    string sil = listBox1.Text;
                    if (sil != "")
                    {
                        Clipboard.SetText("https://kodzamani.weebly.com");
                        kopya = "https://kodzamani.weebly.com";
                        baglanti.Open();
                        komut.Connection = baglanti;
                        komut.CommandText = "delete from metinler where Metin='" + sil + "'";
                        komut.ExecuteNonQuery();
                        baglanti.Close();
                        listBox1.SelectedIndex = -1;
                        listBox1.Items.Remove(sil);
                    }
                }
                catch
                {
                   
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            Move_Z = 1;
            Mouse_X = e.X;
            Mouse_Y = e.Y;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            Move_Z = 0;
        }
        int Move_Z;
        int Mouse_X;
        int Mouse_Y;
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Move_Z == 1)
            {
                this.SetDesktopLocation(MousePosition.X - Mouse_X, MousePosition.Y - Mouse_Y);
            }
        }
    }
}
