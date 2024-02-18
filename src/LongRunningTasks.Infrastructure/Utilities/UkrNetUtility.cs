using MailKit;
using MailKit.Net.Imap;
using System.Net;

namespace LongRunningTasks.Infrastructure.Utilities
{
    public static class UkrNetUtility
    {
        public static ImapClient CreateClient()
        {
            ProtocolLogger protocolLogger = new(Console.OpenStandardError());

            return new(protocolLogger);
        }

        public static async Task<ImapClient> SignInAsync(this ImapClient client)
        {
            NetworkCredential credentials = new("rokossokal@ukr.net", "m09Yn4xIAY5fJqDu");
            Uri uri = new("imaps://imap.ukr.net");
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