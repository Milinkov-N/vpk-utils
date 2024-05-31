using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace VpkUtils.Utility;

public class EmailRepository : IDisposable
{
    public ImapClient Client { get; set; } = new();

    public EmailRepository(string login, string appPassword)
    {
        Client.Connect("imap.gmail.com", 993, true);
        Client.Authenticate(login, appPassword);
    }

    public List<MimeMessage> GetEmails()
    {
        var messages = new List<MimeMessage>();

        Client.Inbox.Open(FolderAccess.ReadOnly);

        var belovaUids = Client.Inbox.Search(SearchQuery.FromContains("noreply@hh.ru"));
        var todayUids = Client.Inbox.Search(SearchQuery.SentSince(DateTime.Now - TimeSpan.FromDays(1)));
        var unseenUids = Client.Inbox.Search(SearchQuery.NotSeen);

        foreach (var uid in unseenUids
            .Where(uid => todayUids.Contains(uid))
            .Where(uid => belovaUids.Contains(uid)))
        {
            var msg = Client.Inbox.GetMessage(uid);
            Console.WriteLine($"{msg.Subject} - {msg.Date}");
            // messages.Add(Client.Inbox.GetMessage(uid));

        }

        return messages;
    }

    public void Dispose() => Client.Disconnect(true);
}
