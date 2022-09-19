using System;
using System.Net.Sockets;
using System.Text;
using System.Net.Mail;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;


namespace Server
{
    public class ClientObject
    {
        public const int BufferSize = 65536;
        public TcpClient client;
        public MailMessage mailMessage;
        private MailAddress from;
        private MailAddress to;
        private bool SendToConsole;
        const string help = "\r\nHELO <SP> <domain> <CRLF>\r\nMAIL <SP> FROM:<reverse-path> <CRLF>\r\n" +
            "RCPT <SP> TO:<forward-path> <CRLF>\r\nDATA <CRLF>\r\nRSET <CRLF> \r\n" +
            "SEND <SP> FROM:<reverse-path> <CRLF> \r\nSOML <SP> FROM:<reverse-path> <CRLF>\r\n" +
            "SAML <SP> FROM:<reverse-path> <CRLF> \r\nVRFY <SP> <string> <CRLF> \r\n" +
            "EXPN <SP> <string> <CRLF> \r\nNOOP <CRLF> \r\nQUIT <CRLF>\r\n";

        private mainForm form;
        public ClientObject(TcpClient tcpClient, mainForm mForm)
        {
            client = tcpClient;
            form = mForm;
            mailMessage = null;
            from = null;
            to = null;
            SendToConsole = false;
        }
        static List<MailAddress> clients = new List<MailAddress>()
        {
            new MailAddress("romanshidlovsky@mail.ru", "Roman"),
            new MailAddress("TomSh@gmail.com", "Tom"),
            new MailAddress("05100200@bsuir.by", "Roman"),
        };

        

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                while (true)
                {
                    string message = Read(stream);

                    


                    // message = message.Substring(message.IndexOf(':') + 1).Trim();
                    if (message.Length > 0)
                    {


                        if (message.StartsWith("QUIT"))
                        {
                            client.Close();
                            Write("Server is closing transmission channel", stream);
                            break;
                        }
                        else
                        if (message.StartsWith("EHLO") || message.StartsWith("HELO"))
                        {
                            Write("250 server is ready", stream);
                        }
                        else
                        if (message.StartsWith("MAIL"))
                        {
                            string[] parts = message.Split(" ");
                            if (parts.Length > 2)
                            {
                                string addr = parts[2].Trim(new Char[] { '\r', '\n' });
                                if (IsValidEmail(addr))
                                {
                                    from = new MailAddress(addr);
                                    Write($"250 {addr}", stream);
                                }
                                else
                                {
                                    Write("553 Requested action not taken: mailbox name not allowed", stream);
                                }
                            }
                            else
                            {
                                Write("501 MAIL <SP> FROM:<reverse-path> <CRLF>", stream);
                            }
                        }
                        else
                        if (message.StartsWith("RCPT"))
                        {
                            string[] parts = message.Split(" ");
                            if (parts.Length > 2)
                            {
                                string addr = parts[2].Trim(new Char[] { '\r', '\n' });
                                if (IsValidEmail(addr))
                                {
                                    to = new MailAddress(addr);
                                    Write($"250 {addr}", stream);
                                }
                                else
                                {
                                    Write("553 Requested action not taken: mailbox name not allowed", stream);
                                }
                            }
                            else
                            {
                                Write("501 RCPT <SP> TO:<forward-path> <CRLF> ", stream);
                            }
                        }
                        else
                        if (message.StartsWith("DATA"))
                        {
                            if (from != null && to != null)
                            {
                                mailMessage = new MailMessage(from, to);
                                Write("354 Start mail input; end with<CRLF>.<CRLF>", stream);
                                byte[] data = new byte[BufferSize];
                                StringBuilder builder = new StringBuilder();
                                int bytes = 0;
                                while (!builder.ToString().EndsWith("\r\n.\r\n"))
                                {
                                    Write("", stream);
                                    bytes = stream.Read(data, 0, data.Length);

                                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                                }
                                bytes = stream.Read(data, 0, data.Length);
                                string msgData = builder.ToString();
                                mailMessage.Body = msgData;
                                if (SendToConsole)
                                {
                                    Write("\r\n" + msgData + "\r\n250 OK\r\n", stream);
                                    SendToConsole = false;
                                }
                                else
                                {
                                    Write("250 OK", stream);
                                }
                            }
                            else
                            {
                                Write("503 Bad sequence of commands", stream);
                            }

                        }
                        else
                        if (message.StartsWith("RSET"))
                        {
                            mailMessage.Dispose();
                            from = null;
                            to = null;
                            Write("250 Reseted", stream);
                        }
                        else
                        if (message.StartsWith("VRFY"))
                        {
                            string response = "";
                            string[] parts = message.Split(" ");
                            if (parts.Length > 1)
                            {
                                string name = parts[1].Trim(new Char[] { '\r', '\n' });
                                foreach (var item in clients)
                                {
                                    if (item.DisplayName == name)
                                        response += item.Address + "\r\n";
                                }
                                if (response != "")
                                {
                                    Write("\r\n" + response, stream);
                                }
                                else
                                {
                                    Write("551 User not local", stream);
                                }

                            }
                            else
                            {
                                Write("501 VRFY <SP> <string> <CRLF> ", stream);
                            }

                        }
                        else
                        if (message.StartsWith("NOOP"))
                        {
                            Write("250 OK", stream);
                        }
                        else
                        if (message.StartsWith("EXPN"))
                        {
                            string[] parts = message.Split(" ");
                            if (parts.Length > 1)
                            {
                                string addr = parts[1].Trim(new Char[] { '\r', '\n' });
                                if (IsValidEmail(addr))
                                {
                                    Write($"250 Valid email: {addr}", stream);
                                }
                                else
                                {
                                    Write("553 Requested action not taken: mailbox name not allowed", stream);
                                }
                            }
                            else
                            {
                                Write("501 EXPN <SP> <string> <CRLF> ", stream);
                            }
                        }
                        else
                        if (message.StartsWith("HELP"))
                        {
                            Write(help, stream);
                        }
                        else
                        if (message.StartsWith("SEND") || message.StartsWith("SOML") || message.StartsWith("SAML"))
                        {
                            string[] parts = message.Split(" ");
                            if (parts.Length > 2)
                            {
                                string addr = parts[2].Trim(new Char[] { '\r', '\n' });

                                if (IsValidEmail(addr))
                                {
                                    from = new MailAddress(addr);
                                    Write($"250 {addr}", stream);
                                    SendToConsole = true;
                                }
                                else
                                {
                                    Write("553 Requested action not taken: mailbox name not allowed", stream);
                                }
                            }
                            else
                            {
                                Write("501 Error syntax", stream);
                            }

                        }
                        else Write("500 Syntax error, command unrecognised", stream);
                    }
                    else Write("500 Syntax error, command unrecognised", stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();

            }
        }

        private void Write(string message, NetworkStream stream)
        {
            byte[] data = new byte[BufferSize];
            data = Encoding.Unicode.GetBytes(message + "\r\n");
            stream.Write(data, 0, data.Length);
            stream.Flush();
            if (message != "")
            {
                Thread thread1 = new Thread(new ParameterizedThreadStart(form.echo));
                thread1.Start($"S: {message}\r\n");
            }
            
        }

        private string Read(NetworkStream stream)
        {
            byte[] data = new byte[BufferSize];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (stream.DataAvailable);

            string message = builder.ToString();
            
            Thread thread1 = new Thread(new ParameterizedThreadStart(form.echo));
            thread1.Start($"C: {message}");
            // form.echo(message);
            return message;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
