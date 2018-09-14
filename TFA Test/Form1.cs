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

namespace TFA_Test
{
    public partial class Form1 : Form
    {
        TwoFactorAuth tfa;
        String secret;
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string testo = textBox1.Text;
            tfa = new TwoFactorAuth("TFA Test",qrcodeprovider: new MyQRProvider());
            secret = tfa.CreateSecret(160);
            var uri = tfa.QrCodeProvider.GetQrCodeImage(String.Format("otpauth://totp/{0}?secret={1}&issuer=TFA Test", textBox1.Text, secret), 250);
            //Console.WriteLine(System.Text.UTF8Encoding.UTF8.GetString(uri));
            Image x = (Bitmap)((new ImageConverter()).ConvertFrom(uri));
            pictureBox1.Image = x;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CheckQrCode(tfa, secret);
        }

        private void CheckQrCode(TwoFactorAuth tfa,string secret)
        {
            Console.WriteLine(((DateTimeOffset)GetTimeServer()).ToUnixTimeSeconds());
            if (tfa.VerifyCode(secret, textBox2.Text, 1,timestamp: ((DateTimeOffset)GetTimeServer()).ToUnixTimeSeconds()+30)) //Code must be enter in 30 seconds before expire on google auth.
            {
                Console.WriteLine("true");
            }
            else
            {
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
