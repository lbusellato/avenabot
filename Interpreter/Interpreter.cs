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
        //TODO: Migrate from using identity column as players/group IDs to implement dynamic player management in groups
        private static PartecipantiDbContext partecipantiDb = new PartecipantiDbContext();
        private static GironeADbContext gironeADb = new GironeADbContext();
        private static GironeBDbContext gironeBDb = new GironeBDbContext();
        private static GironeCDbContext gironeCDb = new GironeCDbContext();

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
            gironeBDb = new GironeBDbContext();
            gironeCDb = new GironeCDbContext();
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            DateTime closingDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to close registering
            int MaxPlayers = 12; //Only multiples of 3 allowed!!!!!!
            string res = (Find(message)) switch
            {
                // /start
                0 => StartCommand(),
                // /help
                1 => HelpCommand(sender),
                // /partecipanti
                2 => PartecipantiCommand(),
                // /iscrivimi
                3 => IscrivimiCommand(closingDate, MaxPlayers, message, sender),
                // /rimuovi
                4 => RimuoviCommand(message, sender),
                // /disiscrivimi
                5 => DisiscrivimiCommand(sender),
                // /aggiungi
                6 => AggiungiCommand(message, sender),
                // /seed
                7 => SeedCommand(MaxPlayers),
                // /risultati
                8 => RisultatiCommand(message, MaxPlayers),
                // /classifica
                9 => ClassificaCommand(MaxPlayers),
                // /inserisci
                10 => InserisciCommand(message),
                // /torneo
                11 => TorneoCommand(),
                _ => NoCommand(),
            };
            gironeADb.Dispose();
            gironeBDb.Dispose();
            gironeCDb.Dispose();
            partecipantiDb.Dispose();
            return res;
        }

        private string StartCommand()
        {
            string res = "";
            res += Strings.welcomeMsg;
            return res;
        }

        /// <summary>
        /// Shows the list of commands available to the user
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string HelpCommand(string sender)
        {
            //FINAL
            string res = "";
            for (int i = 1; i < commandList.Length; ++i)
            {
                if (adminCommands[i])
                {
                    if (IsAdmin(sender))
                    {
                        res += commandList[i] + "\n" + commandDescr[i] + "\n";
                        for (int j = 0; j < 54; ++j)
                        {
                            res += '-';
                        }
                        res += '\n';
                    }
                }
                else
                {
                    res += commandList[i] + "\n" + commandDescr[i] + "\n";
                    for (int j = 0; j < 54; ++j)
                    {
                        res += '-';
                    }
                    res += '\n';
                }
            }
            return res;
        }

        /// <summary>
        /// Fetches the list of registered players from the DB and shows it
        /// </summary>
        /// <returns></returns>
        private string PartecipantiCommand()
        {
            //FINAL
            string res = "";
            res += Strings.partecipantiHeader;
            foreach (Partecipante p in partecipantiDb.Partecipanti)
            {
                res += p.ID + "-" + p.LichessID + " - @" + p.TGID + " - " + p.ELO + " - " + p.Girone + "\n";
            }
            return res;
        }

        /// <summary>
        /// Registers the player with the Lichess ID provided in message, checking if registration closing time or max player 
        /// number have been reached
        /// </summary>
        /// <param name="closingDate"></param>
        /// <param name="MaxPlayers"></param>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string IscrivimiCommand(DateTime closingDate, int MaxPlayers, string message, string sender)
        {
            //FINAL
            string res = "";
            string lichessID = "";
            int playerCount = GetPlayerCount();
            int elo = -1;

            if (DateTime.Now <= closingDate && playerCount < MaxPlayers)
            {
                //Get the Lichess ID
                lichessID = message.Replace(commandList[Find("/iscrivimi")], string.Empty).Trim();
                //Fetch the player's ELO from Lichess
                elo = GetELO(lichessID);
                if (lichessID == "") //If no ID was provided
                {
                    res = Strings.iscrivimiUsage;
                }
                else if (elo != -1)
                {
                    //Search the DB for the player, create him if he's not found
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

        /// <summary>
        /// Removes the player with the corresponding Lichess ID, if he exists in the DB (admin only)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string RimuoviCommand(string message, string sender)
        {
            //FINAL
            string res = "";
            string lichessID = "";
            int elo = -1;
            if (IsAdmin(sender))
            {
                //Get the Lichess ID
                lichessID = message.Replace(commandList[Find("/rimuovi")], string.Empty).Trim();
                //Fetch the player's ELO from Lichess
                elo = GetELO(lichessID);
                if (lichessID == "") //If no ID was provided
                {
                    res = Strings.rimuoviUsage;
                }
                else if (elo != -1)
                {
                    //Remove the player from the DB, if he exists
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

        /// <summary>
        /// Removes the player from the DB
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string DisiscrivimiCommand(string sender)
        {
            //FINAL
            string res = "";
            //Remove the player if he is in the DB
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

        /// <summary>
        /// Add the player with the specified Lichess ID and Telegram Handle (admin only)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string AggiungiCommand(string message, string sender)
        {
            //FINAL
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
                    if (subs.Length != 3) //If the number of arguments is not the correct one
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
                            //If the player is not in the DB, create him
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

        /// <summary>
        /// Populates the groups using ELO as a parameter
        /// </summary>
        /// <param name="MaxPlayers"></param>
        /// <param name="GroupNumber"></param>
        /// <returns></returns>
        private string SeedCommand(int MaxPlayers)
        {
            string res = "";
            if (MaxPlayers % 3 != 0)
            {
                res += Strings.internalError + 2;
                return res;
            }
            //Check if the DBs are empty
            Girone checkA = gironeADb.Girone.SingleOrDefault(g => g.ID == 1);
            Girone checkB = gironeADb.Girone.SingleOrDefault(g => g.ID == 1);
            Girone checkC = gironeADb.Girone.SingleOrDefault(g => g.ID == 1);
            if (checkA == null && checkB == null && checkC == null)
            {
                //Pull the list players and sort it by ELO
                List<Partecipante> players = new List<Partecipante>();
                foreach (Partecipante p in partecipantiDb.Partecipanti)
                {
                    if(players.Count < MaxPlayers)
                    {
                        players.Add(p);
                    }
                }
                players = players.OrderBy(p1 => p1.ELO).ToList();

                for (int i = 0; i < 3; ++i)
                {
                    List<Partecipante> groupPlayers = new List<Partecipante>();
                    while (groupPlayers.Count < MaxPlayers / 3)
                    {
                        groupPlayers.Add(players.ElementAt(0));
                        players.RemoveAt(0);
                    }

                    //Push the list to the db and update the players record with the group id
                    foreach (Partecipante p in groupPlayers)
                    {
                        Partecipante pUpdateGroup = partecipantiDb.Partecipanti.SingleOrDefault(p1 => p1.ID == p.ID);
                        if (pUpdateGroup != null)
                        {
                            if (i == 0)
                            {
                                pUpdateGroup.Girone = "A";
                            }
                            else if (i == 1)
                            {
                                pUpdateGroup.Girone = "B";
                            }
                            else
                            {
                                pUpdateGroup.Girone = "C";
                            }
                            partecipantiDb.SaveChanges();
                        }
                        string resultsDummy = "";
                        for(int j = 0; j < groupPlayers.Count; ++j)
                        {
                            resultsDummy += "-1";
                            if(j != groupPlayers.Count - 1)
                            {
                                resultsDummy += ",";
                            }
                        }
                        Girone g = new Girone
                        {
                            PlayerID = p.ID,
                            Results = resultsDummy
                        };
                        if(i == 0)
                        {
                            gironeADb.Girone.Add(g);
                            gironeADb.SaveChanges();
                        }
                        else if(i == 1)
                        {
                            gironeBDb.Girone.Add(g);
                            gironeBDb.SaveChanges();
                        }
                        else
                        {
                            gironeCDb.Girone.Add(g);
                            gironeCDb.SaveChanges();
                        }
                    }
                }
                res += Strings.groupsSeeded;
            }
            else
            {
                res += Strings.groupsAlreadySeeded;
            }
            return res;
        }

        /// <summary>
        /// Shows the games results of a player, group or all groups
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MaxPlayers"></param>
        /// <returns></returns>
        private string RisultatiCommand(string message, int MaxPlayers)
        {
            string res = "";
            string results = "";
            string[] subresults;
            string[] subs;
            int maxLen = 0;

            subs = message.Split(' ');
            if (subs.Length == 1) //All groups
            {
                res += FetchGroupResults(0, MaxPlayers);
                for (int i = 0; i < 60; ++i)
                {
                    res += "-";
                }
                res += "\n";
                res += FetchGroupResults(1, MaxPlayers);
                for (int i = 0; i < 60; ++i)
                {
                    res += "-";
                }
                res += "\n";
                res += FetchGroupResults(2, MaxPlayers);
                for (int i = 0; i < 60; ++i)
                {
                    res += "-";
                }
                res += "\n";
            }
            else if (subs.Length == 2)
            {
                if (subs[1] == "A" || subs[1] == "B" || subs[1] == "C") //One group
                {
                    if (subs[1] == "A")
                    {
                        res += FetchGroupResults(0, MaxPlayers);
                    }
                    else if (subs[1] == "B")
                    {
                        res += FetchGroupResults(1, MaxPlayers);
                    }
                    else
                    {
                        res += FetchGroupResults(2, MaxPlayers);
                    }
                }
                else //One player
                {
                    string submittedID = subs[1];
                    int? playerID = null;
                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID) != null)
                    {
                        playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).ID;
                    }
                    Girone checkA = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                    Girone checkB = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                    Girone checkC = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                    int groupID = (checkA != null) ? 0 : (checkB != null) ? 1 : (checkC != null) ? 2 : -1;
                    string opponent;
                    if (playerID != null && groupID != -1)
                    {
                        //Retrieve the lichess id with proper capitalization
                        submittedID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == playerID).LichessID;
                        //Retrieve the player results, split them in an array
                        if(groupID == 0)
                        {
                            results = gironeADb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results;
                        }
                        else if (groupID == 1)
                        {
                            results = gironeBDb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results;
                        }
                        else
                        {
                            results = gironeCDb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results;
                        }
                        subresults = results.Split(',');

                        res += "<pre>";
                        res += submittedID + "\n";

                        //Find out the longest opponents name to nicely format the results
                        maxLen = 0;

                        if (groupID == 0)
                        {
                            foreach (Girone g in gironeADb.Girone)
                            {
                                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                                {
                                    maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                                }
                            }
                        }
                        else if (groupID == 1)
                        {
                            foreach (Girone g in gironeBDb.Girone)
                            {
                                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                                {
                                    maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                                }
                            }
                        }
                        else
                        {
                            foreach (Girone g in gironeCDb.Girone)
                            {
                                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                                {
                                    maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                                }
                            }
                        }

                        //Format opponents names and results
                        for (int i = 0; i < subresults.Length; ++i)
                        {
                            int opponentID;
                            if (groupID == 0)
                            {
                                opponentID = gironeADb.Girone.SingleOrDefault(g1 => g1.ID == i + 1).PlayerID;
                            }
                            else if (groupID == 1)
                            {
                                opponentID = gironeBDb.Girone.SingleOrDefault(g1 => g1.ID == i + 1).PlayerID;
                            }
                            else
                            {
                                opponentID = gironeCDb.Girone.SingleOrDefault(g1 => g1.ID == i + 1).PlayerID;
                            }
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

        /// <summary>
        /// Blanket method to fetch the specified group results
        /// </summary>
        /// <param name="Group"></param>
        /// <param name="MaxPlayers"></param>
        /// <returns></returns>
        private string FetchGroupResults(int Group, int MaxPlayers)
        {
            string results;
            string[] subresults;
            string res = "";
            Girone gir;
            if (Group == 0)
            {
                gir = gironeADb.Girone.SingleOrDefault(g => g.ID == 1);
            }
            else if(Group == 1)
            {
                gir = gironeBDb.Girone.SingleOrDefault(g => g.ID == 1);
            }
            else
            {
                gir = gironeCDb.Girone.SingleOrDefault(g => g.ID == 1);
            }
            if (gir != null)
            {

                if (Group == 0)
                {
                    res += Strings.risultatiHeaderA;
                }
                else if (Group == 1)
                {
                    res += Strings.risultatiHeaderB;
                }
                else
                {
                    res += Strings.risultatiHeaderC;
                }
                for (int i = 0; i < MaxPlayers / 3; ++i)
                {

                    if (Group == 0)
                    {
                        res += gironeADb.Girone.SingleOrDefault(g => g.ID == i + 1).PlayerID;
                        if (gironeADb.Girone.SingleOrDefault(g => g.ID == i + 1).PlayerID < 10)
                        {
                            res += "&#32 ";
                        }
                        else
                        {
                            res += " ";
                        }
                    }
                    else if (Group == 1)
                    {
                        res += gironeBDb.Girone.SingleOrDefault(g => g.ID == i + 1).PlayerID;
                        if (gironeBDb.Girone.SingleOrDefault(g => g.ID == i + 1).PlayerID < 10)
                        {
                            res += "&#32 ";
                        }
                        else
                        {
                            res += " ";
                        }
                    }
                    else
                    {
                        res += gironeCDb.Girone.SingleOrDefault(g => g.ID == i + 1).PlayerID;
                        if (gironeCDb.Girone.SingleOrDefault(g => g.ID == i + 1).PlayerID < 10)
                        {
                            res += "&#32 ";
                        }
                        else
                        {
                            res += " ";
                        }
                    }
                }
                res += "\n";
                if (Group == 0)
                {
                    foreach (Girone g in gironeADb.Girone)
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
                }
                else if (Group == 1)
                {
                    foreach (Girone g in gironeBDb.Girone)
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
                }
                else
                {
                    foreach (Girone g in gironeCDb.Girone)
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
                }
                
                res += "</pre>\n";
            }
            return res;
        }

        /// <summary>
        /// Shows the current groups standings
        /// </summary>
        /// <returns></returns>
        private string ClassificaCommand(int MaxPlayers)
        {
            string res = "";
            string[] subresults;

            //Find out the longest opponents name to nicely format the results
            int maxLen = 0;

            for (int j = 0; j < 3; ++j)
            {
                Girone dbCheck;
                if(j == 0)
                {
                    dbCheck = gironeADb.Girone.SingleOrDefault(g => g.ID == 1);
                }
                else if(j == 1)
                {
                    dbCheck = gironeBDb.Girone.SingleOrDefault(g => g.ID == 1);
                }
                else
                {
                    dbCheck = gironeCDb.Girone.SingleOrDefault(g => g.ID == 1);
                }
                if (dbCheck != null)
                {
                    if (j == 0)
                    {
                        foreach (Girone g in gironeADb.Girone)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                            {
                                maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                            }
                        }
                    }
                    else if (j == 1)
                    {
                        foreach (Girone g in gironeBDb.Girone)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                            {
                                maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                            }
                        }
                    }
                    else
                    {
                        foreach (Girone g in gironeCDb.Girone)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length > maxLen)
                            {
                                maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID.Length;
                            }
                        }
                    }

                    List<Standing> standings = new List<Standing>();

                    if (j == 0)
                    {
                        foreach (Girone g in gironeADb.Girone)
                        {
                            Standing stg = new Standing
                            {
                                ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID
                            };
                            subresults = g.Results.Split(",");
                            stg.Games = new string[subresults.Length];
                            stg.Tot = 0;
                            for (int i = 0; i < subresults.Length; ++i)
                            {
                                if (subresults[i] == "x")
                                {
                                    stg.Games[i] = "&#189;";
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
                    }
                    else if (j == 1)
                    {
                        foreach (Girone g in gironeBDb.Girone)
                        {
                            Standing stg = new Standing
                            {
                                ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID
                            };
                            subresults = g.Results.Split(",");
                            stg.Games = new string[subresults.Length];
                            stg.Tot = 0;
                            for (int i = 0; i < subresults.Length; ++i)
                            {
                                if (subresults[i] == "x")
                                {
                                    stg.Games[i] = "&#189;";
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
                    }
                    else
                    {
                        foreach (Girone g in gironeCDb.Girone)
                        {
                            Standing stg = new Standing
                            {
                                ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.ID == g.PlayerID).LichessID
                            };
                            subresults = g.Results.Split(",");
                            stg.Games = new string[subresults.Length];
                            stg.Tot = 0;
                            for (int i = 0; i < subresults.Length; ++i)
                            {
                                if (subresults[i] == "x")
                                {
                                    stg.Games[i] = "&#189;";
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
                    }
                    standings = standings.OrderByDescending(s => s.Tot).ToList();
                    if (j == 0)
                    {
                        res += "<pre>" + Strings.classificaHeader1A;
                    }
                    else if (j == 1)
                    {
                        res += "<pre>" + Strings.classificaHeader1B;
                    }
                    else
                    {
                        res += "<pre>" + Strings.classificaHeader1C;
                    }
                    for (int i = 0; i < maxLen - 4; ++i)
                    {
                        res += " ";
                    }
                    res += Strings.classificaHeader2;
                    for (int i = 0; i < MaxPlayers / 3; ++i)
                    {
                        res += " " + i;
                    }
                    res += " P\n"; 
                    foreach (Standing s in standings)
                    {
                        int gamesPlayed = 0;
                        res += s.ID;
                        for (int i = 0; i < maxLen - s.ID.Length; ++i)
                        {
                            res += " ";
                        }
                        res += " ";
                        string[] test = s.Games;
                        for (int i = 0; i < s.Games.Length; ++i)
                        {
                            if (s.Games[i] != null)
                            {
                                res += s.Games[i] + " ";
                                gamesPlayed++;
                            }
                        }
                        for (int i = 0; i < 2 * ((MaxPlayers / 3) - gamesPlayed) - 1; ++i)
                        {
                            res += " ";
                        }
                        if (s.Tot % 1 != 0)
                        {
                            if ((int)s.Tot == 0)
                            {
                                res += " &#189 \n";
                            }
                            else
                            {
                                res += " " + (int)s.Tot + "&#189 \n";
                            }
                        }
                        else
                        {
                            res += " " + s.Tot + "\n";
                        }
                    }
                    res += "</pre>";
                }
                else
                {
                    res += Strings.errorNotSeeded;
                }
                for(int i = 0; i < 60; ++i)
                {
                    res += "-";
                }
                res += "\n";
            }
            return res;
        }

        /// <summary>
        /// Adds the specified player to the DB, if it doesn't already exist (Admin only)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string InserisciCommand(string message)
        {
            //TODO: Simplify the method
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
            //Check if the db is populated 
            Girone dbCheck = gironeADb.Girone.SingleOrDefault(g => g.ID == 1);
            if (dbCheck != null)
            {
                if (subs.Length != 4)
                {
                    res += Strings.inserisciUsage;
                }
                else
                {
                    string sub1 = subs[1];
                    string sub2 = subs[2];
                    //Check if LichessIDs are valid
                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower()) != null &&
                        partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower()) != null)
                    {
                        Partecipante p1 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                        Partecipante p2 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower());
                        Girone check1A = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == p1.ID);
                        Girone check1B = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == p1.ID);
                        Girone check1C = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == p1.ID);
                        Girone check2A = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == p2.ID);
                        Girone check2B = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == p2.ID);
                        Girone check2C = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == p2.ID);
                        int groupIDp1 = (check1A != null) ? 0 : (check1B != null) ? 1 : (check1C != null) ? 2 : -1;
                        int groupIDp2 = (check2A != null) ? 0 : (check2B != null) ? 1 : (check2C != null) ? 2 : -1;
                        if (groupIDp1 != -1 && groupIDp2 != -1 && groupIDp2 == groupIDp1)
                        {
                            int groupID = groupIDp1;
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
                                if(groupID == 0)
                                {
                                    player1GroupID = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).ID;
                                }
                                else if(groupID == 1)
                                {
                                    player1GroupID = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).ID;
                                }
                                else
                                {
                                    player1GroupID = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).ID;
                                }

                                player2LichessID = subs[2];
                                player2LichessID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).LichessID;
                                player2ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).ID;
                                if (groupID == 0)
                                {
                                    player2GroupID = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).ID;
                                }
                                else if (groupID == 1)
                                {
                                    player2GroupID = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).ID;
                                }
                                else
                                {
                                    player2GroupID = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).ID;
                                }

                                if (groupID == 0)
                                {
                                    prevResults = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results;
                                }
                                else if (groupID == 1)
                                {
                                    prevResults = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results;
                                }
                                else
                                {
                                    prevResults = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results;
                                }
                                subresults = prevResults.Split(",");

                                if (subresults[player2GroupID - 1] == "-1")
                                {
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
                                    if (groupID == 0)
                                    {
                                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);
                                    }
                                    else if (groupID == 1)
                                    {
                                        gironeBDb.Database.ExecuteSqlCommand(SQLCommand);
                                    }
                                    else
                                    {
                                        gironeCDb.Database.ExecuteSqlCommand(SQLCommand);
                                    }

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
                                        helper = "x";
                                    }

                                    if (groupID == 0)
                                    {
                                        prevResults = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results;
                                    }
                                    else if (groupID == 1)
                                    {
                                        prevResults = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results;
                                    }
                                    else
                                    {
                                        prevResults = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results;
                                    }
                                    subresults = prevResults.Split(",");
                                }
                                else
                                {
                                    res += Strings.alreadyInserted + Strings.errorContact;
                                    return res;
                                }
                                if (subresults[player1GroupID - 1] == "-1")
                                {
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
                                    if (groupID == 0)
                                    {
                                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);
                                    }
                                    else if (groupID == 1)
                                    {
                                        gironeBDb.Database.ExecuteSqlCommand(SQLCommand);
                                    }
                                    else
                                    {
                                        gironeCDb.Database.ExecuteSqlCommand(SQLCommand);
                                    }

                                    res += Strings.insertedResult + Strings.checkResults;
                                }
                                else
                                {
                                    res += Strings.alreadyInserted + Strings.errorContact;
                                    return res;
                                }
                            }
                            else
                            {
                                res += Strings.inserisciInvalidResult;
                            }
                        }
                        else
                        {
                            res += Strings.inserisciInvalidGroup;
                        }
                    }
                    else
                    {
                        res += Strings.inserisciInvalidIDs;
                    }
                }
            }
            else
            {
                res += Strings.errorNotSeeded;
            }
            return res;
        }

        /// <summary>
        /// Shows torunament info
        /// </summary>
        /// <returns></returns>
        private string TorneoCommand()
        {
            //FINAL
            string res = "";
            res += Strings.torneoInfo;
            return res;
        }

        /// <summary>
        /// Responds to anything other than a known command
        /// </summary>
        /// <returns></returns>
        private string NoCommand()
        {
            //FINAL
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

                if (int.TryParse(downloadString.Substring(i + 16, 4), out int elo))
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

        /// <summary>
        /// Check if the message sender is an admin
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
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