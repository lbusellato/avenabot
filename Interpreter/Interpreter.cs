using avenabot.DAL;
using Telegram.Bot.Args;
using System.Linq;
using System.Net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using avenabot.Log;
using CoreHtmlToImage;
using System.IO;
using static avenabot.Interpreter.Command;
using avenabot.Models.Gironi;
using avenabot.Models.Partecipanti;
using avenabot.Models.Eliminatorie;

namespace avenabot.Interpreter
{
    public class Interpreter
    {
        private static PartecipantiDbContext partecipantiDb = new PartecipantiDbContext();
        private static GironeADbContext gironeADb = new GironeADbContext();
        private static GironeBDbContext gironeBDb = new GironeBDbContext();
        private static GironeCDbContext gironeCDb = new GironeCDbContext();
        private static GironeFDbContext gironeFDb = new GironeFDbContext();
        private static EliminatorieDbContext eliminatoriaDb = new EliminatorieDbContext();

        private static readonly string[] admin = new string[]
        {
            "lbusellato"
        };
        readonly List<Tuple<CommandMethod, string[]>> commandInit = new List<Tuple<CommandMethod, string[]>>()
        {
            new Tuple<CommandMethod, string[]>(new CommandMethod(StartCommand), 
                new string[]{ Strings.startCommand, Strings.startDescr, Strings.falseString, }),
            new Tuple<CommandMethod, string[]>(new CommandMethod(HelpCommand),
                new string[]{ Strings.helpCommand, Strings.helpDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(PartecipantiCommand), 
                new string[]{ Strings.partecipantiCommand, Strings.partecipantiDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(IscrivimiCommand), 
                new string[]{ Strings.iscrivimiCommand, Strings.iscrivimiDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(RimuoviCommand), 
                new string[]{ Strings.rimuoviCommand, Strings.rimuoviDescr, Strings.trueString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(AggiungiCommand), 
                new string[]{ Strings.aggiungiCommand, Strings.aggiungiDescr, Strings.trueString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(SeedCommand), 
                new string[]{ Strings.seedCommand, Strings.seedDescr, Strings.trueString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(RisultatiCommand), 
                new string[]{ Strings.risultatiCommand, Strings.risultatiDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(ClassificaCommand), 
                new string[]{ Strings.classificaCommand, Strings.classificaDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(InserisciCommand), 
                new string[]{ Strings.inserisciCommand, Strings.inserisciDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(TorneoCommand), 
                new string[]{ Strings.torneoCommand, Strings.torneoDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(PartiteCommand), 
                new string[]{ Strings.partiteCommand, Strings.partiteDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(MiePartiteCommand), 
                new string[]{ Strings.miePartiteCommand, Strings.miePartiteDescr, Strings.falseString,}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(NoCommand),
                new string[]{ "", "", Strings.falseString,}), //NoCommand ****MUST**** be the last command in this list
        };

        public struct Standing
        {
            public string ID { get; set; }
            public string[] Games { get; set; }
            public double Tot { get; set; }
        }

        private static Random rng = new Random();
        private static readonly DateTime finalsDate = new DateTime(2021, 1, 1, 12, 0, 0); //Change this to select when to switch to the final group
        private static readonly DateTime endDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to select when to end the tournament group
        private static readonly DateTime closingDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to select when to close registering
        private static DateTime lastCommand;
        public static int coolDown = 0;
        static readonly bool GroupFinals = false; //True for a final group final, false for a knockout final
        static readonly int BestOf = 3; //n° of games for knockout rounds
        static readonly int FinalBestOf = 5; //n° of games for final knockout round
        static readonly int MaxPlayers = 8;
        static readonly int MaxGroups = 2;
        static readonly int MaxFinalists = 2;

        static Command[] commandList;

        public Interpreter() {
            commandList = new Command[commandInit.Count];
            int id = commandList.Length;
            for(int i = 0; i < commandList.Length; ++i)
            {
                commandList[i] = 
                    new Command(commandInit[i].Item1, 
                    i, 
                    commandInit[i].Item2[0], 
                    commandInit[i].Item2[1], 
                    Convert.ToBoolean(commandInit[i].Item2[2]));
            }
        }

        public string Parse(MessageEventArgs e, DateTime LastCommand)
        {
            lastCommand = LastCommand;
            partecipantiDb = new PartecipantiDbContext();
            gironeADb = new GironeADbContext();
            gironeBDb = new GironeBDbContext();
            gironeCDb = new GironeCDbContext();
            gironeFDb = new GironeFDbContext();
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            string command = message.Split(" ")[0];
            int inlineCheck = command.IndexOf("@"); //Trim the @ part if the command was sent like this: /command@AvenaChessBot
            if(inlineCheck != -1)
            {
                command = command.Substring(0, inlineCheck);
            }
            string res = commandList[Find(command)].Execute(message, sender);
            gironeADb.Dispose();
            gironeBDb.Dispose();
            gironeCDb.Dispose();
            gironeFDb.Dispose();
            partecipantiDb.Dispose();
            return res;
        }

        /// <summary>
        /// Display the welcome message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string StartCommand(string message, string sender)
        {
            return Strings.welcomeMsg; //Show the welcome message
        }

        /// <summary>
        /// Shows the list of commands available to the user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string HelpCommand(string message, string sender)
        {
            string res = "";
            foreach (Command c in commandList.Skip(1))
            {
                if(c.name == "")
                {
                    continue;
                }
                //Show the command's name and description only if the sender is admin or it's not an admin only command
                if (!c.admin || IsAdmin(sender))
                {
                    res += c.name + c.descr + "\n";
                    //Add a separator to nicely format the list
                    for (int j = 0; j < 50; ++j)
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
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string PartecipantiCommand(string message, string sender)
        {
            string html = "<div><table border=\"1\" cellspacing=\"0\" cellpadding=\"4\" align=\"center\"><tr>" +
                "<th>ID</th><th>ID Lichess</th><th>ID Telegram</th><th>ELO</th><th>Var.ELO</th><th>Girone</th></tr>";
            //Update the elos
            if (partecipantiDb.Partecipanti.Count() > 0)
            {
                UpdateElos();
            }
            //Pull each player's data from the db and nicely format it
            foreach (Partecipante p in partecipantiDb.Partecipanti)
            {
                html += "<tr align>";
                html += "<td align=\"center\">" + p.TID + "</td>";
                html += "<td>" + p.LichessID + "</td>";
                html += "<td>@" + p.TGID + "</td>";
                html += "<td align=\"right\">" + p.ELO + "</td>";
                html += "<td align=\"center\">" + ((p.ELOvar > 0) ? "+" : (p.ELOvar < 0) ? "-" : "") + p.ELOvar + "</td>";
                html += "<td align=\"center\">" + p.Girone + "</td>";
                html += "</tr>";
            }
            Render(html, 0, "partecipanti.png", 60);
            return "partecipanti";
        }

        /// <summary>
        /// Registers the player with the Lichess ID provided in message, checking if registration closing time or max player 
        /// number have been reached
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string IscrivimiCommand(string message, string sender)
        {
            string res = "";
            string lichessID = "";
            string[] subs;
            int playerCount = GetPlayerCount();
            int elo = -1;

            if (DateTime.Now > closingDate || playerCount <= MaxPlayers)
            {
                res = Strings.closedRegistrations;
                return res;
            }

            subs = message.Split(" ");
            if (subs.Length < 2) //If no ID was provided
            {
                res = Strings.iscrivimiUsage;
                return res;
            }
            //Get the Lichess ID
            lichessID = subs[1];
            //Fetch the player's ELO from Lichess
            elo = GetELO(lichessID);

            if (sender == null) //If the user doesn't have a Telegram username
            {
                res = Strings.usernameNeeded;
                return res;
            }

            //If the player is already registered
            if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == sender) != null) 
            {
                res = Strings.registeredError + Strings.checkPartecipanti + Strings.errorContact;
                return res;
            }

            if (elo == -1) //If the player wasn't found on Lichess
            {
                res = Strings.lichess404 + Strings.errorContact;
                return res;
            }

            int tid = GetMaxTID() + 1; //Get a valid tournament ID

            lichessID = GetCorrectLichessID(lichessID); //Retrieve the Lichess ID with proper capitalization

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
        private static string RimuoviCommand(string message, string sender)
        {
            string res = "";
            string lichessID = "";
            string[] subs;
            int elo = -1;
            if (IsAdmin(sender))
            {
                subs = message.Split(" ");
                if (subs.Length < 2) //If no ID was provided
                {
                    res = Strings.rimuoviUsage;
                    return res;
                }
                //Get the Lichess ID
                lichessID = subs[1];
                //Fetch the player's ELO from Lichess
                elo = GetELO(lichessID);
                //Check if the player was found on Lichess
                if (elo == -1)
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
                foreach (Partecipante par in partecipantiDb.Partecipanti)
                {
                    if (par.TID > removedTID)
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
        /// Add the player with the specified Lichess ID and Telegram Handle (admin only)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string AggiungiCommand(string message, string sender)
        {
            string res = "";
            string lichessID = "";
            string tgID = "";
            string[] subs;
            int elo = -1;
            if (IsAdmin(sender))
            {
                subs = message.Split(' ');
                if (subs.Length != 3) //If the number of arguments is not the correct one
                {
                    res = Strings.aggiungiUsage;
                    return res;
                }

                //Get the Lichess ID
                lichessID = subs[1];
                //Get the Telegram ID
                tgID = subs[2];
                //Fetch the player's ELO from Lichess
                elo = GetELO(lichessID);
                //Check if the player exists on Lichess
                if (elo == -1)
                {
                    res = Strings.lichess404 + Strings.errorContact;
                    return res;
                }
                //Check if the player is already in the DB
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID == tgID) != null) 
                {
                    res = Strings.registeredAdminError + Strings.checkPartecipanti + Strings.errorContact;
                    return res;
                }

                int tid = GetMaxTID() + 1; //Get a valid tournament ID
                //Push the new player to the db
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
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string SeedCommand(string message, string sender)
        {
            string res = "";
            //Seed the final group if it's past the date
            if (GroupFinals)
            {
                if (DateTime.Now > finalsDate)
                {
                    //Check if the group was already seeded
                    int checkF = gironeFDb.Girone.Count();
                    if (checkF > 0)
                    {
                        res += Strings.finalGroupAlreadySeeded;
                        return res;
                    }
                    //Pull the results from the preliminary groups
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
                        //Calculate the total points for each player
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
                        //Sort the list by the total score and pull only the top players
                        standings = standings.OrderByDescending(s => s.Tot).ToList();
                        int id;
                        int cnt = 0;
                        foreach (Standing s in standings)
                        {
                            if (cnt < MaxFinalists)
                            {
                                cnt++;
                                id = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == s.ID.ToLower()).TID;
                                Girone g = j switch
                                {
                                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                };
                                finalPlayers.Add(g);
                            }
                        }
                    }
                    //Generate the results string
                    string resultsDummy = "";
                    for (int j = 0; j < finalPlayers.Count; ++j)
                    {
                        resultsDummy += "-1";
                        if (j != finalPlayers.Count - 1)
                        {
                            resultsDummy += ",";
                        }
                    }
                    //Push the finalists list to the db
                    int gid = 1;
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
                //Check if the DBs are empty
                if (gironeADb.Girone.Count() > 0)
                {
                    res += Strings.groupsAlreadySeeded;
                    return res;
                }
                //Check if the player list has enough elements
                if (GetPlayerCount() < MaxPlayers)
                {
                    res += Strings.notEnoughPlayers;
                    return res;
                }
                //Pull the list of players and sort it by ELO
                List<Partecipante> players = new List<Partecipante>();
                foreach (Partecipante p in partecipantiDb.Partecipanti)
                {
                    if (players.Count < MaxPlayers)
                    {
                        players.Add(p);
                    }
                }
                players = players.OrderBy(p1 => p1.ELO).ToList();

                //Divide evenly the player list
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
                        //Assign a group to each player
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
                        //Generate the results string
                        string resultsDummy = "";
                        for (int j = 0; j < groupPlayers.Count; ++j)
                        {
                            resultsDummy += "-1";
                            if (j != groupPlayers.Count - 1)
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
                        //Push each player to the correct db
                        switch (i)
                        {
                            case 0:
                                gid = GetMaxGID("A") + 1;
                                g.GID = gid;
                                gironeADb.Girone.Add(g);
                                break;
                            case 1:
                                gid = GetMaxGID("B") + 1;
                                g.GID = gid;
                                gironeBDb.Girone.Add(g);
                                break;
                            default:
                                gid = GetMaxGID("C") + 1;
                                g.GID = gid;
                                gironeCDb.Girone.Add(g);
                                break;
                        }
                    }
                    //Save the changes to the dbs
                    gironeADb.SaveChanges();
                    gironeBDb.SaveChanges();
                    gironeCDb.SaveChanges();
                }
                res += Strings.groupsSeeded;
            }
            else
            {
                if (DateTime.Now > finalsDate)
                {
                    //Pull the results from the preliminary groups
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
                        //Calculate the total points for each player
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
                        //Sort the list by the total score and pull only the top players
                        standings = standings.OrderByDescending(s => s.Tot).ToList();
                        int id;
                        int cnt = 0;
                        foreach (Standing s in standings)
                        {
                            if (cnt < MaxFinalists / 2)
                            {
                                cnt++;
                                id = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == s.ID.ToLower()).TID;
                                Girone g = j switch
                                {
                                    0 => gironeADb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                    1 => gironeBDb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                    _ => gironeCDb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                };
                                finalPlayers.Add(g);
                            }
                        }
                    }
                    List<int> eids = new List<int>();
                    string resultsDummy = "";
                    int k = 0;
                    switch (MaxFinalists)
                    {
                        case 2:
                            DbSet<Finale> finale = eliminatoriaDb.Finale;
                            if (finale.Count() > 0)
                            {
                                res += Strings.knockGroupAlreadySeeded;
                                return res;
                            }
                            //Generate the results string
                            resultsDummy = "";
                            for (int j = 0; j < FinalBestOf; ++j)
                            {
                                resultsDummy += "-1";
                                if (j != FinalBestOf - 1)
                                {
                                    resultsDummy += ",";
                                }
                            }
                            for (int i = 0; i < MaxFinalists; ++i)
                            {
                                eids.Add(i);
                            }
                            k = 0;
                            //Push the players into the db
                            foreach (Girone g in finalPlayers)
                            {
                                Finale f = new Finale
                                {
                                    PlayerID = g.PlayerID,
                                    Results = resultsDummy,
                                    OpponentEID = 0,
                                    EID = eids.ElementAt(k),
                                };
                                k++;
                                finale.Add(f);
                            }
                            eliminatoriaDb.SaveChanges();
                            //Randomly assign an opponent to each player
                            foreach (Finale f in finale)
                            {
                                int eid = eids.ElementAt(rng.Next(eids.Count()));
                                while(f.EID == eid)
                                {
                                    eid = eids.ElementAt(rng.Next(eids.Count()));
                                }
                                f.OpponentEID = eid;
                                eids.Remove(eid);
                            }
                            eliminatoriaDb.SaveChanges();
                            break;
                        case 4:
                            DbSet<Semifinali> semifinali = eliminatoriaDb.Semifinali;
                            if (semifinali.Count() > 0)
                            {
                                res += Strings.knockGroupAlreadySeeded;
                                return res;
                            }
                            //Generate the results string
                            resultsDummy = "";
                            for (int j = 0; j < BestOf; ++j)
                            {
                                resultsDummy += "-1";
                                if (j != BestOf - 1)
                                {
                                    resultsDummy += ",";
                                }
                            }
                            for (int i = 0; i < MaxFinalists; ++i)
                            {
                                eids.Add(i);
                            }
                            k = 0;
                            //Push the players into the db
                            foreach (Girone g in finalPlayers)
                            {
                                Semifinali s = new Semifinali
                                {
                                    PlayerID = g.PlayerID,
                                    Results = resultsDummy,
                                    OpponentEID = 0,
                                    EID = eids.ElementAt(k),
                                };
                                k++;
                                semifinali.Add(s);
                            }
                            eliminatoriaDb.SaveChanges();
                            //Randomly assign an opponent to each player
                            foreach (Semifinali s in semifinali)
                            {
                                int eid = eids.ElementAt(rng.Next(eids.Count()));
                                while (s.EID == eid)
                                {
                                    eid = eids.ElementAt(rng.Next(eids.Count()));
                                }
                                s.OpponentEID = eid;
                                eids.Remove(eid);
                            }
                            eliminatoriaDb.SaveChanges();
                            break;
                        case 8:
                            DbSet<Quarti> quarti = eliminatoriaDb.Quarti;
                            if (quarti.Count() > 0)
                            {
                                res += Strings.knockGroupAlreadySeeded;
                                return res;
                            }
                            //Generate the results string
                            resultsDummy = "";
                            for (int j = 0; j < BestOf; ++j)
                            {
                                resultsDummy += "-1";
                                if (j != BestOf - 1)
                                {
                                    resultsDummy += ",";
                                }
                            }
                            for (int i = 0; i < MaxFinalists; ++i)
                            {
                                eids.Add(i);
                            }
                            k = 0;
                            //Push the players into the db
                            foreach (Girone g in finalPlayers)
                            {
                                Quarti q = new Quarti
                                {
                                    PlayerID = g.PlayerID,
                                    Results = resultsDummy,
                                    OpponentEID = 0,
                                    EID = eids.ElementAt(k),
                                };
                                k++;
                                quarti.Add(q);
                            }
                            eliminatoriaDb.SaveChanges();
                            //Randomly assign an opponent to each player
                            foreach (Quarti q in quarti)
                            {
                                int eid = eids.ElementAt(rng.Next(eids.Count()));
                                while (q.EID == eid)
                                {
                                    eid = eids.ElementAt(rng.Next(eids.Count()));
                                }
                                q.OpponentEID = eid;
                                eids.Remove(eid);
                            }
                            eliminatoriaDb.SaveChanges();
                            break;
                        default:
                            return ""; 
                    }
                    res += Strings.knockoutsSeeded;
                }
            }
            return res;
        }

        /// <summary>
        /// Shows the games results of a player, group or all groups
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string RisultatiCommand(string message, string sender)
        {
            string res = "";
            string html = "";
            string[] subs;
            //Avoid spamming the results picture
            if (DateTime.Now < lastCommand.AddMinutes(coolDown))
            {
                return res;
            }
            //Check if the dbs are populated
            if (gironeADb.Girone.Count() <= 0 || 
                (DateTime.Now > finalsDate && gironeFDb.Girone.Count() <= 0) ||
                (DateTime.Now > finalsDate && eliminatoriaDb.Finale.Count() <= 0 && MaxFinalists == 2) ||
                (DateTime.Now > finalsDate && eliminatoriaDb.Semifinali.Count() <= 0 && MaxFinalists == 4) ||
                (DateTime.Now > finalsDate && eliminatoriaDb.Quarti.Count() <= 0 && MaxFinalists == 8))
            {
                res += Strings.notYetSeededGroups;
                return res;
            }
            lastCommand = DateTime.Now;

            subs = message.Split(' ');
            if (GroupFinals)
            {
                if (subs.Length == 1) //All groups
                {
                    if (DateTime.Now < finalsDate)
                    {
                        for (int j = 0; j < MaxGroups; ++j)
                        {
                            html += FetchGroupResults(j);
                        }
                        Render(html, 5);
                    }
                    else
                    {
                        html = FetchGroupResults(3);
                        Render(html, 3);
                    }
                    partecipantiDb.SaveChanges();
                    return "mostragironi";
                }
                else if (subs.Length == 2) //Single group
                {
                    switch (subs[1].ToUpper())
                    {
                        case "A":
                            html = FetchGroupResults(0);
                            Render(html, 0);
                            partecipantiDb.SaveChanges();
                            return "mostragironeA";
                        case "B":
                            html = FetchGroupResults(1);
                            Render(html, 1);
                            partecipantiDb.SaveChanges();
                            return "mostragironeB";
                        case "C":
                            if (MaxGroups != 3)
                            {
                                res += Strings.saywhat;
                                return res;
                            }
                            html = FetchGroupResults(2);
                            Render(html, 2);
                            partecipantiDb.SaveChanges();
                            return "mostragironeC";
                        case "F":
                            if (DateTime.Now < finalsDate)
                            {
                                res += Strings.saywhat;
                                return res;
                            }
                            html = FetchGroupResults(3);
                            Render(html, 3);
                            partecipantiDb.SaveChanges();
                            return "mostragironeF";
                        default:
                            res = Strings.risultatiUsage;
                            return res;
                    }
                }
                else //Wrong usage
                {
                    res = Strings.risultatiUsage;
                    return res;
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Blanket method to fetch the specified group results
        /// </summary>
        /// <param name="Group"></param>
        /// <returns></returns>
        private static string FetchGroupResults(int Group)
        {
            string results;
            string[] subresults;
            //The html code generation is pretty straightforward, we simply make a table inside a div
            string html = "<div><table border=\"1\" cellspacing=\"0\" cellpadding=\"4\" align=\"center\"><tr><b>Risultati girone ";
            html += Group switch
            {
                0 => "A",
                1 => "B",
                3 => "finale",
                _ => "C",
            };
            //Proceed only if the dbs are populated
            if (gironeADb.Girone.Count() > 0)
            {
                DbSet<Girone> dbset = Group switch
                {
                    0 => gironeADb.Girone,
                    1 => gironeBDb.Girone,
                    3 => gironeFDb.Girone,
                    _ => gironeCDb.Girone,
                };

                html += "<tr><td></td>";
                for (int i = 0; i < dbset.Count(); ++i)
                {
                    int pid = dbset.SingleOrDefault(g => g.GID == i + 1).PlayerID;
                    string lid = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == pid).LichessID;
                    html += "<td>" + pid + "</td>";
                }
                html += "</tr>";
                int k = 0;
                foreach (Girone g in dbset)
                {
                    results = g.Results;
                    subresults = results.Split(',');
                    html += (k % 2 == 0) ?
                        "<tr bgcolor=\"#e9ede4\">" :
                        "<tr bgcolor=\"#d7edb4\">";
                    k++;
                    html += "<td>(" + g.PlayerID + ") " 
                         + partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID + "</td>";
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        html += "<td align=\"center\">";
                        if (subresults[i] == "x")
                        {
                            html += "&#189";
                        }
                        else if (subresults[i] != "-1")
                        {
                            html += subresults[i];
                        }
                        else if (g.GID == i + 1)
                        {
                            html += "x";
                        }
                        html += "</td>";
                    }
                    html += "</tr>";
                }
                html += "</table></div>";
            }
            else
            {
                html = null;
            }
            return html;
        }

        /// <summary>
        /// Shows the current groups standings
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string ClassificaCommand(string message, string sender)
        {
            string res = "";
            string html = "";
            string[] subresults;
            int dbCheck;
            DbSet<Girone> dbset;
            //Check if the cooldown has passed
            if (DateTime.Now < lastCommand.AddMinutes(coolDown))
            {
                return res;
            }
            //Check if the dbs are empty
            if (gironeADb.Girone.Count() <= 0 ||  (gironeFDb.Girone.Count() <= 0 && DateTime.Now > finalsDate))
            {
                res += Strings.notYetSeededStandings;
                return res;
            }
            lastCommand = DateTime.Now;

            //Pretty straightforward html table generation
            html += "<div>";
            for (int j = 0; j < MaxGroups + 1; ++j)
            {
                if(j == 0 && DateTime.Now < finalsDate )
                {
                    continue;
                }
                html += "<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\" align=\"center\"><tr><b>Classifica girone ";
                html += j switch
                {
                    0 => "finale",
                    1 => "A",
                    2 => "B",
                    _ => "C",
                };
                html += ":</b></tr>";
                dbCheck = j switch
                {
                    0 => gironeFDb.Girone.Count(),
                    1 => gironeADb.Girone.Count(),
                    2 => gironeBDb.Girone.Count(),
                    _ => gironeCDb.Girone.Count(),
                };
                if (dbCheck <= 0)
                {
                    res += Strings.errorNotSeeded;
                    return res;
                }
                dbset = j switch
                {
                    0 => gironeFDb.Girone,
                    1 => gironeADb.Girone,
                    2 => gironeBDb.Girone,
                    _ => gironeCDb.Girone,
                };

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
                        else if (subresults[i] == "1")
                        {
                            stg.Games[i] = subresults[i];
                            stg.Tot += 1;
                        }
                        else if (subresults[i] == "0")
                        {
                            stg.Games[i] = subresults[i];
                        }
                        else if (subresults[i] == "-1" && i != g.GID - 1)
                        {
                            stg.Games[i] = subresults[i];
                        }
                    }
                    standings.Add(stg);
                }
                standings = standings.OrderByDescending(s => s.Tot).ToList();

                html += "<th>ID</th>";
                for (int i = 1; i < MaxPlayers / MaxGroups; ++i)
                {
                    html += "<th>G" + i + "</th>";
                }

                int k = 0;
                int cnt;
                html += "<th>PTI</th>";
                foreach (Standing s in standings)
                {
                    cnt = 0;
                    res += s.ID;
                    html += (k % 2 == 0) ?
                        "<tr bgcolor=\"#e9ede4\"><td>" + s.ID + "</td>" :
                        "<tr bgcolor=\"#d7edb4\"><td>" + s.ID + "</td>";
                    k++;
                    for (int i = 0; i < s.Games.Length; ++i)
                    {
                        if (s.Games[i] != null)
                        {
                            if (s.Games[i] == "-1")
                            {
                                cnt++;
                                continue;
                            }
                            else
                            {
                                html += "<td align=\"center\">" + s.Games[i] + "</td>";
                            }
                        }
                    }
                    for(int i = 0; i < cnt; ++i)
                    {
                        html += "<td> </td>";
                    }
                    if (s.Tot % 1 != 0)
                    {
                        if ((int)s.Tot == 0)
                        {
                            html += "<td align=\"center\">&#189</td>";
                        }
                        else
                        {
                            html += "<td align=\"center\">" + (int)s.Tot + "&#189</td>";
                        }
                    }
                    else
                    {
                        html += "<td align=\"center\">" + s.Tot + "</td>";
                    }
                    html += "</tr>";
                }
                if(DateTime.Now > finalsDate)
                {
                    break;
                }
            }
            html += "</table></div>";
            Render(html, 4);
            res = "mostraclassifica";
            return res;
        }

        /// <summary>
        /// Adds the specified player to the DB, if it doesn't already exist (Admin only)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string InserisciCommand(string message, string sender)
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
            Partecipante p1;
            Partecipante p2;
            int groupID;
            string sub1;
            string sub2;
            subs = message.Split(" ");
            //Check if the db is populated 
            int dbCheckA = gironeADb.Girone.Count();
            if (dbCheckA <= 0)
            {
                res += Strings.errorNotSeeded;
                return res;
            }
            if (subs.Length != 4 && subs.Length != 2)
            {
                res += Strings.inserisciUsage;
                return res;
            }
            if (subs.Length == 2) // /inserisci OpponentID
            {
                sub1 = subs[1];
                sub1.Trim('@');
                //Check if the players are in the same group first
                p1 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower());
                p2 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                if(p1 == p2)
                {
                    res += Strings.inserisciUsage;
                    return res;
                }
                //Check if a Telegram ID was sent instead of a Lichess ID
                if(p2 == null)
                {
                    p2 = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sub1.ToLower());
                }
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
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string TorneoCommand(string message, string sender)
        {
            return Strings.tournamentInfo;
        }

        /// <summary>
        /// Shows list of played games
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string PartiteCommand(string message, string sender)
        {
            string res = "";
            string[] subs = message.Split(" ");
            string p1Lichess;
            string p2Lichess;
            string link;
            //Check for correct usage
            if (subs.Length != 2)
            {
                return Strings.partiteUsage;
            }
            //Check if the cooldown has passed
            if (DateTime.Now < lastCommand.AddMinutes(coolDown))
            {
                return "";
            }
            if(subs[1].Length == 1) // /partite (A B o C)
            {
                subs[1] = subs[1].ToUpper();
                int check = subs[1] switch
                {
                    "A" => gironeADb.Partite.Count(),
                    "B" => gironeBDb.Partite.Count(),
                    "C" => gironeCDb.Partite.Count(),
                    "F" => gironeFDb.Partite.Count(),
                    _ => -2
                };

                if(check == -2)
                {
                    return Strings.notValidGroup;
                }
                else if(check <= 0)
                {
                    return Strings.notYetPlayedGames;
                }
                if(subs[1] == "F")
                {
                    res += Strings.partiteHeader + "finale:\n";
                }
                else
                {
                    res += Strings.partiteHeader + subs[1] + ":\n";
                }
                DbSet<Game> dbset = subs[1] switch
                {
                    "A" => gironeADb.Partite,
                    "B" => gironeBDb.Partite,
                    "F" => gironeFDb.Partite,
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
                //Check if the player exists
                if (partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID) == null)
                {
                    res += Strings.player404 + Strings.errorContact;
                    return res;
                }
                //Pull the player's tournament and group id
                playerID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).TID;
                int groupID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).Girone switch {
                    "A" => 0,
                    "B" => 1,
                    "C" => 2,
                    "F" => 3,
                    _ => -1
                };
                //Check if the player is in the tournament
                if(groupID == -1)
                {
                    res += Strings.player404 + Strings.errorContact;
                    return res;
                }
                //Check if the dbs are populated
                if (gironeADb.Partite.Count() <= 0 || (DateTime.Now > finalsDate && gironeFDb.Partite.Count() > 0))
                {
                    res += Strings.notYetPlayedGames;
                    return res;
                }

                string senderLichessID = partecipantiDb.Partecipanti.SingleOrDefault(p => p.TID == playerID).LichessID;
                res += "Partite di " + senderLichessID + ":\n";
                DbSet<Game> dbset = groupID switch
                {
                    0 => gironeADb.Partite,
                    1 => gironeBDb.Partite,
                    3 => gironeFDb.Partite,
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
                //If no games were found, they have all already been played
                if (res == prevRes)
                {
                    res = Strings.noGamesToPlay;
                }
            }
            return res;
        }

        /// <summary>
        /// Shows the list of games the sender has yet to play
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string MiePartiteCommand(string message, string sender)
        {
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
            string dummy = null;
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
                    else
                    {
                        opponents.Add(dummy);
                    }
                }
            }
            bool flag = false;
            foreach(string s in opponents)
            {
                if(s != null)
                {
                    flag = true;
                }
            }
            if(!flag)
            {
                res += Strings.noGamesToPlay;
                return res;
            }

            res += "Devi ancora giocare contro:\n";
            string opponentTG;
            for (int i = 0; i < opponents.Count; ++i)
            {
                if (opponents.ElementAt(i) != null)
                {
                    res += opponents.ElementAt(i);
                    res += "(@";
                    opponentTG = opponents.ElementAt(i);
                    res += partecipantiDb.Partecipanti.SingleOrDefault(p => p.LichessID == opponentTG).TGID + ")";
                    res += ", giochi con il ";
                    if (pGroupID % 2 == 0)
                    {
                        if (i % 2 == 0)
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
            }
            return res;
        }

        /// <summary>
        /// Updates the ELOs of each player registered
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static void UpdateElos()
        {
            foreach(Partecipante p in partecipantiDb.Partecipanti)
            {
                //Simply pull the elo from both the db and Lichess and compare them
                int elo = GetELO(p.LichessID);
                int var = elo - p.ELO;
                if (var != 0)
                {
                    p.ELO = elo;
                    p.ELOvar = var;
                }
            }
            //Save the changes to the db
            partecipantiDb.SaveChanges();
        }

        /// <summary>
        /// Responds to anything other than a known command
        /// </summary>
        /// <returns></returns>
        private static string NoCommand(string message, string sender)
        {
            return (message.ToLower().IndexOf(Strings.invalidMessage) != -1) ?  Strings.errorInvalidMessage : "";
        }

        /// <summary>
        /// Counts all the players registered
        /// </summary>
        /// <returns></returns>
        private static int GetPlayerCount()
        {
            return partecipantiDb.Partecipanti.Count();
        }

        /// <summary>
        /// Finds the index of the provided command in the array commandList
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static int Find(string command)
        {
            for(int i = 0; i < commandList.Length; ++i)
            {
                if (commandList[i].name == command)
                    return i;
            }
            return commandList.Length - 1;
        }

        /// <summary>
        /// Pulls the provided player's rpaid ELO from Lichess
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int GetELO(string player)
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
                Logger.Log("Giocatore non trovato su Lichess, eccezione generata:" + e);
                return -1;
            }   
        }

        /// <summary>
        /// Check if the message sender is an admin
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private static bool IsAdmin(string username)
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
        private static string[] PullLatestResult(string player, string opponent)
        {
            string[] res = new string[2];
            res[0] = "-1";
            res[1] = "";
            string downloadString;
            int i = 0;
            int j = 0;
            WebClient client = new WebClient();

            try
            {
                downloadString = client.DownloadString("https://lichess.org/@/" + player + "/all");

                i = downloadString.IndexOf("<article");
                j = downloadString.IndexOf("article>");
                if (i == -1 || j == -1)
                {
                    return res;
                }

                string substring = downloadString[i..j];

                int opponentCheck = downloadString.IndexOf(opponent);
                if (opponentCheck == -1)
                {
                    return res;
                }

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
            catch(WebException e)
            {
                Logger.Log("Giocatore non trovato su Lichess, eccezione generata:" + e);
                return res;
            }
        }
    
        /// <summary>
        /// Get the highest tournament id to assign the next one
        /// </summary>
        /// <returns></returns>
        private static int GetMaxTID()
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
        private static int GetMaxGID(string Group)
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

        /// <summary>
        /// Try and get the provided Lichess ID with proper capitalization
        /// </summary>
        /// <param name="lichessID"></param>
        /// <returns></returns>
        private static string GetCorrectLichessID(string lichessID)
        {
            string res = lichessID;
            WebClient client = new WebClient();
            try
            {
                string downloadString = client.DownloadString("https://lichess.org/@/" + lichessID.ToLower());
                int i = downloadString.IndexOf("<title>") + 7;
                int j = downloadString.IndexOf(" : Activity ");
                if (i != -1 && j != -1)
                {
                    res = downloadString[i..j];
                }
                return res;
            }
            catch (WebException e)
            {
                Logger.Log("Giocatore non trovato su Lichess, eccezione generata:" + e);
                return res;
            }
        }

        /// <summary>
        /// Render the provided html source, to a specific file and a specific width if needed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="file"></param>
        /// <param name="width"></param>
        public static void Render(string source, int n, string file = "", int width = 50)
        {
            var converter = new HtmlConverter();
            byte[] bytes;
            if(file == "")
            {
                bytes = converter.FromHtmlString(source, width * (int)(MaxPlayers / MaxGroups), ImageFormat.Png);
                file = n switch
                {
                    0 => "gironeA.png",
                    1 => "gironeB.png",
                    2 => "gironeC.png",
                    3 => "gironeF.png",
                    4 => "classifica.png",
                    _ => "gironi.png"
                };
            }
            else
            {
                bytes = converter.FromHtmlString(source, (int)(20 * width / 3), ImageFormat.Png);
            }
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.WriteAllBytes(file, bytes);
        }
    }
}