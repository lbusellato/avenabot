using avenabot.Interpreter;
using avenabot.Log;
using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

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
                Console.WriteLine($"Received a text message from {e.Message.From.Username} in chat {e.Message.Chat.Id}: {e.Message.Text}");
                Logger.Log($"Received a text message from {e.Message.From.Username} in chat {e.Message.Chat.Id}: {e.Message.Text}");
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
                        await botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat,
                            text: res,
                            parseMode: ParseMode.Html,
                            true
                        );
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
                Console.WriteLine($"Ignored an empty text message from {e.Message.From.Username} in chat {e.Message.Chat.Id}");
                Logger.Log($"Ignored an empty text message from {e.Message.From.Username} in chat {e.Message.Chat.Id}");
            }
        }
    }
}