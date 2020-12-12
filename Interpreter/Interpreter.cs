using avenabot.DAL;
using avenabot.Models.Partecipanti;
using Telegram.Bot.Args;
using System.Linq;
using System.Net;
using System;
using avenabot.Models.Gironi;
using System.Collections.Generic;
using System.Data.Entity;

//TODO: Semplificazione (es. utilizzo campo Girone di partecipante anziché continuare a fare query per trovarlo)

namespace avenabot.Interpreter
{
    public class Interpreter
    {
        private static PartecipantiDbContext partecipantiDb = new PartecipantiDbContext();
        private static GironeADbContext gironeADb = new GironeADbContext();
        private static GironeBDbContext gironeBDb = new GironeBDbContext();
        private static GironeCDbContext gironeCDb = new GironeCDbContext();
        private static GironeFDbContext gironeFDb = new GironeFDbContext();

        public string[] admin = new string[]
        {
            "albusellato"
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
            Strings.partiteCommand, //Show games list
            Strings.miePartiteCommand, //Show games to play
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
            Strings.partiteDescr,
            Strings.miePartiteDescr,
        };

        public Interpreter() { }

        public struct Standing
        {
            public string ID { get; set; }
            public string[] Games { get; set; }
            public double Tot { get; set; }
        }

        private readonly DateTime finalsDate = new DateTime(2020, 12, 1, 12, 0, 0); //Change this to select when to switch to the final group

        public string Parse(MessageEventArgs e)
        {
            partecipantiDb = new PartecipantiDbContext();
            gironeADb = new GironeADbContext();
            gironeBDb = new GironeBDbContext();
            gironeCDb = new GironeCDbContext();
            gironeFDb = new GironeFDbContext();
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            DateTime closingDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to close registering
            int MaxPlayers = 10;
            int MaxGroups = 2;
            int MaxFinalists = 2;
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
                7 => SeedCommand(MaxPlayers, MaxGroups, MaxFinalists),
                // /risultati
                8 => RisultatiCommand(message, MaxGroups),
                // /classifica
                9 => ClassificaCommand(MaxPlayers, MaxGroups),
                // /inserisci
                10 => InserisciCommand(message, sender, MaxGroups),
                // /torneo
                11 => TorneoCommand(),
                // /partite
                12 => PartiteCommand(message),
                // /miepartite
                13 => MiePartiteCommand(message, sender),
                _ => NoCommand(),
            };
            gironeADb.Dispose();
            gironeBDb.Dispose();
            gironeCDb.Dispose();
            gironeFDb.Dispose();
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
                        if (commandDescr[i].IndexOf(commandList[i]) == -1)
                        {
                            res += commandList[i] + "\n";
                        }
                        res += commandDescr[i] + "\n";
                        for (int j = 0; j < 54; ++j)
                        {
                            res += '-';
                        }
                        res += '\n';
                    }
                }
                else
                {
                    if(commandDescr[i].IndexOf(commandList[i]) == -1)
                    {
                        res += commandList[i] + "\n";
                    }
                    res += commandDescr[i] + "\n";
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
                res += p.TID + "-" + p.LichessID + " - @" + p.TGID + " - " + p.ELO + " - " + p.Girone + "\n";
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

            if (DateTime.Now > closingDate && playerCount >= MaxPlayers)
            {
                res = Strings.closedRegistrations;
                return res;
            }

            //Get the Lichess ID
            lichessID = message.Replace(commandList[Find("/iscrivimi")], string.Empty).Trim();
            //Fetch the player's ELO from Lichess
            elo = GetELO(lichessID);
            if (lichessID == "") //If no ID was provided
            {
                res = Strings.iscrivimiUsage;
                return res;
            }

            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == sender) != null) //If the player is already registered
            {
                res = Strings.registeredError + Strings.checkPartecipanti + Strings.errorContact;
                return res;
            }

            if (elo == -1) //If the player wasn't found on Lichess
            {
                res = Strings.lichess404 + Strings.errorContact;
                return res;
            }

            int tid = GetMaxTID() + 1;

            //Create the player and push him into the db
            Partecipante p = new Partecipante
            {
                LichessID = lichessID,
                TGID = sender,
                ELO = elo,
                TID = tid
            };
            partecipantiDb.Partecipanti.Add(p);
            partecipantiDb.SaveChanges();
            res = Strings.registered + Strings.checkPartecipanti;
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
                    return res;
                }
                if(elo == -1)
                {
                    res = Strings.lichess404 + Strings.errorContact;
                    return res;
                }
                //Remove the player from the DB, if he exists
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower()) == null)
                {
                    res = Strings.player404 + Strings.checkPartecipanti + Strings.errorContact;
                    return res;
                }
                
                //Find and remove the player from the db
                Partecipante p = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower());
                int removedTID = p.TID;
                partecipantiDb.Partecipanti.Attach(p);
                partecipantiDb.Partecipanti.Remove(p);

                //Update the TIDs of other players
                foreach(Partecipante par in partecipantiDb.Partecipanti)
                {
                    if(par.TID > removedTID)
                    {
                        par.TID -= 1;
                    }
                }
                partecipantiDb.SaveChanges();
                res = Strings.removedAdmin + Strings.checkPartecipanti;
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
                int removedTID = p.TID;
                partecipantiDb.Partecipanti.Attach(p);
                partecipantiDb.Partecipanti.Remove(p);
                //Update the TIDs of other players
                foreach (Partecipante par in partecipantiDb.Partecipanti)
                {
                    if (par.TID > removedTID)
                    {
                        par.TID -= 1;
                    }
                }
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
                if (message.Length == commandList[6].Length) //If the number of arguments is not the correct one
                {
                    res = Strings.aggiungiUsage;
                    return res;
                }
                subs = message.Split(' ');
                if (subs.Length != 3) //If the number of arguments is not the correct one
                {
                    res = Strings.aggiungiUsage;
                    return res;
                }

                lichessID = subs[1];
                tgID = subs[2];
                elo = GetELO(lichessID);
                if (elo == -1)
                {
                    res = Strings.lichess404 + Strings.errorContact;
                    return res;
                }
                
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == tgID) != null) //If the player is already in the DB
                {
                    res = Strings.registeredAdminError + Strings.checkPartecipanti + Strings.errorContact;
                    return res;
                }

                int tid = GetMaxTID() + 1;
                Partecipante p = new Partecipante
                {
                    LichessID = lichessID,
                    TGID = tgID,
                    ELO = elo,
                    TID = tid
                };
                partecipantiDb.Partecipanti.Add(p);
                partecipantiDb.SaveChanges();
                res = Strings.registeredAdmin + Strings.checkPartecipanti;
            }
            return res;
        }

        /// <summary>
        /// Populates the groups using ELO as a parameter
        /// </summary>
        /// <param name="MaxPlayers"></param>
        /// <param name="MaxGroups"></param>
        /// <returns></returns>
        private string SeedCommand(int MaxPlayers, int MaxGroups, int MaxFinalists)
        {
            //FINAL
            string res = "";

            //Seed final group if it's past the date
            if (DateTime.Now > finalsDate)
            {
                int checkF = gironeFDb.Girone.Count();
                if (checkF > 0)
                {
                    res += Strings.finalGroupAlreadySeeded;
                    return res;
                }

                List<Standing> standings;
                List<Girone> finalPlayers = new List<Girone>();
                string[] subresults;
                for (int j = 0; j < MaxGroups; ++j)
                {
                    //Pull the list of players
                    DbSet<Girone> dbset = j switch
                    {
                        0 => gironeADb.Girone,
                        1 => gironeBDb.Girone,
                        _ => gironeCDb.Girone,
                    };
                    standings = new List<Standing>();
                    foreach (Girone g in dbset)
                    {
                        Standing stg = new Standing
                        {
                            ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID
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
                    standings = standings.OrderByDescending(s => s.Tot).ToList();
                    int id;
                    int cnt = 0;
                    foreach (Standing s in standings)
                    {
                        if (cnt < MaxFinalists)
                        {
                            cnt++;
                            id = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == s.ID.ToLower()).TID;
                            Girone g = j switch {
                                0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == id),
                            };
                            finalPlayers.Add(g);
                        }
                    }
                }
                int gid = 1;
                string resultsDummy = "";
                for (int j = 0; j < finalPlayers.Count; ++j)
                {
                    resultsDummy += "-1";
                    if (j != finalPlayers.Count - 1)
                    {
                        resultsDummy += ",";
                    }
                }
                //Push to the db
                foreach (Girone g in finalPlayers)
                {
                    partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).Girone = "F";
                    partecipantiDb.SaveChanges();
                    g.GID = gid;
                    gid++;
                    g.Results = resultsDummy;
                    gironeFDb.Girone.Add(g);
                    gironeFDb.SaveChanges();
                }
                res += Strings.finalGroupSeeded;
                return res;
            }

            if (MaxPlayers % 3 != 0 || MaxPlayers % 2 != 0)
            {
                res += Strings.internalError + 2;
                return res;
            }
            //Check if the DBs are empty
            int checkA = gironeADb.Girone.Count();
            int checkB = gironeADb.Girone.Count();
            int checkC;
            if (checkA > 0 && checkB > 0)
            {
                res += Strings.groupsAlreadySeeded;
                return res;
            }
            if (MaxGroups == 3)
            {
                checkC = gironeADb.Girone.Count();
                if (checkC > 0)
                {
                    res += Strings.groupsAlreadySeeded;
                    return res;
                }
            }

            //Check if the player list has enough elements
            if(GetPlayerCount() < MaxPlayers)
            {
                res += Strings.notEnoughPlayers;
                return res;
            }
            //Pull the list of players and sort it by ELO
            List<Partecipante> players = new List<Partecipante>();
            foreach (Partecipante p in partecipantiDb.Partecipanti)
            {
                if(players.Count < MaxPlayers)
                {
                    players.Add(p);
                }
            }
            players = players.OrderBy(p1 => p1.ELO).ToList();

            for (int i = 0; i < MaxGroups; ++i)
            {
                List<Partecipante> groupPlayers = new List<Partecipante>();
                while (groupPlayers.Count < MaxPlayers / MaxGroups)
                {
                    groupPlayers.Add(players.ElementAt(0));
                    players.RemoveAt(0);
                }

                //Push the list to the db and update the players record with the group id
                foreach (Partecipante p in groupPlayers)
                {
                    Partecipante pUpdateGroup = partecipantiDb.Partecipanti.SingleOrDefault(p1 => p1.TID == p.TID);
                    if (pUpdateGroup != null)
                    {
                        pUpdateGroup.Girone = i switch
                        {
                            0 => "A",
                            1 => "B",
                            _ => "C",
                        };
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
                    int gid;
                    Girone g = new Girone
                    {
                        PlayerID = p.TID,
                        Results = resultsDummy
                    };
                    switch(i)
                    {
                        case 0:
                            gid = GetMaxGID("A") + 1;
                            g.GID = gid;
                            gironeADb.Girone.Add(g);
                            gironeADb.SaveChanges();
                            break;
                        case 1:
                            gid = GetMaxGID("B") + 1;
                            g.GID = gid;
                            gironeBDb.Girone.Add(g);
                            gironeBDb.SaveChanges();
                            break;
                        default:
                            gid = GetMaxGID("C") + 1;
                            g.GID = gid;
                            gironeCDb.Girone.Add(g);
                            gironeCDb.SaveChanges();
                            break;
                    }
                }
            }
            res += Strings.groupsSeeded;
            return res;
        }

        /// <summary>
        /// Shows the games results of a player, group or all groups
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MaxPlayers"></param>
        /// <returns></returns>
        private string RisultatiCommand(string message, int MaxGroups)
        {
            //FINAL
            string res = "";
            string results = "";
            string[] subresults;
            string[] subs;
            int maxLen = 0;
            if (gironeADb.Girone.Count() <= 0 || gironeBDb.Girone.Count() <= 0 || 
                (MaxGroups == 3 && gironeCDb.Girone.Count() <= 0) ||
                (DateTime.Now > finalsDate && gironeFDb.Girone.Count() <= 0))
            {
                res += Strings.notYetSeededGroups;
                return res;
            }

            subs = message.Split(' ');

            if (subs.Length == 1) //All groups
            {
                if (DateTime.Now < finalsDate)
                {
                    for (int j = 0; j < MaxGroups; ++j)
                    {
                        res += FetchGroupResults(j);
                        for (int i = 0; i < 60; ++i)
                        {
                            res += "-";
                        }
                        res += "\n";
                    }
                }
                else
                {
                    res += FetchGroupResults(3);
                }
                res += "\n" + Strings.resultsDisclaimer + "\n";
            }
            else if (subs.Length == 2)
            {
                switch(subs[1])
                {
                    case "A":
                        res += FetchGroupResults(0);
                        res += "\n" + Strings.resultsDisclaimer + "\n";
                        break;
                    case "B":
                        res += FetchGroupResults(1);
                        res += "\n" + Strings.resultsDisclaimer + "\n";
                        break;
                    case "C":
                        if (MaxGroups != 3)
                        {
                            res += Strings.saywhat;
                            return res;
                        }
                        res += FetchGroupResults(2);
                        res += "\n" + Strings.resultsDisclaimer + "\n";
                        break;
                    case "F":
                        if (DateTime.Now < finalsDate)
                        {
                            res += Strings.saywhat;
                            return res;
                        }
                        res += FetchGroupResults(3);
                        res += "\n" + Strings.resultsDisclaimer + "\n";
                        break;
                    default:
                        string submittedID = subs[1];
                        int? playerID = null;
                        if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID) == null)
                        {
                            res += Strings.player404;
                            return res;
                        }
                        playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).TID;
                        Girone checkA = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                        Girone checkB = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                        Girone checkC = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                        Girone checkF = gironeFDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                        int groupID = (checkA != null) ? 0 : (checkB != null) ? 1 : -1;
                        if(MaxGroups == 3)
                        {
                            groupID = (checkA != null) ? 0 : (checkB != null) ? 1 : (checkC != null) ? 2 : -1;
                        }
                        if(DateTime.Now > finalsDate)
                        {
                            groupID = (checkA != null) ? 0 : (checkB != null) ? 1 : (checkC != null) ? 2 : (checkF != null) ? 3 : -1;
                        }
                        if (groupID == -1)
                        {
                            res = Strings.risultatiUsage;
                            return res;
                        }

                        string opponent;
                        //Retrieve the lichess id with proper capitalization
                        submittedID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == playerID).LichessID;
                        //Retrieve the player results, split them in an array
                        results = groupID switch
                        {
                            0 => gironeADb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results,
                            1 => gironeBDb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results,
                            3 => gironeFDb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results,
                            _ => gironeCDb.Girone.SingleOrDefault(p => p.PlayerID == playerID).Results,
                        };
                        subresults = results.Split(',');

                        res += "<pre>";
                        res += submittedID + "\n";

                        //Find out the longest opponents name to nicely format the results
                        maxLen = 0;
                        DbSet<Girone> dbset = groupID switch
                        {
                            0 => gironeADb.Girone,
                            1 => gironeBDb.Girone,
                            3 => gironeFDb.Girone,
                            _ => gironeCDb.Girone
                        };
                        foreach (Girone g in dbset)
                        {
                            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID.Length > maxLen)
                            {
                                maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID.Length;
                            }
                        }

                        //Format opponents names and results
                        for (int i = 0; i < subresults.Length; ++i)
                        {
                            int opponentID = groupID switch
                            {
                                0 => gironeADb.Girone.SingleOrDefault(g1 => g1.GID == i + 1).PlayerID,
                                1 => gironeBDb.Girone.SingleOrDefault(g1 => g1.GID == i + 1).PlayerID,
                                3 => gironeFDb.Girone.SingleOrDefault(g1 => g1.GID == i + 1).PlayerID,
                                _ => gironeCDb.Girone.SingleOrDefault(g1 => g1.GID == i + 1).PlayerID
                            };
                            opponent = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == opponentID).LichessID;

                            if (opponent != submittedID)
                            {
                                res += "vs " + opponent;

                                for (int j = 0; j < maxLen - opponent.Length + 1; ++j)
                                {
                                    res += "&#32";
                                }

                                res += subresults[i] switch
                                {
                                    "x" => " &#189 \n",
                                    "-1" => " -\n",
                                    _ => " " + subresults[i] + "\n"
                                };
                            }
                        }
                        res += "</pre>";
                        break;
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
        private string FetchGroupResults(int Group)
        {
            //FINAL
            string results;
            string[] subresults;
            string res = "";
            int gir = Group switch
            {
                0 => gironeADb.Girone.Count(),
                1 => gironeBDb.Girone.Count(),
                3 => gironeFDb.Girone.Count(),
                _ => gironeCDb.Girone.Count(),
            };
            if (gir > 0)
            {
                res += Group switch
                {
                    0 => Strings.risultatiHeaderA,
                    1 => Strings.risultatiHeaderB,
                    3 => Strings.risultatiHeaderF,
                    _ => Strings.risultatiHeaderC,
                };
                for (int i = 0; i < gir; ++i)
                {
                    switch(Group)
                    {
                        case 0:
                            res += gironeADb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID;
                            if (gironeADb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID < 10)
                            {
                                res += "&#32 ";
                            }
                            else
                            {
                                res += " ";
                            }
                            break;
                        case 1:
                            res += gironeBDb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID;
                            if (gironeBDb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID < 10)
                            {
                                res += "&#32 ";
                            }
                            else
                            {
                                res += " ";
                            }
                            break;
                        case 3:
                            res += gironeFDb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID;
                            if (gironeFDb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID < 10)
                            {
                                res += "&#32 ";
                            }
                            else
                            {
                                res += " ";
                            }
                            break;
                        default:
                            res += gironeCDb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID;
                            if (gironeCDb.Girone.SingleOrDefault(g => g.GID == i + 1).PlayerID < 10)
                            {
                                res += "&#32 ";
                            }
                            else
                            {
                                res += " ";
                            }
                            break;
                    };
                }
                res += "\n";
                DbSet<Girone> dbset = Group switch
                {
                    0 => gironeADb.Girone,
                    1 => gironeBDb.Girone,
                    3 => gironeFDb.Girone,
                    _ => gironeCDb.Girone,
                };

                foreach (Girone g in dbset)
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
                res += "</pre>\n";
            }
            return res;
        }

        /// <summary>
        /// Shows the current groups standings
        /// </summary>
        /// <returns></returns>
        private string ClassificaCommand(int MaxPlayers, int MaxGroups)
        {
            //FINAL
            string res = "";
            string[] subresults;
            int dbCheck;
            DbSet<Girone> dbset;

            if (gironeADb.Girone.Count() <= 0 || gironeBDb.Girone.Count() <= 0 ||
                (gironeCDb.Girone.Count() <= 0 && MaxGroups == 3) ||
                (gironeFDb.Girone.Count() <= 0 && DateTime.Now > finalsDate))
            {
                res += Strings.notYetSeededStandings;
                return res;
            }

            if(DateTime.Now > finalsDate)
            {
                dbCheck = gironeFDb.Girone.Count();
                if (dbCheck <= 0)
                {
                    res += Strings.errorNotSeeded;
                    return res;
                }
                dbset = gironeFDb.Girone;
                //Find out the longest opponents name to nicely format the results
                int maxLen = 0;
                foreach (Girone g in dbset)
                {
                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID.Length > maxLen)
                    {
                        maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID.Length;
                    }
                }

                List<Standing> standings = new List<Standing>();

                foreach (Girone g in dbset)
                {
                    Standing stg = new Standing
                    {
                        ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID
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
                standings = standings.OrderByDescending(s => s.Tot).ToList();

                res += "<pre>" + Strings.classificaHeader1F;

                for (int i = 0; i < maxLen - 4; ++i)
                {
                    res += " ";
                }
                res += Strings.classificaHeader2;
                for (int i = 1; i < dbCheck; ++i)
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
                    for (int i = 0; i < 2 * (dbCheck - gamesPlayed) - 2; ++i)
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
                        res += s.Tot + "\n";
                    }
                }
                res += "</pre>";
                for (int i = 0; i < 60; ++i)
                {
                    res += "-";
                }
                res += "\n";
                return res;
            }

            for (int j = 0; j < MaxGroups; ++j)
            {
                dbCheck = j switch
                {
                    0 => gironeADb.Girone.Count(),
                    1 => gironeBDb.Girone.Count(),
                    _ => gironeCDb.Girone.Count(),
                };
                if (dbCheck <= 0)
                {
                    res += Strings.errorNotSeeded;
                    return res;
                }
                dbset = j switch
                {
                    0 => gironeADb.Girone,
                    1 => gironeBDb.Girone,
                    _ => gironeCDb.Girone,
                };
                //Find out the longest opponents name to nicely format the results
                int maxLen = 0;
                foreach (Girone g in dbset)
                {
                    if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID.Length > maxLen)
                    {
                        maxLen = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID.Length;
                    }
                }

                List<Standing> standings = new List<Standing>();

                foreach (Girone g in dbset)
                {
                    Standing stg = new Standing
                    {
                        ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID
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
                standings = standings.OrderByDescending(s => s.Tot).ToList();

                res += j switch
                {
                    0 => "<pre>" + Strings.classificaHeader1A,
                    1 => "<pre>" + Strings.classificaHeader1B,
                    _ => "<pre>" + Strings.classificaHeader1C,
                };
                for (int i = 0; i < maxLen - 4; ++i)
                {
                    res += " ";
                }
                res += Strings.classificaHeader2;
                for (int i = 1; i < MaxPlayers / MaxGroups; ++i)
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
                    for (int i = 0; i < 2 * ((MaxPlayers / MaxGroups) - gamesPlayed) - 2; ++i)
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
                        res += s.Tot + "\n";
                    }
                }
                res += "</pre>";
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
        private string InserisciCommand(string message, string sender, int MaxGroups)
        {
            //FINAL
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
            Partecipante p1;
            Partecipante p2;
            int groupID;
            string sub1;
            string sub2;
            subs = message.Split(" ");
            //Check if the db is populated 
            int dbCheckA = gironeADb.Girone.Count();
            int dbCheckB = gironeBDb.Girone.Count();
            if (dbCheckA <= 0 && dbCheckB <= 0)
            {
                res += Strings.errorNotSeeded;
                return res;
            }
            if(MaxGroups == 3)
            {
                int dbCheckC = gironeCDb.Girone.Count();
                if (dbCheckC <= 0)
                {
                    res += Strings.errorNotSeeded;
                    return res;
                }
            }
            if (subs.Length != 4 && subs.Length != 2)
            {
                res += Strings.inserisciUsage;
                return res;
            }
            if (subs.Length == 2) // /inserisci OpponentID
            {
                sub1 = subs[1];
                //Check if the players are in the same group first
                p1 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower());
                p2 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                //Check if LichessIDs are valid
                if (p1 == null || p2 == null)
                {
                    res += Strings.inserisciInvalidIDs2;
                    return res;
                }
                if (p1.Girone != p2.Girone)
                {
                    res += Strings.notSameGroup;
                    return res;
                }
                groupID = p1.Girone switch {
                    "A" => 0,
                    "B" => 1,
                    "F" => 3,
                    _ => 2,
                };
                string[] elab = PullLatestResult(p1.LichessID, p2.LichessID);
                if(elab[0] == "-1")
                {
                    res += Strings.gameNotFound;
                    return res;
                }
                helper = elab[0];
                //Push the game link to the games db
                //Push the result to the group db
                Game game = new Game
                {
                    P1ID = p1.TID,
                    P2ID = p2.TID,
                    Link = elab[1]
                };

                DbSet<Girone> dbsetGroup = groupID switch
                {
                    0 => gironeADb.Girone,
                    1 => gironeBDb.Girone,
                    3 => gironeFDb.Girone,
                    _ => gironeCDb.Girone,
                };
                DbSet<Game> dbsetGames = groupID switch
                {
                    0 => gironeADb.Partite,
                    1 => gironeBDb.Partite,
                    3 => gironeFDb.Partite,
                    _ => gironeCDb.Partite,
                };
                dbsetGames.Add(game);
                player1GroupID = dbsetGroup.SingleOrDefault(g => g.PlayerID == p1.TID).GID;
                player2GroupID = dbsetGroup.SingleOrDefault(g => g.PlayerID == p2.TID).GID;
                prevResults = dbsetGroup.SingleOrDefault(g => g.PlayerID == p1.TID).Results;

                subresults = prevResults.Split(",");

                if (subresults[player2GroupID - 1] != "-1")
                {
                    res += Strings.alreadyInserted + Strings.errorContact;
                    return res;
                }
                subresults[player2GroupID - 1] = elab[0];
                results = "";
                for (int i = 0; i < subresults.Length; ++i)
                {
                    results += subresults[i];
                    if (i != subresults.Length - 1)
                    {
                        results += ",";
                    }
                }

                SQLCommand = "UPDATE Girone SET Results='" + results + "' WHERE PlayerID=" + p1.TID;
                switch(groupID)
                {
                    case 0:
                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                    case 1:
                        gironeBDb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                    case 3:
                        gironeFDb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                    default:
                        gironeCDb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                }
                helper = helper switch {
                    "0" => "1",
                    "1" => "0",
                    _ => "x",
                };
                prevResults = groupID switch {
                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
                    3 => gironeFDb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
                };

                subresults = prevResults.Split(",");
                if (subresults[player1GroupID - 1] != "-1")
                {
                    res += Strings.alreadyInserted + Strings.errorContact;
                    return res;
                }
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

                SQLCommand = "UPDATE Girone SET Results='" + results + "' WHERE PlayerID=" + p2.TID;
                switch (groupID)
                {
                    case 0:
                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeADb.SaveChanges();
                        break;
                    case 1:
                        gironeBDb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeBDb.SaveChanges();
                        break;
                    case 3:
                        gironeFDb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeFDb.SaveChanges();
                        break;
                    default:
                        gironeCDb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeCDb.SaveChanges();
                        break;
                }
                res += Strings.insertedResult + Strings.checkResults;
            }
            else if(IsAdmin(sender))
            {
                sub1 = subs[1];
                sub2 = subs[2];
                //Check if LichessIDs are valid
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower()) == null ||
                    partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower()) == null)
                {
                    res += Strings.inserisciInvalidIDs;
                    return res;
                }
                p1 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                p2 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower());
               
                if (p1.Girone != p2.Girone)
                {
                    res += Strings.notSameGroup;
                    return res;
                }
                groupID = p1.Girone switch
                {
                    "A" => 0,
                    "B" => 1,
                    "F" => 3,
                    _ => 2,
                };
                //Valid result?
                if (subs[3] != "1" && subs[3] != "2" && subs[3] != "x")
                {
                    res += Strings.inserisciInvalidResult;
                    return res;
                }
                helper = subs[3] switch {
                    "1" => "1",
                    "2" => "0",
                    _ => "x",
                };

                player1LichessID = subs[1];
                player1LichessID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).LichessID;
                player1ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).TID;
                player1GroupID = groupID switch
                {
                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                    3 => gironeFDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                };

                player2LichessID = subs[2];
                player2LichessID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).LichessID;
                player2ID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).TID;
                player2GroupID = groupID switch
                {
                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                    3 => gironeFDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                };

                prevResults = groupID switch
                {
                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
                    3 => gironeFDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
                };
                subresults = prevResults.Split(",");

                if (subresults[player2GroupID - 1] != "-1")
                {
                        res += Strings.alreadyInserted + Strings.errorContact;
                        return res;
                }

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
                switch (groupID)
                {
                    case 0:
                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                    case 1:
                        gironeBDb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                    case 3:
                        gironeFDb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                    default:
                        gironeCDb.Database.ExecuteSqlCommand(SQLCommand);
                        break;
                }
                helper = helper switch
                {
                    "0" => "1",
                    "1" => "0",
                    _ => "x",
                };
                
                prevResults = groupID switch
                {
                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
                    3 => gironeFDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
                };
                subresults = prevResults.Split(",");
                if (subresults[player1GroupID - 1] != "-1")
                {
                    res += Strings.alreadyInserted + Strings.errorContact;
                    return res;
                }
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
                switch (groupID)
                {
                    case 0:
                        gironeADb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeADb.SaveChanges();
                        break;
                    case 1:
                        gironeBDb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeBDb.SaveChanges();
                        break;
                    case 3:
                        gironeFDb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeFDb.SaveChanges();
                        break;
                    default:
                        gironeCDb.Database.ExecuteSqlCommand(SQLCommand);
                        gironeCDb.SaveChanges();
                        break;
                }
                res += Strings.insertedResult + Strings.checkResults;
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
        /// Shows list of played games
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string PartiteCommand(string message)
        {
            //FINAL
            string res = "";
            string[] subs = message.Split(" ");
            string p1Lichess;
            string p2Lichess;
            string link;
            if(gironeADb.Partite.Count() <= 0 ||
                gironeBDb.Partite.Count() <= 0 ||
                gironeCDb.Partite.Count() <= 0)
            {
                res += Strings.notYetPlayedGames;
                return res;
            }

            if(subs.Length > 2)
            {
                res += Strings.partiteUsage;
                return res;
            }

            if(subs.Length == 2 && subs[1].Length == 1) // /partite (A B o C)
            {
                res += subs[1] switch
                {
                    "A" => Strings.partiteHeaderA,
                    "B" => Strings.partiteHeaderB,
                    _ => Strings.partiteHeaderC,
                };
                DbSet<Game> dbset = subs[1] switch
                {
                    "A" => gironeADb.Partite,
                    "B" => gironeBDb.Partite,
                    _ => gironeCDb.Partite,
                };
                foreach (Game g in dbset)
                {
                    p1Lichess = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.P1ID).LichessID;
                    p2Lichess = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.P2ID).LichessID;
                    link = g.Link;
                    res += p1Lichess + " vs " + p2Lichess + "\n" + link + "\n";
                }
            }
            else //player
            {
                string submittedID = subs[1];
                int? playerID = null;
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID) == null)
                {
                    res += Strings.player404 + Strings.errorContact;
                    return res;
                }

                playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).TID;
                Girone checkA = gironeADb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                Girone checkB = gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                Girone checkC = gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == playerID);
                int groupID = (checkA != null) ? 0 : (checkB != null) ? 1 : (checkC != null) ? 2 : -1;

                if(groupID == -1)
                {
                    res += Strings.player404 + Strings.errorContact;
                    return res;
                }
                res += groupID switch {
                    0 => Strings.partiteHeaderA,
                    1 => Strings.partiteHeaderB,
                    _ => Strings.partiteHeaderC,
                };
                DbSet<Game> dbset = groupID switch
                {
                    0 => gironeADb.Partite,
                    1 => gironeBDb.Partite,
                    _ => gironeCDb.Partite,
                };
                string prevRes = res;
                foreach (Game g in dbset)
                {
                    p1Lichess = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.P1ID).LichessID;
                    p2Lichess = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.P2ID).LichessID;
                    link = g.Link;
                    if (p1Lichess.ToLower() == submittedID.ToLower() || p2Lichess.ToLower() == submittedID.ToLower())
                    {
                        res += p1Lichess + " vs " + p2Lichess + "\n" + link + "\n";
                    }
                }
                if (res == prevRes)
                {
                    res = Strings.noGames;
                }
            }
            return res;
        }

        /// <summary>
        /// Shows the list of games the sender has yet to play
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string MiePartiteCommand(string message, string sender)
        {
            //FINAL
            if (IsAdmin(sender)) //Submit whose tg id to use, only if admin and only for testing
            {
                string[] submessage = message.Split(" ");
                if (submessage.Length == 2)
                {
                    sender = submessage[1];
                }
            }
            string res = "";
            Partecipante p = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower());
            if (p == null)
            {
                res += Strings.notRegistered + Strings.errorContact;
                return res;
            }
            string lichessID = p.LichessID;
            int groupID = p.Girone switch {
                "A" => 0,
                "B" => 1,
                "C" => 2,
                "F" => 3,
                _ => -1,
            };
            if (groupID == -1)
            {
                res += Strings.notRegistered + Strings.errorContact;
                return res;
            }
            List<string> opponents = new List<string>();
            string results;
            string[] subresults;

            DbSet<Girone> dbset = groupID switch
            {
                0 => gironeADb.Girone,
                1 => gironeBDb.Girone,
                3 => gironeFDb.Girone,
                _ => gironeCDb.Girone,
            };
            int pGroupID = dbset.SingleOrDefault(g => g.PlayerID == p.TID).GID;
            foreach (Girone g in dbset)
            {
                if (g.PlayerID != p.TID)
                {
                    results = g.Results;
                    subresults = results.Split(",");
                    if (subresults[pGroupID - 1] == "-1")
                    {
                        opponents.Add(partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID);
                    }
                }
            }

            res += "Devi ancora giocare contro:\n";
            for(int i = 0; i < opponents.Count; ++i)
            {
                res += opponents.ElementAt(i);
                res += ", giochi con il ";
                if(pGroupID % 2 == 0)
                {
                    if(i % 2 == 0)
                    {
                        res += "bianco\n";
                    }
                    else
                    {
                        res += "nero\n";
                    }
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        res += "nero\n";
                    }
                    else
                    {
                        res += "bianco\n";
                    }
                }
            }
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
            //FINAL
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
            //FINAL
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
            //FINAL
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
    
        /// <summary>
        /// Pulls the latest player game link and result from Lichess
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private string[] PullLatestResult(string player, string opponent)
        {
            //FINAL
            string[] res = new string[2];
            res[0] = "-1";
            res[1] = "";
            string downloadString;
            int i = 0;
            int j = 0;
            WebClient client = new WebClient();

            downloadString = client.DownloadString("https://lichess.org/@/mattiatrucco/search?perf=6&players.a=" + player + "&players.b=" + opponent + "&sort.field=d&sort.order=desc");

            i = downloadString.IndexOf("<article");
            j = downloadString.IndexOf("article>");
            if(i == -1)
            {
                return res;
            }
            string substring = downloadString[i..j];
            string link = substring.Substring(substring.IndexOf("href") + 7, substring.IndexOf("></a>") - substring.IndexOf("href") - 8);
            int result = substring.IndexOf("<span class=\"loss\">") == -1 ? substring.IndexOf("<span class=\"win\">") == -1 ? -1 : 1 : 0;

            res[1] = "lichess.org/" + link;
            if (result == -1)
            {
                res[0] = "x";
            }
            else
            {
                res[0] = result.ToString();
            }
            return res;
        }
    
        /// <summary>
        /// Get the highest tournament id to assign the next one
        /// </summary>
        /// <returns></returns>
        private int GetMaxTID()
        {
            int max = 0;
            foreach(Partecipante p in partecipantiDb.Partecipanti)
            {
                if(p.TID > max)
                {
                    max = p.TID;
                }
            }
            return max;
        }

        /// <summary>
        /// Get the highest group id to assign the next one
        /// </summary>
        /// <returns></returns>
        private int GetMaxGID(string Group)
        {
            int max = 0;
            DbSet<Girone> dbset = Group switch {
                "A" => gironeADb.Girone,
                "B" => gironeBDb.Girone,
                "F" => gironeFDb.Girone,
                _ => gironeCDb.Girone,
            };
            foreach (Girone g in dbset)
            {
                if (g.GID > max)
                {
                    max = g.GID;
                }
            }
            return max;
        }
    }
}