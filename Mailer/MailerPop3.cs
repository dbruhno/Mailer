using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;

namespace Mailer
{
    public class Pop3Exception : ApplicationException
    {
        public Pop3Exception(string str)
            : base(str)
        {
        }
    };

    public class Pop3Message
    {
        public long number;
        public long bytes;
        public bool retrieved;
        public string message;
    }

    public class MailerPop3 : TcpClient
    {
        private string user;
        private string pass;

        public string login { get { return user; } set { user = value; } }
        public string password { get { return pass; } set { pass = value; } }

        public void Start(string server, int port)
        {
            Connect(server, port);

            string message;
            string response;

            response = Response();

            if (response.Substring(0, 3) != "+OK") { throw new Pop3Exception(response); }

            message = "USER " + user + "\r\n";
            Write(message);

            response = Response();

            if (response.Substring(0, 3) != "+OK") { throw new Pop3Exception(response); }

            message = "PASS " + pass + "\r\n";

            Write(message);

            response = Response();

            if (response.Substring(0, 3) != "+OK") { throw new Pop3Exception(response); }
        }

        public void Disconnect()
        {
            string message;
            string response;

            message = "QUIT\r\n";
            Write(message);

            response = Response();

            if (response.Substring(0, 3) != "+OK") { throw new Pop3Exception(response); }
        }

        public ArrayList list()
        {
            string message;
            string response;

            ArrayList retval = new ArrayList();

            message = "LIST\r\n";
            Write(message);

            response = Response();

            if (response.Substring(0, 3) != "+OK") { throw new Pop3Exception(response); }

            while (true)
            {
                response = Response();
                if (response.Substring(0, 3) == ".\r\n")
                {
                    return retval;
                }
                else
                {
                    Pop3Message msg = new Pop3Message();
                    char[] seps = { ' ' };
                    string[] values = response.Split(seps);
                    msg.number = Int32.Parse(values[0]);
                    msg.bytes = Int32.Parse(values[1]);
                    msg.retrieved = false;

                    retval.Add(msg);

                    continue;
                }
            }
        }

        public Pop3Message retrive(Pop3Message rhs)
        {
            string message;
            string response;

            Pop3Message msg = new Pop3Message();
            msg.number = rhs.number;
            msg.bytes = rhs.bytes;

            message = "RETR " + msg.number + "\r\n";
            Write(message);

            response = Response();

            if (response.Substring(0, 3) != "+OK") { throw new Pop3Exception(response); }

            msg.retrieved = true;

            while (true)
            {
                response = Response();

                if (response.Substring(0, 3) == ".\r\n")
                {
                    break;
                }
                else
                {
                    msg.message += response;
                }
            }

            return msg;
        }

        private void Write(string message)
        {
            ASCIIEncoding en = new ASCIIEncoding();
            byte[] WriteBuffer = new byte[1024];
            
            WriteBuffer = en.GetBytes(message);

            NetworkStream stream = GetStream();
            stream.Write(WriteBuffer, 0, WriteBuffer.Length);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message.Substring(0, message.Length-2));
        }



        private string Response()
        {
            ASCIIEncoding enc = new ASCIIEncoding();

            byte[] serverBuffer = new byte[1024];
            NetworkStream stream = GetStream();

            int count = 0;

            while(true)
            {
                byte[] buf = new byte[2];
                int bytes = stream.Read(buf, 0, 1);
                if (bytes == 1)
                {
                    serverBuffer[count] = buf[0];
                    count++;
                    if (buf[0] == '\n')
                        break;
                }
                else
                    break;
            }

            string retval = enc.GetString(serverBuffer);

            if (retval.Substring(0, 3) == "+OK")
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.White;
            count -= 2;
            count = (count < 0) ? 0 : count;
            Console.WriteLine(retval.Substring(0, count));

            return retval;
        }
    }
}
