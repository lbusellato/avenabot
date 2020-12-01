using avenabot.DAL;
using avenabot.Models.Partecipanti;
using Telegram.Bot.Args;
using System.Linq;

namespace avenabot.Interpreter
{
    public class Interpreter
    {
        private static PartecipantiDbContext db = new PartecipantiDbContext();

        public string[] commandList = new string[]
        {
            "/start",
            "/help", //Show the list off commands
            "/partecipanti", //Show the list of users registered
            "/iscrivimi", //Register the user in the Partecipanti db 
        };

        public string[] commandDescr = new string[]
        {
            "",
            "Mostra la lista di comandi disponibili",
            "Mostra la lista di partecipanti attualmente iscritti",
            "Iscrive l'ID Lichess specificato al torneo\nUtilizzo: /iscrivimi IDLichess",
        };

        public Interpreter() { }

        public string Parse(MessageEventArgs e)
        {
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            string res = "";
            switch (Find(message))
            {
                case 0:
                    res = "Ciao! Sono il Bot che gestisce il Torneo Avenoni Scacchisti.\n" +
                          "Usa /help per visualizzare i comandi disponibili";
                    break;
                case 1:
                    for(int i = 1; i < commandList.Length; ++i)
                    {
                        res += commandList[i] + "\n\n" + commandDescr[i] + "\n\n";
                    }
                    break;
                case 2:
                    res += "Elenco partecipanti:\nID Lichess - ID Telegram\n";
                    foreach (Partecipante p in db.Partecipanti)
                    {
                        res += p.LichessID + " - @" + p.TGID + "\n";
                    }
                    break;
                case 3:
                    if(message.Length == commandList[2].Length)
                    {
                        res = "Utilizzo: /iscrivimi IDLichess";
                    } else
                    {
                        string lichessID = message.Replace(commandList[2], string.Empty);
                        if (db.Partecipanti.FirstOrDefault(p => p.TGID == sender) == null)
                        {
                            Partecipante p = new Partecipante
                            {
                                LichessID = lichessID,
                                TGID = sender
                            };
                            db.Partecipanti.Add(p);
                            db.SaveChanges();
                        } else
                        {
                            res = "Risulti già iscritto! Usa /partecipanti per vedere la lista degli iscritti.\nSe credi sia un errore contatta @lbusellato";
                        }
                    }
                    break;
                case -1:
                default:
                    res = "Il comando specificato non esiste!";
                    break;
            }
            return res;
        }

        /// <summary>
        /// Finds the index of the provided command in the array commandList
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private int Find(string command)
        {
            for(int i = 0; i < commandList.Length; ++i)
            {
                if (command.IndexOf(commandList[i]) != -1)
                    return i;
            }
            return -1;
        }
    }
}
