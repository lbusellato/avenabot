using avenabot.DAL;
using avenabot.Interpreter;
using avenabot.Models.Partecipanti;
using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace Awesome
{
    class Program
    {
        static ITelegramBotClient botClient;

        public static Interpreter interpreter = new Interpreter();

        static void Main()
        {
            botClient = new TelegramBotClient("1444146870:AAG22lLxZqCxi7s21rXC5Co4Na6hNL6DDkA");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            botClient.StopReceiving();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
                string res = interpreter.Parse(e);
                if (res != "")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: res,
                        parseMode: ParseMode.Html,
                        true
                    );
                }
            }
        }
    }
}