using avenabot.Interpreter;
using avenabot.Log;
using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using avenabot.DAL;
using avenabot.Models.Chat;
using CoreHtmlToImage;
using System.IO;
using Telegram.Bot.Types.InputFiles;

namespace Awesome
{
    class Program
    {
        static ITelegramBotClient botClient;
        public static DateTime lastCommand;
        public static Interpreter interpreter = new Interpreter();

        static void Main()
        {
            lastCommand = DateTime.Now.AddMinutes(-5);
            botClient = new TelegramBotClient("1444146870:AAG22lLxZqCxi7s21rXC5Co4Na6hNL6DDkA");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );
            Logger.Log("Starting up...");
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit"); 
            Console.ReadKey();

            Logger.Log("Shutting down...");
            Logger.Dispose();
            botClient.StopReceiving();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Logger.Log($"Received a text message from {e.Message.From.Username} in chat {e.Message.Chat.Id}: {e.Message.Text}");
                ManageChatID(e.Message.Chat.Id);
                string res;
                if(e.Message.Text.ToLower().IndexOf(Strings.invalidMessage) != -1)
                {
                    res = Strings.errorInvalidMessage;
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: res,
                        parseMode: ParseMode.Html,
                        true,
                        false,
                        e.Message.MessageId
                    );
                }
                else
                {
                    res = interpreter.Parse(e, lastCommand);
                    if (res != "")
                    {
                        string dir = Directory.GetCurrentDirectory();
                        if (res == "@" + e.Message.From.Username + "\n" + "classifica")
                        {
                            var fs = new FileStream(dir + "\\classifica.png", FileMode.Open, FileAccess.Read);
                            var file = new InputOnlineFile(fs);
                            await botClient.SendPhotoAsync(
                                chatId: e.Message.Chat,
                                photo: file,
                                caption: "@" + e.Message.From.Username
                                );
                            fs.Close();
                        }
                        else if (res == "@" + e.Message.From.Username + "\n" + "gironi")
                        {
                            var fs = new FileStream(dir + "\\girone.png", FileMode.Open, FileAccess.Read);
                            var file = new InputOnlineFile(fs);
                            await botClient.SendPhotoAsync(
                                chatId: e.Message.Chat,
                                photo: file,
                                caption: "@" + e.Message.From.Username
                                );
                            fs.Close();
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: res,
                                parseMode: ParseMode.Html,
                                true
                            );
                        }
                        Logger.Log($"Responded to command.");
                    }
                    else
                    {
                        Logger.Log($"Ignored message (not a command).");
                    }
                }
            }
            else
            {
                Logger.Log($"Ignored an empty text message from {e.Message.From.Username} in chat {e.Message.Chat.Id}");
            }
        }

        static void ManageChatID(long ChatID)
        {
            bool flag = true;
            using PartecipantiDbContext db = new PartecipantiDbContext();
            foreach (Chat c in db.Chats)
            {
                if (c.ChatID == ChatID)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                Logger.Log($"New chat id: {ChatID}, I'll register it.");
                Chat c = new Chat()
                {
                    ChatID = ChatID
                };
                db.Chats.Add(c);
                db.SaveChanges();
            }
        }
    }
}