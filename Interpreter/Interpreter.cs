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
        private static PartecipantiDbContext partecipantiDb = new PartecipantiDbContext();
        private static GironeADbContext gironeADb = new GironeADbContext();

        public string[] admin = new string[]
        {
            "lbujusellato"
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
            Strings.inserisciCommand, //Insert a game result
            Strings.torneoCommand, //Show tournament info
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
            Strings.inserisciDescr,
            Strings.torneoDescr,
        };

        public Interpreter() { }

        public struct Standing
        {
            public string ID { get; set; }
            public string[] Games { get; set; }
            public double Tot { get; set; }
        }

        public string Parse(MessageEventArgs e)
        {
            partecipantiDb = new PartecipantiDbContext();
            gironeADb = new GironeADbContext();
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            DateTime closingDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to close registering
            int MaxPlayers = 24;
            string res = (Find(message)) switch
            {
                // /start
                0 => startCommand(),
                // /help
                1 => helpCommand(sender),
                // /partecipanti
                2 => partecipantiCommand(),
                // /iscrivimi
                3 => iscrivimiCommand(closingDate, MaxPlayers, message, sender),
                // /rimuovi
                4 => rimuoviCommand(message, sender),
                // /disiscrivimi
                5 => disiscrivimiCommand(sender),
                //aggiungi
                6 => aggiungiCommand(message, sender),
                // /seed
                7 => seedCommand(MaxPlayers),
                // /risultati
                8 => risultatiCommand(message, MaxPlayers),
                // /classifica
                9 => classificaCommand(),
                // /inserisci
                10 => inserisciCommand(message),
                // /torneo
                11 => torneoCommand(),
                _ => noCommand(),
            };
            gironeADb.Dispose();
            partecipantiDb.Dispose();
            return res;
        }

        private string startCommand()
        {
            string res = "";
            res += Strings.welcomeMsg;
            return res;
        }

        private string helpCommand(string sender)
        {
            string res = "";
            for (int i = 1; i < commandList.Length; ++i)
            {
                if (adminCommands[i])
                {
                    if (IsAdmin(sender))
                    {
                        res += commandList[i] + "\n\n" + commandDescr[i] + "\n\n";
                        for (int j = 0; j < 54; ++j)
                        {
                            res += '-';
                        }
                    }
                }
                else
                {
                    res += commandList[i] + "\n\n" + commandDescr[i] + "\n\n";
                    for (int j = 0; j < 54; ++j)
                    {
                        res += '-';
                    }
                }
                res += '\n';
            }
            return res;
        }

        private string partecipantiCommand()
        {
            string res = "";
            res += Strings.partecipantiHeader;
            foreach (Partecipante p in partecipantiDb.Partecipanti)
            {
                res += p.ID + "-" + p.LichessID + " - @" + p.TGID + " - " + p.ELO + "\n";
            }
            return res;
        }

        private string iscrivimiCommand(DateTime closingDate, int MaxPlayers, string message, string sender)
        {
            string res = "";
            string lichessID = "";
            int playerCount = GetPlayerCount();
            int elo = -1;

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
            return res;
        }

        private string rimuoviCommand(string message, string sender)
        {
            string res = "";
            string lichessID = "";
            int elo = -1;
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
            return res;
        }

        private string disiscrivimiCommand(string sender)
        {
            string res = "";
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
            return res;
        }

        private string aggiungiCommand(string message, string sender)
        {
            string res = "";
            string lichessID = "";
            string tgID = "";
            string[] subs;
            int elo = -1;
            if (IsAdmin(sender))
            {
                if (message.Length == commandList[6].Length)
                {
                    res = Strings.aggiungiUsage;
                }
                else
                {
                    subs = message.Split(' ');
                    if (subs.Length != 3)
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
                    }
                }
            }
            return res;
        }

        private string seedCommand(int MaxPlayers)
        {
            string res = "";
            //TODO generalise seeding?
            //TODO seed group B and C
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
                //Cull the players after the no 8
                while (players.Count > MaxPlayers / 3)
                {
                    players.RemoveAt(players.Count - 1);
                }
                //Push the list to the db
                foreach (Partecipante p in players)
                {
                    Girone g = new Girone
                    {
                        PlayerID = p.ID,
                        Results = "-1,-1,-1,-1,-1,-1,-1,-1"
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
            return res;
        }

        private string risultatiCommand(string message, int MaxPlayers)
        {
            string res = "";
            string results = "";
            string[] subresults;
            string[] subs;
            int maxLen = 0;

            subs = message.Split(' ');
            if (subs.Length == 1) //All groups
            {
                Girone gir = gironeADb.GironeA.SingleOrDefault(g => g.ID == 1); //Used to check if the group has players
                if (gir != null)
                {
                    res += "<pre>Risultati girone A:\n&#32&#32 ";
                    for (int i = 0; i < MaxPlayers / 3; ++i)
                    {
                        res += gironeADb.GironeA.SingleOrDefault(g => g.ID == i + 1).PlayerID;
                        if (gironeADb.GironeA.SingleOrDefault(g => g.ID == i + 1).PlayerID < 10)
                        {
                            res += "&#32 ";
                        }
                        else
                        {
                            res += " ";
                        }
                    }
                    res += "\n";
                    foreach (Girone g in gironeADb.GironeA)
                    {
                        results = g.Results;
                        subresults = results.Split(',');
                        res += g.PlayerID;
                        if (g.PlayerID < 10)
                        {
                            res += "&#32";
                        }
                        for (int i = 0; i < subresults.Length; ++i)
                        {
                            if (subresults[i] == "x")
                            {
                                res += " &#189 ";
                            }
                            else if (subresults[i] == "-1")
                            {
                                res += " - ";
                            }
                            else
                            {
                                res += " " + subresults[i] + " ";
                            }
                        }
                        res += "\n";
                    }
                    res += "</pre>";
                }
                //TODO girone B, girone C
            }
            else if (subs.Length == 2)
            {
                if (subs[1] == "A" || subs[1] == "B" || subs[1] == "C") //One group
                {
                    if (subs[1] == "A")
                    {
                        Girone gir = gironeADb.GironeA.SingleOrDefault(g => g.ID == 1); //Used to check if the group has players
                        if (gir != null)
                        {
                            res += "<pre>&#32&#32 ";
                            for (int i = 0; i < MaxPlayers / 3; ++i)
                            {
                                res += gironeADb.GironeA.SingleOrDefault(g => g.ID == i + 1).PlayerID;
                                if (gironeADb.GironeA.SingleOrDefault(g => g.ID == i + 1).PlayerID < 10)
                                {
                                    res += "&#32 ";
                                }
                                else
                                {
                                    res += " ";
                                }
                            }
                            res += "\n";
                            foreach (Girone g in gironeADb.GironeA)
                            {
                                results = g.Results;
                                subresults = results.Split(',');
                                res += g.PlayerID;
                                if (g.PlayerID < 10)
                                {
                                    res += "&#32";
                                }
                                for (int i = 0; i < subresults.Length; ++i)
                                {
                                    if (subresults[i] == "x")
                                    {
                                        res += " &#189 ";
                                    }
                                    else if (subresults[i] == "-1")
                                    {
                                        res += " - ";
                                    }
                                    else
                                    {
                                        res += " " + subresults[i] + " ";
                                    }
                                }
                                res += "\n";
                            }
                            res += "</pre>";
                        }
                    }
                    else if (subs[1] == "B")
                    {
                        //TODO
                    }
                    else
                    {
                        //TODO
                    }
                }
                else //One player
                {
                    //TODO search the player in both dbs to find out wich one to use
                    string submittedID = subs[1];
                    int? playerID = null;
                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID) != null)
                    {
                        playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).ID;
                    }
                    Girone gir = gironeADb.GironeA.SingleOrDefault(g => g.ID == 1); //Used to check if the group has players
                    string opponent;
                    if (playerID != null && gir != null)
                    {
                        //Retrieve the lichess id with proper capitalization
                        submittedID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == playerID).LichessID;
                        //Retrieve the player results, split them in an array
                        results = gironeADb.GironeA.SingleOrDefault(p => p.PlayerID == playerID).Results;
                        subresults = results.Split(',');

                        res += "<pre>";
                        res += submittedID + "\n";

                        //Find out the longest opponents name to nicely format the results
                        maxLen = 0;

                        foreach (Girone g in gironeADb.GironeA)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
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
                                if (subresults[i] == "x")
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
                        res = Strings.risultatiUsage;
                    }
                }
            }
            return res;
        }

        private string classificaCommand()
        {
            string res = "";
            string[] subresults;
            //TODO standings group B and C

            //Find out the longest opponents name to nicely format the results
            int maxLen = 0;

            foreach (Girone g in gironeADb.GironeA)
            {
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                {
                    maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                }
            }

            List<Standing> standings = new List<Standing>();
            foreach (Girone g in gironeADb.GironeA)
            {
                Standing stg = new Standing();
                stg.ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID;
                subresults = g.Results.Split(",");
                stg.Games = new string[subresults.Length];
                stg.Tot = 0;
                for (int i = 0; i < subresults.Length; ++i)
                {
                    if (subresults[i] == "x")
                    {
                        stg.Games[i] = "&#189 ";
                        stg.Tot += 0.5;
                    }
                    else
                    {
                        if (subresults[i] == "1")
                        {
                            stg.Games[i] = subresults[i];
                            stg.Tot += 1;
                        }
                        else if (subresults[i] == "0")
                        {
                            stg.Games[i] = subresults[i];
                        }
                    }
                }
                standings.Add(stg);
            }
            standings.OrderBy(s => s.Tot);
            res += "<pre>Classifica Girone A:\nID";
            for (int i = 0; i < maxLen - 4; ++i)
            {
                res += " ";
            }
            res += "G: 1 2 3 4 5 6 7 P\n";
            foreach (Standing s in standings)
            {
                int gamesPlayed = 0;
                res += s.ID;
                for (int i = 0; i < maxLen - s.ID.Length; ++i)
                {
                    res += " ";
                }
                res += " ";
                for (int i = 0; i < s.Games.Length; ++i)
                {
                    if (s.Games[i] != null)
                    {
                        res += s.Games[i];
                        gamesPlayed++;
                    }
                }
                for (int i = 0; i < 13 - gamesPlayed; ++i)
                {
                    res += " ";
                }
                if (s.Tot == 0.5) //TODO Check decimal in general
                {
                    res += "&#189 \n";
                }
                else
                {
                    res += " " + s.Tot + "\n";
                }
            }
            res += "</pre>";
            return res;
        }

        private string inserisciCommand(string message)
        {
            string res = "";
            string helper = "";
            string player1LichessID;
            string player2LichessID;
            string prevResults;
            string results;
            string SQLCommand;
            string[] subresults;
            string[] subs;
            int player1ID;
            int player2ID;
            int player1GroupID;
            int player2GroupID;
            subs = message.Split(" ");
            if (subs.Length != 4)
            {
                res += Strings.inserisciUsage;
            }
            else
            {
                //TODO find which group the players are in

                //Valid lichess ids?
                string sub1 = subs[1];
                string sub2 = subs[2];
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower()) != null &&
                    partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower()) != null)
                {
                    //Valid result?
                    if (subs[3] == "1" || subs[3] == "2" || subs[3] == "x")
                    {
                        if (subs[3] == "1")
                        {
                            helper = "1";
                        }
                        else if (subs[3] == "2")
                        {
                            helper = "0";
                        }
                        else
                        {
                            helper = "x";
                        }

                        player1LichessID = subs[1];
                        player1LichessID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).LichessID;
                        player1ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).ID;
                        player1GroupID = gironeADb.GironeA.SingleOrDefault(g => g.PlayerID == player1ID).ID;

                        player2LichessID = subs[2];
                        player2LichessID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).LichessID;
                        player2ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).ID;
                        player2GroupID = gironeADb.GironeA.SingleOrDefault(g => g.PlayerID == player2ID).ID;


                        prevResults = gironeADb.GironeA.SingleOrDefault(g => g.PlayerID == player1ID).Results;
                        subresults = prevResults.Split(",");
                        subresults[player2GroupID - 1] = helper;
                        results = "";
                        for (int i = 0; i < subresults.Length; ++i)
                        {
                            results += subresults[i];
                            if (i != subresults.Length - 1)
                            {
                                results += ",";
                            }
                        }

                        SQLCommand = "UPDATE Girone SET Results='" + results + "' WHERE PlayerID=" + player1ID;
                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);

                        if (helper == "1")
                        {
                            helper = "0";
                        }
                        else if (helper == "0")
                        {
                            helper = "1";
                        }
                        else
                        {
                            helper = "&#189 ";
                        }

                        prevResults = gironeADb.GironeA.SingleOrDefault(g => g.PlayerID == player2ID).Results;
                        subresults = prevResults.Split(",");
                        subresults[player1GroupID - 1] = helper;
                        results = "";
                        for (int i = 0; i < subresults.Length; ++i)
                        {
                            results += subresults[i];
                            if (i != subresults.Length - 1)
                            {
                                results += ",";
                            }
                        }

                        SQLCommand = "UPDATE Girone SET Results='" + results + "' WHERE PlayerID=" + player2ID;
                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);

                        res += Strings.insertedResult + Strings.checkResults;
                    }
                    else
                    {
                        res += Strings.inserisciInvalidResult;
                    }
                }
                else
                {
                    res += Strings.inserisciInvalidIDs;
                }
            }
            return res;
        }

        private string torneoCommand()
        {
            string res = "";
            res += Strings.torneoInfo;
            return res;
        }

        private string noCommand()
        {
            string res = "";
            res += Strings.saywhat;
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
            catch (WebException e)
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