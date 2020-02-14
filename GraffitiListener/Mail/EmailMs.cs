using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GraffitiListener.Mail
{
    class EmailMsg
    {
        static string fromName = "MES";
        static string fromAdress = "mes@mst.pl";

        public static void SendEmailMessage(string messageText, string subject)
        {
            List<MailboxAddress> toAddresses = new List<MailboxAddress>();
            toAddresses.Add(new MailboxAddress("artur.owsiewski@mst.pl"));
            toAddresses.Add(new MailboxAddress("piotr.dabrowski@mst.pl"));
            toAddresses.Add(new MailboxAddress("katarzyna.kustra@mst.pl"));
            toAddresses.Add(new MailboxAddress("lider.elektronika@mstechnology.pl"));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAdress));
            message.To.AddRange(toAddresses);
            message.Subject = subject;

            var builder = new BodyBuilder();

            //builder.TextBody = messageText;
            builder.HtmlBody = messageText;
            message.Body = builder.ToMessageBody();

            try
            {
                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect("mail.mst.pl", 465, true);
                    client.Authenticate(fromAdress, "Mes!234");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch
            {
                //Console.WriteLine("Send Mail Failed : " + e.Message);
            }
        }
    }
}
