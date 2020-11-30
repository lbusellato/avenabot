using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Awesome
{
    class Program
    {
        static ITelegramBotClient botClient;

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

                await botClient.SendTextMessageAsync(
                  chatId: e.Message.Chat,
                  text: "You said:\n" + e.Message.Text
                );
            }
        }
    }
}