using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwoFactorAuthNet;
using System.Security.Cryptography;
using System.Reflection;
using Microsoft.Win32;

namespace TFA_Test
{
    public partial class Form1 : Form
    {
        TwoFactorAuth tfa;
       String secret =null;
        public Form1()
        {
            InitializeComponent();
            tfa = new TwoFactorAuth("TFA Test", qrcodeprovider: new MyQRProvider());
            // string curFile = @"Auth.txt";
            // Console.WriteLine(File.Exists(curFile) ? "File exists." : "File does not exist.");
            if (File.Exists(@"Auth.txt"))
            {
                panel1.Hide();
                panel2.Location = panel1.Location;
                //opening the subkey  
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TFA Store");
                //if it does exist, retrieve the stored values  
                if (key != null)
                {
                    //Password = key.GetValue("Password").ToString();
                    //Name = key.GetValue("Name").ToString();
                    Console.WriteLine(key.GetValue("Name"));
                    Console.WriteLine(key.GetValue("Data"));
                    String key2=key.GetValue("Data").ToString();
                    key.Close();
                    Console.WriteLine(key2);
                    byte [] decrypted=AES_Decrypt(Encoding.Unicode.GetBytes(key2), Encoding.ASCII.GetBytes("fisher89"));
                    Console.WriteLine(Encoding.ASCII.GetChars(decrypted));
                    secret = new String(Encoding.ASCII.GetChars(decrypted)).Split(',')[1];
                }
                else
                {
                    panel1.Show();

                }
            }
            else
            {
                panel2.Hide();
                panel1.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string FullName = textBox1.Text;
            if (FullName.Length != 0 && maskedTextBox1.TextLength !=0 )
            {
                secret = tfa.CreateSecret(160);
                var uri = tfa.QrCodeProvider.GetQrCodeImage(String.Format("otpauth://totp/{0}?secret={1}&issuer=TFA Store", FullName, secret), 150);
                //Console.WriteLine(System.Text.UTF8Encoding.UTF8.GetString(uri));
                Image x = (Bitmap)((new ImageConverter()).ConvertFrom(uri));
                pictureBox1.Image = x;
                RegiStrKey(FullName, secret);
            }
        }
        private void RegiStrKey(string name,string secret)
        {
            
                StringBuilder encryptData = new StringBuilder();
                encryptData.Append(name+","+secret+","+maskedTextBox1.Text);
                byte [] encrypted=AES_Encrypt(Encoding.ASCII.GetBytes(encryptData.ToString()),Encoding.ASCII.GetBytes(maskedTextBox1.Text));
                
                //opening the subkey  
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\TFA Store");
                key.SetValue("Name", name);
                key.SetValue("Data", new String(Encoding.Unicode.GetChars(encrypted)));
                key.Close();
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        private void button2_Click(object sender, EventArgs e)
        {
          
            CheckQrCode(tfa, secret);
            /*byte[] bytes = Encoding.ASCII.GetBytes(maskedTextBox1.Text);
            Console.WriteLine(Encoding.ASCII.GetByteCount(maskedTextBox1.Text));
            foreach(Byte b in bytes)
            {
                Console.WriteLine(b);
            }
            using (AesManaged myAes = new AesManaged())
            {
                myAes.Mode = CipherMode.ECB;

                myAes.IV = bytes;
                Console.WriteLine(myAes.IV.Length);
                myAes.Key = Encoding.ASCII.GetBytes(secret);
                Console.WriteLine(myAes.Key.Length);
                ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);

                using (CryptoStream csEncrypt = new CryptoStream(new MemoryStream(), encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(Encoding.ASCII.GetBytes("ciao come stai"), 0, 13);
                    Console.WriteLine(csEncrypt.ToString());
                }

            }*/
        }
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        //cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
                
            }

            return encryptedBytes;
        }
        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
        private void CheckQrCode(TwoFactorAuth tfa, string secret)
        {
            Console.WriteLine(((DateTimeOffset)GetTimeServer()).ToUnixTimeSeconds());
            if (tfa.VerifyCode(secret, maskedTextBox3.Text.Replace(maskedTextBox3.PromptChar.ToString(),""), 1, timestamp: ((DateTimeOffset)GetTimeServer()).ToUnixTimeSeconds() + 30)) //Code must be enter in 30 seconds before expire on google auth.
            {
                    label3.Text="Login Successful";
                    Console.WriteLine("true");
            }
            else
            {
                label3.Text = "Password Timeout";
                Console.WriteLine("false");
            }
        }
        private static DateTime GetTimeServer()
        {
            try
            {
                using (var response =
                  WebRequest.Create("http://www.google.com").GetResponse())
                    //string todaysDates =  response.Headers["date"];
                    return DateTime.ParseExact(response.Headers["date"],
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal);
            }
            catch (WebException)
            {
                return DateTime.Now; //In case something goes wrong. 
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if(textBox1.Text == "Full Name")
                textBox1.Clear();
            textBox1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
        }

        private void maskedTextBox3_Enter(object sender, EventArgs e)
        {
            maskedTextBox3.Focus();
            maskedTextBox3.SelectionStart = 0;
        }

        private void maskedTextBox3_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox3.Focus();
            maskedTextBox3.SelectionStart = 0;
        }
    }
       /* public static DateTime GetServerTime()
        {
            var result = DateTime.Now;

            // Initialize the list of NIST time servers
            // http://tf.nist.gov/tf-cgi/servers.cgi
            string[] servers = new string[] {

                "time-c.nist.gov",
                "time-d.nist.gov",
                "nist1-macon.macon.ga.us",
                "wolfnisttime.com",
                "nist.netservicesgroup.com",
                "nisttime.carsoncity.k12.mi.us",
                "nist1-lnk.binary.net",
                "wwv.nist.gov",
                "time.nist.gov",
                "utcnist.colorado.edu",
                "utcnist2.colorado.edu",
                "nist-time-server.eoni.com",
                "nist-time-server.eoni.com"
            };
            Random rnd = new Random();
            foreach (string server in servers.OrderBy(x => rnd.NextDouble()).Take(9))
            {
                try
                {
                    // Connect to the server (at port 13) and get the response. Timeout max 1second
                    string serverResponse = string.Empty;
                    var tcpClient = new TcpClient();
                    if (tcpClient.ConnectAsync(server, 13).Wait(1000))
                    {
                        using (var reader = new StreamReader(tcpClient.GetStream()))
                        {
                            serverResponse = reader.ReadToEnd();
                        }
                    }
                    // If a response was received
                    if (!string.IsNullOrEmpty(serverResponse))
                    {
                        // Split the response string ("55596 11-02-14 13:54:11 00 0 0 478.1 UTC(NIST) *")
                        string[] tokens = serverResponse.Split(' ');

                        // Check the number of tokens
                        if (tokens.Length >= 6)
                        {
                            // Check the health status
                            string health = tokens[5];
                            if (health == "0")
                            {
                                // Get date and time parts from the server response
                                string[] dateParts = tokens[1].Split('-');
                                string[] timeParts = tokens[2].Split(':');

                                // Create a DateTime instance
                                DateTime utcDateTime = new DateTime(
                                Convert.ToInt32(dateParts[0]) + 2000,
                                Convert.ToInt32(dateParts[1]), Convert.ToInt32(dateParts[2]),
                                Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]),
                                Convert.ToInt32(timeParts[2]));

                                // Convert received (UTC) DateTime value to the local timezone
                                result = utcDateTime.ToLocalTime();

                                return result;
                                // Response successfully received; exit the loop

                            }
                        }

                    }

                }
                catch
                {
                    // Ignore exception and try the next server
                }

            }
            return result;
        }
        }*/
}
