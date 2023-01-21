using Aspose.Email.Clients.Imap;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class UkrNetParsingBackgroundService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ImapClient client = new ImapClient("imap.ukr.net", "rokossokal@ukr.net", "dvQMK1gjCeR6X41Q");



            // Select folder
            client.SelectFolder("Inbox");

            //client.getm
            return Task.CompletedTask;
        }
    }
}
