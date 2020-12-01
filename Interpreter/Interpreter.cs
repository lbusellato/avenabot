using avenabot.DAL;
using avenabot.Models.Partecipanti;
using Telegram.Bot.Args;
using System.Linq;
using System.Net;
using System;

namespace avenabot.Interpreter
{
    public class Interpreter
    {
        private static PartecipantiDbContext db = new PartecipantiDbContext();

        public string[] admin = new string[]
        {
            "lbusellato"
        };

        public string[] commandList = new string[]
        {
            "/start",
            "/help", //Show the list off commands
            "/partecipanti", //Show the list of users registered
            "/iscrivimi", //Register the user in the Partecipanti db 
            "/rimuovi", //Remove the specified player, admin only
            "/disiscrivimi", //Remove the specified player, only if the tg id is the same as the sender
        };

        public string[] commandDescr = new string[]
        {
            "",
            "Mostra la lista di comandi disponibili",
            "Mostra la lista di partecipanti attualmente iscritti",
            "Iscrive l'ID Lichess specificato al torneo\nUtilizzo: /iscrivimi IDLichess",
            "",
            "Disicrive dal torneo",
        };

        public Interpreter() { }

        public string Parse(MessageEventArgs e)
        {
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            string res = "";
            string lichessID = "";
            int elo = -1;
            switch (Find(message))
            {
                case 0: // /start
                    res = "Ciao! Sono il Bot che gestisce il Torneo Avenoni Scacchisti.\n" +
                          "Usa /help per visualizzare i comandi disponibili";
                    break;
                case 1: // /help
                    for(int i = 1; i < commandList.Length; ++i)
                    {
                        if (commandDescr[i] != "")
                        {
                            res += commandList[i] + "\n\n" + commandDescr[i] + "\n\n";
                        }
                    }
                    break;
                case 2: // /partecipanti
                    res += "Elenco partecipanti:\nID Lichess - ID Telegram - ELO\n";
                    foreach (Partecipante p in db.Partecipanti)
                    {
                        res += p.LichessID + " - @" + p.TGID + " - " + p.ELO + "\n";
                    }
                    break;
                case 3: // /iscrivimi
                    lichessID = message.Replace(commandList[Find("/iscrivimi")], string.Empty).Trim();
                    elo = GetELO(lichessID);
                    if (lichessID == "")
                    {
                        res = "Utilizzo: /iscrivimi IDLichess";
                    } 
                    else if(elo != -1)
                    {
                        if (db.Partecipanti.SingleOrDefault(p => p.TGID == sender) == null)
                        {
                            Partecipante p = new Partecipante
                            {
                                LichessID = lichessID,
                                TGID = sender,
                                ELO = elo
                            };
                            db.Partecipanti.Add(p);
                            db.SaveChanges();
                            res = "Ti ho iscritto! Usa /partecipanti per vedere la lista degli iscritti.";
                        } else
                        {
                            res = "Risulti già iscritto! Usa /partecipanti per vedere la lista degli iscritti.\nSe credi sia un errore contatta @lbusellato";
                        }
                    }
                    else
                    {
                        res = "L'ID Lichess che hai inserito non sembra esistere, controlla di averlo scritto giusto. Se credi sia un errore contatta @lbusellato";
                    }
                    break;
                case 4: // /rimuovi
                    lichessID = message.Replace(commandList[Find("/rimuovi")], string.Empty).Trim();
                    elo = GetELO(lichessID);
                    if (lichessID == "")
                    {
                        res = "Utilizzo: /rimuovi IDLichess";
                    }
                    else if (elo != -1)
                    {
                        if (db.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower()) != null)
                        {
                            Partecipante p = db.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower());
                            db.Partecipanti.Attach(p);
                            db.Partecipanti.Remove(p);
                            db.SaveChanges();
                            res = "Giocatore rimosso! Usa /partecipanti per vedere la lista degli iscritti.";
                        }
                        else
                        {
                            res = "Il giocatore indicato non sembra essere iscritto! Usa /partecipanti per vedere la lista degli iscritti.\nSe credi sia un errore contatta @lbusellato";
                        }
                    }
                    else
                    {
                        res = "L'ID Lichess che hai inserito non sembra esistere, controlla di averlo scritto giusto. Se credi sia un errore contatta @lbusellato";
                    }
                    break;
                case 5: // /disiscrivimi
                    if (db.Partecipanti.SingleOrDefault(p => p.TGID == sender) != null)
                    {
                        Partecipante p = db.Partecipanti.SingleOrDefault(p => p.TGID == sender);
                        db.Partecipanti.Attach(p);
                        db.Partecipanti.Remove(p);
                        db.SaveChanges();
                        res = "Ti ho rimosso! Usa /iscrivimi per riiscriverti.";
                    }
                    else
                    {
                        res = "Non mi sembra che tu sia iscritto! Usa /iscrivimi per iscriverti.\nSe credi sia un errore contatta @lbusellato";
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

        /// <summary>
        /// Pulls the provided player's rpaid ELO from Lichess
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int GetELO(string player)
        {
            WebClient client = new WebClient();
            try
            {
                string downloadString = client.DownloadString("https://lichess.org/@/" + player.ToLower() + "/perf/rapid");
                int i = downloadString.IndexOf("Rating: <strong>");
                return Int32.Parse(downloadString.Substring(i + 16, 4));
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine("\nIscrizione fallita: ID Lichess non trovato");
                return -1;
            }   
        }

        private bool isAdmin(string username)
        {
            bool res = false;
            for(int i = 0; i < admin.Length; ++i)
            {
                if(username == admin[i])
                {
                    res = true;
                }
            }
            return res;
        }
    }
}