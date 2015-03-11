using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace ClusterLogAnalyzer
{
    public class EmailUtils
    {
        public static void MailReport(string subject, string body, string toLine, string displayName, string additionalMsgBodyFile, string fileAttachments)
        {           
            var additionalMsgBody = String.Empty;
            if (!String.IsNullOrWhiteSpace(additionalMsgBodyFile) && File.Exists(additionalMsgBodyFile))
            {
                additionalMsgBody = File.ReadAllText(additionalMsgBodyFile);
            }

            MailReportWithAdditionalMessageBody(subject: subject, body: body, toLine: toLine,
                displayName:displayName, additionalMsgBody: additionalMsgBody, fileAttachments: fileAttachments);
        }

        public static void MailReportWithAdditionalMessageBody(string subject, string body, string toLine, 
            string displayName,
            string additionalMsgBody, string fileAttachments)
        {
            string fromAddress = Environment.GetEnvironmentVariable("USERNAME") + "@microsoft.com";
            var name = displayName;
            SmtpClient client = new SmtpClient("smtphost");
            client.UseDefaultCredentials = true;

            Console.WriteLine("SMTP Server: {0}, from address: {1}", client.Host, fromAddress);
            MailAddress from = new MailAddress(fromAddress, name, Encoding.ASCII);

            List<MailAddress> to = new List<MailAddress>();

            // parse to and file attachments
            string[] toAddresses = toLine.Split(";,".ToCharArray());
            foreach (string toAddress in toAddresses)
            {
                if (!string.IsNullOrWhiteSpace(toAddress))
                {
                    to.Add(new MailAddress(toAddress));
                }
            }

            List<Attachment> attachmentList = new List<Attachment>();
            if (!string.IsNullOrWhiteSpace(fileAttachments))
            {
                string[] attachments = fileAttachments.Split(";,".ToCharArray());
                foreach (var attachment in attachments)
                {
                    if (File.Exists(attachment))
                    {
                        attachmentList.Add(new Attachment(attachment));
                    }
                }
            }

            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = from;
            to.ForEach(entry => message.To.Add(entry));


            var messageBody = new StringBuilder();
            messageBody.AppendLine("<FONT face=Calibri>");
            messageBody.AppendLine(String.Format("{0}\r\n", body));

            if (!String.IsNullOrWhiteSpace(additionalMsgBody))
            {

                messageBody.AppendLine("<hr>");
                messageBody.AppendLine(String.Format("{0}\r\n", additionalMsgBody));
                messageBody = messageBody.Replace(Environment.NewLine, "<br><br>");
            }
            message.Body = messageBody.ToString();

            attachmentList.ForEach(file => message.Attachments.Add(file));

            message.BodyEncoding = Encoding.UTF8;
            message.Subject = subject;
            message.SubjectEncoding = Encoding.UTF8;

            client.Send(message);
        }
    }
}
