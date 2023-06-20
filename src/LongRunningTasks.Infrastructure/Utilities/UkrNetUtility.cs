using MailKit;
using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Utilities
{
    public static class UkrNetUtility
    {
        public static ImapClient CreateClient()
        {
            var protocolLogger = new ProtocolLogger(Console.OpenStandardError());

            return new ImapClient(protocolLogger);
        }

        public static async Task<ImapClient> SignInAsync(this ImapClient client)
        {
            var credentials = new NetworkCredential("rokossokal@ukr.net", "m09Yn4xIAY5fJqDu");
            var uri = new Uri("imaps://imap.ukr.net");

            await client.ConnectAsync(uri);

            client.AuthenticationMechanisms.Remove("XOAUTH2");

            await client.AuthenticateAsync(credentials);

            return client;
        }

        public static async Task<ImapClient> SignOutAsync(this ImapClient client)
        {
            await client.DisconnectAsync(true);

            return client;
        }
    }
}
