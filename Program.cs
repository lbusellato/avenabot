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
                string res = "";
                if(e.Message.Text.ToLower().IndexOf("a38") != -1)
                {
                    res = "Bravo bravo porcodio ma vai a fare in culo";
                }
                else
                {
                    res = interpreter.Parse(e, lastCommand);
                }
                if (res != "")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: res,
                        parseMode: ParseMode.Html,
                        true
                    );
                }
                lastCommand = DateTime.Now;
            }
            else
            {
                Console.WriteLine($"Ignored (cooldown) a text message in chat {e.Message.Chat.Id}.");
            }
        }
    }
}