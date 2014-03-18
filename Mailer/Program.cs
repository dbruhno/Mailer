using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;


namespace Mailer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MailerPop3 pop3 = new MailerPop3();

                pop3.login = "dimabrukhn@rambler.ru";
                pop3.password = "U6iTD3K4";

                pop3.Start("mail.rambler.ru", 110);

                ArrayList list = pop3.list();
                
                pop3.retrive((Pop3Message)list[0]);

                pop3.Disconnect();
            }
            catch (Pop3Exception e)
            {
                Console.WriteLine("Pop3 error");
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadKey();
        }
    }
}
