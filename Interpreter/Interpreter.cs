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
        private readonly static PartecipantiDbContext partecipantiDb = new PartecipantiDbContext();
        private readonly static GironeADbContext gironeADb = new GironeADbContext();

        public string[] admin = new string[]
        {
            "lbusellato"
        };

        public bool[] adminCommands = new bool[]
        {
            false,
            false,
            false,
            false,
            true,
            false,
            true,
            true,
            false,
            false,
        };

        public string[] commandList = new string[]
        {
            "/start",
            "/help", //Show the list off commands
            "/partecipanti", //Show the list of users registered
            "/iscrivimi", //Register the user in the Partecipanti db 
            "/rimuovi", //Remove the specified player, admin only
            "/disiscrivimi", //Remove the specified player, only if the tg id is the same as the sender
            "/aggiungi", //Register a player, admin only
            "/seed", //Seed the group
            "/risultati", //Show group games results
            "/classifica", //Show group standings
        };

        public string[] commandDescr = new string[]
        {
            "",
            "Mostra la lista di comandi disponibili",
            "Mostra la lista di partecipanti attualmente iscritti",
            "Iscrive l'ID Lichess specificato al torneo\nUtilizzo: /iscrivimi IDLichess",
            "Rimuove il giocatore specificato\nUtilizzo: /rimuovi IDLichess",
            "Disicrive dal torneo",
            "Aggiunge il giocatore specificato\nUtilizzo: /aggiungi IDLichess IDTelegram",
            "Popola il girone\nUtilizzo: /seed IDGirone(A o B) n°partecipanti",
            "Mostra i risultati delle partite.\nUtilizzo:\n/risultati\nMostra i risultati di entrambi i gironi.\n/risultati IDGirone (A o B)\nMostra i risultati del girone specificato.\n/risultati IDLichess\nMostra i risultati del giocatore specificato.",
            "Mostra le classifiche.\nUtilizzo:\n/classifica\nMostra le classifiche di entrambi i gironi.\n/classifica IDGirone (A o B)\nMostra la classifica del girone specificato.",
        };

        public Interpreter() { }

        public string Parse(MessageEventArgs e)
        {

            DateTime closingDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to close registering
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            string res = "";
            string lichessID = "";
            string tgID = "";
            string[] subs;
            int elo = -1;
            int playerCount = GetPlayerCount();
            int MaxPlayers = 24; // Change this to edit how many players you want
            switch (Find(message))
            {
                case 0: // /start
                    res = "Ciao! Sono il Bot che gestisce il Torneo Avenoni Scacchisti.\n" +
                          "Usa /help per visualizzare i comandi disponibili";
                    break;
                case 1: // /help
                    for(int i = 1; i < commandList.Length; ++i)
                    {
                        if (adminCommands[i])
                        {
                            if(IsAdmin(sender))
                            {
                                res += commandList[i] + "\n\n" + commandDescr[i] + "\n\n";
                            }
                        }
                        else
                        {
                            res += commandList[i] + "\n\n" + commandDescr[i] + "\n\n";
                        }
                        for(int j = 0; j < 54; ++j)
                        {
                            res += '-';
                        }
                        res += '\n';
                    }
                    break;
                case 2: // /partecipanti
                    res += "Elenco partecipanti:\nID Torneo - ID Lichess - ID Telegram - ELO\n";
                    foreach (Partecipante p in partecipantiDb.Partecipanti)
                    {
                        res += p.ID + "-" + p.LichessID + " - @" + p.TGID + " - " + p.ELO + "\n";
                    }
                    break;
                case 3: // /iscrivimi
                    if (DateTime.Now <= closingDate && playerCount < MaxPlayers)
                    {
                        lichessID = message.Replace(commandList[Find("/iscrivimi")], string.Empty).Trim();
                        elo = GetELO(lichessID);
                        if (lichessID == "")
                        {
                            res = "Utilizzo: /iscrivimi IDLichess";
                        }
                        else if (elo != -1)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == sender) == null)
                            {
                                Partecipante p = new Partecipante
                                {
                                    LichessID = lichessID,
                                    TGID = sender,
                                    ELO = elo
                                };
                                partecipantiDb.Partecipanti.Add(p);
                                partecipantiDb.SaveChanges();
                                res = "Ti ho iscritto! Usa /partecipanti per vedere la lista degli iscritti.";
                            }
                            else
                            {
                                res = "Risulti già iscritto! Usa /partecipanti per vedere la lista degli iscritti.\nSe credi sia un errore contatta @lbusellato";
                            }
                        }
                        else
                        {
                            res = "L'ID Lichess che hai inserito non sembra esistere, controlla di averlo scritto giusto. Se credi sia un errore contatta @lbusellato";
                        }
                    }
                    else
                    {
                        res = "Spiacente, le iscrizioni sono chiuse!";
                    }
                    break;
                case 4: // /rimuovi
                    if (IsAdmin(sender))
                    {
                        lichessID = message.Replace(commandList[Find("/rimuovi")], string.Empty).Trim();
                        elo = GetELO(lichessID);
                        if (lichessID == "")
                        {
                            res = "Utilizzo: /rimuovi IDLichess";
                        }
                        else if (elo != -1)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower()) != null)
                            {
                                Partecipante p = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower());
                                partecipantiDb.Partecipanti.Attach(p);
                                partecipantiDb.Partecipanti.Remove(p);
                                partecipantiDb.SaveChanges();
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
                    }
                    break;
                case 5: // /disiscrivimi
                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == sender) != null)
                    {
                        Partecipante p = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == sender);
                        partecipantiDb.Partecipanti.Attach(p);
                        partecipantiDb.Partecipanti.Remove(p);
                        partecipantiDb.SaveChanges();
                        res = "Ti ho rimosso! Usa /iscrivimi per riiscriverti.";
                    }
                    else
                    {
                        res = "Non mi sembra che tu sia iscritto! Usa /iscrivimi per iscriverti.\nSe credi sia un errore contatta @lbusellato";
                    }
                    break;
                case 6: //aggiungi
                    if (IsAdmin(sender))
                    {
                        if (message.Length == commandList[6].Length)
                        {
                            res = "Utilizzo: /aggiungi IDLichess IDTelegram";
                        }
                        else
                        {
                            subs = message.Split(' ');
                            if(subs.Length != 3)
                            {
                                res = "Utilizzo: /aggiungi IDLichess IDTelegram";
                            }
                            else
                            {
                                lichessID = subs[1];
                                tgID = subs[2];
                                elo = GetELO(lichessID);
                                if (elo != -1)
                                {
                                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == tgID) == null)
                                    {
                                        Partecipante p = new Partecipante
                                        {
                                            LichessID = lichessID,
                                            TGID = tgID,
                                            ELO = elo
                                        };
                                        partecipantiDb.Partecipanti.Add(p);
                                        partecipantiDb.SaveChanges();
                                        res = "Giocatore iscritto! Usa /partecipanti per vedere la lista degli iscritti.";
                                    }
                                    else
                                    {
                                        res = "Il giocatore risulta già iscritto! Usa /partecipanti per vedere la lista degli iscritti.\nSe credi sia un errore contatta @lbusellato";
                                    }
                                }
                                else
                                {
                                    res = "L'ID Lichess che hai inserito non sembra esistere, controlla di averlo scritto giusto. Se credi sia un errore contatta @lbusellato";
                                }
                                break;
                            }
                        }
                    }
                    break;
                case 7: // /seed
                    break;
                case 8: // /risultati
                    subs = message.Split(' ');
                    if(subs.Length == 1) //Both groups
                    {
                        //TODO
                    }
                    else if(subs.Length == 2)
                    {
                        if(subs[1] == "A" || subs[1] == "B") //One group
                        {
                            //TODO
                        }
                        else //One player
                        {
                            string submittedID = subs[1];
                            int playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).ID;
                            if(playerID != null)
                            {
                                string results = gironeADb.GironeA.SingleOrDefault(p => p.PlayerID == playerID).Results;
                                string[] subresults = results.Split(',');
                                for(int i = 0; i < subresults.Length; ++i)
                                {
                                    //TODO Fetch lichess ID from player ID
                                    res += (i + 1) + "\t - \t" + subresults[i] + '\n';
                                }
                            }
                            else
                            {
                                res = "Non ho trovato il giocatore su Lichess, controlla di aver scritto correttamente l'ID.";
                            }
                        }
                    }
                    break;
                case 9: // /classifica
                    break;
                case -1:
                default:
                    res = "Non ho capito, usa /help per vedere la lista dei comandi disponibili.";
                    break;
            }
            return res;
        }

        /// <summary>
        /// Counts all the players registered
        /// </summary>
        /// <returns></returns>
        private int GetPlayerCount()
        {
            int res = 0;
            foreach (Partecipante p in partecipantiDb.Partecipanti)
            {
                res++;
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
                Console.WriteLine("\nIscrizione fallita: ID Lichess non trovato\nEccezione: " + e);
                return -1;
            }   
        }

        private bool IsAdmin(string username)
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