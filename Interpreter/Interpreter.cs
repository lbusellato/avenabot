using avenabot.DAL;
using avenabot.Models.Partecipanti;
using Telegram.Bot.Args;
using System.Linq;
using System.Net;
using System;
using avenabot.Models.Gironi;
using System.Collections.Generic;

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
            Strings.startCommand,
            Strings.helpCommand, //Show the list off commands
            Strings.partecipantiCommand, //Show the list of users registered
            Strings.iscrivimiCommand, //Register the user in the Partecipanti db
            Strings.rimuoviCommand, //Remove the specified player, admin only
            Strings.disiscrivimiCommand, //Remove the specified player, only if the tg id is the same as the sender
            Strings.aggiungiCommand, //Register a player, admin only
            Strings.seedCommand, //Seed the group
            Strings.risultatiCommand, //Show group games results
            Strings.classificaCommand, //Show group standings
        };

        public string[] commandDescr = new string[]
        {
            Strings.startDescr,
            Strings.helpDescr,
            Strings.partecipantiDescr,
            Strings.iscrivimiDescr,
            Strings.rimuoviDescr,
            Strings.disiscrivimiDescr,
            Strings.aggiungiDescr,
            Strings.seedDescr,
            Strings.risultatiDescr,
            Strings.classificaDescr,
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
            int maxLen = 0;
            int elo = -1;
            int playerCount = GetPlayerCount();
            int MaxPlayers = 24; // Change this to edit how many players you want
            switch (Find(message))
            {
                case 0: // /start
                    res = Strings.welcomeMsg;
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
                    res += Strings.partecipantiHeader;
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
                            res = Strings.iscrivimiUsage;
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
                                res = Strings.registered + Strings.checkPartecipanti;
                            }
                            else
                            {
                                res = Strings.registeredError + Strings.checkPartecipanti + Strings.errorContact;
                            }
                        }
                        else
                        {
                            res = Strings.lichess404 + Strings.errorContact; 
                        }
                    }
                    else
                    {
                        res = Strings.closedRegistrations;
                    }
                    break;
                case 4: // /rimuovi
                    if (IsAdmin(sender))
                    {
                        lichessID = message.Replace(commandList[Find("/rimuovi")], string.Empty).Trim();
                        elo = GetELO(lichessID);
                        if (lichessID == "")
                        {
                            res = Strings.rimuoviUsage;
                        }
                        else if (elo != -1)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower()) != null)
                            {
                                Partecipante p = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower());
                                partecipantiDb.Partecipanti.Attach(p);
                                partecipantiDb.Partecipanti.Remove(p);
                                partecipantiDb.SaveChanges();
                                res = Strings.removedAdmin + Strings.checkPartecipanti;
                            }
                            else
                            {
                                res = Strings.player404 + Strings.checkPartecipanti + Strings.errorContact;
                            }
                        }
                        else
                        {
                            res = Strings.lichess404 + Strings.errorContact;
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
                        res = Strings.removed + Strings.iscrivimiUsage2;
                    }
                    else
                    {
                        res = Strings.notRegistered + Strings.iscrivimiUsage + Strings.errorContact;
                    }
                    break;
                case 6: //aggiungi
                    if (IsAdmin(sender))
                    {
                        if (message.Length == commandList[6].Length)
                        {
                            res = Strings.aggiungiUsage;
                        }
                        else
                        {
                            subs = message.Split(' ');
                            if(subs.Length != 3)
                            {
                                res = Strings.aggiungiUsage;
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
                                        res = Strings.registeredAdmin + Strings.checkPartecipanti;
                                    }
                                    else
                                    {
                                        res = Strings.registeredAdminError + Strings.checkPartecipanti + Strings.errorContact;
                                    }
                                }
                                else
                                {
                                    res = Strings.lichess404 + Strings.errorContact;
                                }
                                break;
                            }
                        }
                    }
                    break;
                case 7: // /seed
                    Girone check = gironeADb.GironeA.SingleOrDefault(g => g.ID == 1);
                    if (check == null)
                    {
                        //Pull the list of 12 lowest elo players and populate the group db
                        List<Partecipante> players = new List<Partecipante>();
                        foreach (Partecipante p in partecipantiDb.Partecipanti)
                        {
                            players.Add(p);
                        }
                        players = players.OrderBy(p1 => p1.ELO).ToList();
                        //Cull the players after the no 12
                        while (players.Count > 12)
                        {
                            players.RemoveAt(players.Count - 1);
                        }
                        //Push the list to the db
                        foreach (Partecipante p in players)
                        {
                            Girone g = new Girone
                            {
                                PlayerID = p.ID,
                                Results = "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,"
                            };
                            gironeADb.GironeA.Add(g);
                            gironeADb.SaveChanges();
                        }
                        res += Strings.groupSeeded;
                    }
                    else
                    {
                        res += Strings.groupAlreadySeeded;
                    }
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
                            //TODO differentiate between groups
                            string submittedID = subs[1];
                            int? playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).ID;
                            Girone gir = gironeADb.GironeA.SingleOrDefault(g => g.ID == 1); //Used to check if the group has players
                            string opponent;
                            if(playerID != null && gir != null)
                            {
                                //Retrieve the lichess id with proper capitalization
                                submittedID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == playerID).LichessID; 
                                //Retrieve the player results, split them in an array
                                string results = gironeADb.GironeA.SingleOrDefault(p => p.PlayerID == playerID).Results;
                                string[] subresults = results.Split(',');

                                res += "<pre>";
                                res += submittedID + "\n";

                                //Find out the longest opponents name to nicely format the results
                                maxLen = 0;

                                foreach(Girone g in gironeADb.GironeA)
                                {
                                    if(partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                                    {
                                        maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                                    }
                                }

                                //Format opponents names and results
                                for (int i = 0; i < subresults.Length; ++i)
                                {
                                    int opponentID = gironeADb.GironeA.SingleOrDefault(g1 => g1.ID == i + 1).PlayerID;
                                    opponent = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == opponentID).LichessID;
                                    if (opponent != submittedID)
                                    {
                                        res += "vs " + opponent;

                                        for (int j = 0; j < maxLen - opponent.Length + 1; ++j)
                                        {
                                            res += "&#32";
                                        }
                                        if (subresults[i] == "0.5")
                                        {
                                            res += " &#189 \n";
                                        }
                                        else if (subresults[i] == "-1")
                                        {
                                            res += " -\n";
                                        }
                                        else
                                        {
                                            res += " " + subresults[i] + "\n";
                                        }
                                    }
                                }
                                res += "</pre>";
                            }
                            else
                            {
                                res = Strings.player404 + Strings.checkPartecipanti + Strings.errorContact;
                            }
                        }
                    }
                    break;
                case 9: // /classifica
                    break;
                case -1:
                default:
                    res = Strings.saywhat;
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
                int elo;

                if (int.TryParse(downloadString.Substring(i + 16, 4), out elo))
                {
                    return elo;
                }
                else
                {
                    return 0;
                }
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