using avenabot.DAL;
using avenabot.Log;
using avenabot.Models.Eliminatorie;
using avenabot.Models.Gironi;
using avenabot.Models.Membri;
using avenabot.Models.Partecipanti;
using CoreHtmlToImage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Telegram.Bot.Args;
using static avenabot.Interpreter.Command;

namespace avenabot.Interpreter
{
    public class Interpreter
    {
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
            new Tuple<CommandMethod, string[]>(new CommandMethod(TabelloneCommand),
                new string[]{ Strings.tabelloneCommand, Strings.tabelloneDescr, Strings.falseString, Strings.falseString}),
            new Tuple<CommandMethod, string[]>(new CommandMethod(NoCommand),
                new string[]{ "", "", Strings.falseString,}), //NoCommand ****MUST**** be the last command in this list
        };

        public struct Standing
        {
            public string ID { get; set; }
            public string[] Games { get; set; }
            public double Tot { get; set; }
        }

        private static readonly Random rng = new Random();
        private static readonly DateTime finalsDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to select when to switch to the final group
        private static readonly DateTime endDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to select when to end the tournament group
        private static readonly DateTime closingDate = new DateTime(2021, 12, 1, 12, 0, 0); //Change this to select when to close registering
        private static DateTime lastCommand;
        public static int coolDown = 0;
        static readonly bool GroupFinals = true; //True for a final group final, false for a knockout final
        static readonly int BestOf = 5; //n° of games for knockout rounds
        static readonly int MaxPlayers = 8;
        static readonly int MaxGroups = 2;
        static readonly int MaxFinalists = 8;

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
            string message = e.Message.Text;
            string sender = e.Message.From.Username;
            string command = message.Split(" ")[0];
            int inlineCheck = command.IndexOf("@"); //Trim the @ part if the command was sent like this: /command@AvenaChessBot
            if(inlineCheck != -1)
            {
                command = command.Substring(0, inlineCheck);
            }
            string res = commandList[Find(command)].Execute(message, sender);
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
                if (c.enabled && !c.admin || IsAdmin(sender))
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            string html = "<div><table border=\"1\" cellspacing=\"0\" cellpadding=\"4\" align=\"center\"><tr>" +
                "<th>ID</th><th>ID Lichess</th><th>ID Telegram</th><th>ELO</th><th>Var.ELO</th><th>Girone</th></tr>";
            //Pull each player's data from the db and nicely format it
            foreach (Partecipante p in pdb.Partecipanti)
            {
                html += "<tr align>";
                html += "<td align=\"center\">" + p.TID + "</td>";
                html += "<td>" + p.LichessID + "</td>";
                html += "<td>@" + p.TGID + "</td>";
                html += "<td align=\"right\">" + p.ELO + "</td>";
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
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

            if (sender == null) //If the user doesn't have a Telegram username
            {
                res = Strings.usernameNeeded;
                return res;
            }

            //If the player is already registered
            if (pdb.Partecipanti.SingleOrDefault(p => p.TGID == sender) != null) 
            {
                res = Strings.registeredError + Strings.checkPartecipanti + Strings.errorContact;
                return res;
            }

            int tid = GetMaxTID() + 1; //Get a valid tournament ID

            lichessID = GetCorrectLichessID(lichessID); //Retrieve the Lichess ID with proper capitalization
            elo = GetELO(lichessID);
            //Create the player and push him into the db
            Partecipante p = new Partecipante
            {
                LichessID = lichessID,
                TGID = sender,
                ELO = elo,
                TID = tid
            };
            pdb.Partecipanti.Add(p);
            pdb.SaveChanges();
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            string res = "";
            string lichessID = "";
            string[] subs;
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

                //Remove the player from the DB, if he exists
                if (pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower()) == null)
                {
                    res = Strings.player404 + Strings.checkPartecipanti + Strings.errorContact;
                    return res;
                }
                
                //Find and remove the player from the db
                Partecipante p = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == lichessID.ToLower());
                int removedTID = p.TID;
                pdb.Partecipanti.Attach(p);
                pdb.Partecipanti.Remove(p);

                //Update the TIDs of other players
                foreach (Partecipante par in pdb.Partecipanti)
                {
                    if (par.TID > removedTID)
                    {
                        par.TID -= 1;
                    }
                }
                pdb.SaveChanges();
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
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
                //Check if the player is already in the DB
                if (pdb.Partecipanti.SingleOrDefault(p => p.TGID == tgID) != null) 
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
                pdb.Partecipanti.Add(p);
                pdb.SaveChanges();
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
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
            string res = "";
            //Seed the final group if it's past the date
            if (GroupFinals)
            {
                if (DateTime.Now > finalsDate)
                {
                    //Check if the group was already seeded
                    int checkF = fdb.Girone.Count();
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
                            0 => adb.Girone,
                            _ => bdb.Girone,
                        };
                        standings = new List<Standing>();
                        //Calculate the total points for each player
                        foreach (Girone g in dbset)
                        {
                            Standing stg = new Standing
                            {
                                ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID
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
                                id = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == s.ID.ToLower()).TID;
                                Girone g = j switch
                                {
                                    0 => adb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                    _ => bdb.Girone.SingleOrDefault(g => g.PlayerID == id),
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
                        pdb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).Girone = "F";
                        pdb.SaveChanges();
                        g.GID = gid;
                        gid++;
                        g.Results = resultsDummy;
                        fdb.Girone.Add(g);
                        fdb.SaveChanges();
                    }
                    res += Strings.finalGroupSeeded;
                    return res;
                }
                //Check if the DBs are empty
                if (adb.Girone.Count() > 0)
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
                foreach (Partecipante p in pdb.Partecipanti)
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
                        Partecipante pUpdateGroup = pdb.Partecipanti.SingleOrDefault(p1 => p1.TID == p.TID);
                        if (pUpdateGroup != null)
                        {
                            pUpdateGroup.Girone = i switch
                            {
                                0 => "A",
                                1 => "B",
                                _ => "C",
                            };
                            pdb.SaveChanges();
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
                                adb.Girone.Add(g);
                                break;
                            default:
                                gid = GetMaxGID("B") + 1;
                                g.GID = gid;
                                bdb.Girone.Add(g);
                                break;
                        }
                    }
                    //Save the changes to the dbs
                    adb.SaveChanges();
                    bdb.SaveChanges();
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
                            0 => adb.Girone,
                            _ => bdb.Girone,
                        };
                        standings = new List<Standing>();
                        //Calculate the total points for each player
                        foreach (Girone g in dbset)
                        {
                            Standing stg = new Standing
                            {
                                ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID
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
                                id = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == s.ID.ToLower()).TID;
                                Girone g = j switch
                                {
                                    0 => adb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                    _ => bdb.Girone.SingleOrDefault(g => g.PlayerID == id),
                                };
                                finalPlayers.Add(g);
                            }
                        }
                    }
                    List<int> eids = new List<int>();
                    List<int> pids = new List<int>();
                    string resultsDummy = "";
                    if (edb.Quarti.Count() > 0)
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
                    //Push the players into the db
                    foreach (Girone g in finalPlayers)
                    {
                        Quarti q = new Quarti
                        {
                            PlayerID = g.PlayerID,
                            Results = resultsDummy,
                            QID = 0,
                            OpponentID = 0,
                        };
                        edb.Quarti.Add(q);
                    }
                    edb.SaveChanges();
                    foreach (Partecipante p in pdb.Partecipanti)
                    {
                        p.Bracket = "Q";
                    }
                    pdb.SaveChanges();
                    //Randomly assign an opponent to each player
                    foreach (Quarti q in edb.Quarti)
                    {
                        pids.Add(q.PlayerID);
                    }
                    pids = Shuffle(pids);

                    foreach (Quarti q in edb.Quarti)
                    {
                        q.OpponentID = pids.ElementAt(q.PlayerID - 1);
                    }

                    List<Quarti> qs = new List<Quarti>();
                    foreach (Quarti q in edb.Quarti)
                    {
                        qs.Add(q);
                    }
                    int qid = 1;
                    int k = 0;
                    foreach (Quarti q in qs)
                    {
                        if (q.QID == 0)
                        {
                            q.QID = qid;
                            for (k = 0; k < 8; ++k)
                            {
                                if (qs.ElementAt(k).PlayerID == q.OpponentID)
                                {
                                    break;
                                }
                            }
                            qs.ElementAt(k).QID = qid + 1;
                            qid += 2;
                        }
                    }
                    edb.SaveChanges();

                    Semifinali sdummy = new Semifinali()
                    {
                        PlayerID = -1,
                        OpponentID = -1,
                        Results = resultsDummy,
                    };
                    for(int i = 0; i < 4; ++i)
                    {
                        sdummy.SID = i + 1;
                        edb.Semifinali.Add(sdummy);
                        edb.SaveChanges();
                    }

                    Finale fdummy = new Finale()
                    {
                        PlayerID = -1,
                        OpponentID = -1,
                        Results = resultsDummy,
                    };
                    for (int i = 0; i < 2; ++i)
                    {
                        fdummy.FID = i + 1;
                        edb.Finale.Add(fdummy);
                        edb.SaveChanges();
                    }

                    Consolazione cdummy = new Consolazione()
                    {
                        PlayerID = -1,
                        OpponentID = -1,
                        Results = resultsDummy,
                    };
                    for (int i = 0; i < 2; ++i)
                    {
                        cdummy.CID = i + 1;
                        edb.Consolazione.Add(cdummy);
                        edb.SaveChanges();
                    }
                    res += Strings.knockoutsSeeded;
                }
            }
            return res;
        }

        /// <summary>
        /// Shuffle
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        private static List<int> Shuffle(List<int> l)
        {
            Random rng = new Random();
            List<int> res = new List<int>();
            List<int> tmp = new List<int>();
            int[] tmp2 = new int[l.Count()];
            int dim = l.Count();
            while(tmp.Count() < dim / 2)
            {
                tmp.Add(l.ElementAt(0));
                l.RemoveAt(0);
            }
            for (int i = 0; i < tmp.Count(); ++i)
            {
                int index = rng.Next(l.Count());
                int next = l.ElementAt(index);
                tmp2[tmp.ElementAt(i) - 1] = next;
                tmp2[next - 1] = tmp.ElementAt(i);
                l.RemoveAt(index);
            }
            for(int i = 0; i < tmp2.Length; ++i)
            {
                res.Add(tmp2[i]);
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
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
            string res = "";
            string html = "";
            string[] subs;
            //Avoid spamming the results picture
            if (DateTime.Now < lastCommand.AddMinutes(coolDown))
            {
                return res;
            }
            //Check if the dbs are populated
            if (adb.Girone.Count() <= 0 || 
                (DateTime.Now > finalsDate && fdb.Girone.Count() <= 0) ||
                (DateTime.Now > finalsDate && edb.Finale.Count() <= 0 && MaxFinalists == 2) ||
                (DateTime.Now > finalsDate && edb.Semifinali.Count() <= 0 && MaxFinalists == 4) ||
                (DateTime.Now > finalsDate && edb.Quarti.Count() <= 0 && MaxFinalists == 8))
            {
                res += Strings.notYetSeededGroups;
                return res;
            }
            lastCommand = DateTime.Now;

            subs = message.Split(' ');
            if (subs.Length == 1) //All groups
            {
                if (DateTime.Now < finalsDate)
                {
                    for (int j = 0; j < MaxGroups; ++j)
                    {
                        html += FetchGroupResults(j);
                    }
                    Render(html, 5);
                    return "mostragironi";
                }
                else
                {
                    if (!GroupFinals)
                    {
                        html = FetchGroupResults(3);
                        Render(html, 3);
                        return "mostragironi";
                    }
                    return res;
                }
            }
            else if (subs.Length == 2) //Single group
            {
                switch (subs[1].ToUpper())
                {
                    case "A":
                        html = FetchGroupResults(0);
                        Render(html, 0);
                        return "mostragironeA";
                    case "B":
                        html = FetchGroupResults(1);
                        Render(html, 1);
                        return "mostragironeB";
                    case "F":
                        if (DateTime.Now < finalsDate || !GroupFinals)
                        {
                            res += Strings.saywhat;
                            return res;
                        }
                        html = FetchGroupResults(3);
                        Render(html, 3);
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
        /// <summary>
        /// Blanket method to fetch the specified group results
        /// </summary>
        /// <param name="Group"></param>
        /// <returns></returns>
        private static string FetchGroupResults(int Group)
        {
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
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
            if (adb.Girone.Count() > 0)
            {
                DbSet<Girone> dbset = Group switch
                {
                    0 => adb.Girone,
                    1 => bdb.Girone,
                    _ => fdb.Girone,
                };

                html += "<tr><td></td>";
                for (int i = 0; i < dbset.Count(); ++i)
                {
                    int pid = dbset.SingleOrDefault(g => g.GID == i + 1).PlayerID;
                    string lid = pdb.Partecipanti.SingleOrDefault(p => p.TID == pid).LichessID;
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
                         + pdb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID + "</td>";
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
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
            if (adb.Girone.Count() <= 0 ||  (fdb.Girone.Count() <= 0 && DateTime.Now > finalsDate))
            {
                res += Strings.notYetSeededStandings;
                return res;
            }
            lastCommand = DateTime.Now;

            //Pretty straightforward html table generation
            html += "<div>";
            for (int j = 0; j < MaxGroups + 1; ++j)
            {
                if(j == 0 && DateTime.Now < finalsDate || j == 0 && !GroupFinals)
                {
                    continue;
                }
                html += "<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\" align=\"center\"><tr><b>Classifica girone ";
                html += j switch
                {
                    0 => "finale",
                    1 => "A",
                    _ => "B",
                };
                html += ":</b></tr>";
                dbCheck = j switch
                {
                    0 => fdb.Girone.Count(),
                    1 => adb.Girone.Count(),
                    _ => bdb.Girone.Count(),
                };
                if (dbCheck <= 0)
                {
                    res += Strings.errorNotSeeded;
                    return res;
                }
                dbset = j switch
                {
                    0 => fdb.Girone,
                    1 => adb.Girone,
                    _ => bdb.Girone,
                };

                List<Standing> standings = new List<Standing>();
                foreach (Girone g in dbset)
                {
                    Standing stg = new Standing
                    {
                        ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID
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
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
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
            int bracketID;
            string sub1;
            string sub2;
            subs = message.Split(" ");
            //Check if the db is populated 
            int dbCheckA = adb.Girone.Count();
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
                if (DateTime.Now < finalsDate || (DateTime.Now > finalsDate && GroupFinals))
                {
                    sub1 = subs[1];
                    sub1.Trim('@');
                    //Check if the players are in the same group first
                    p1 = pdb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower());
                    p2 = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                    if (p1 == p2)
                    {
                        res += Strings.inserisciUsage;
                        return res;
                    }
                    //Check if a Telegram ID was sent instead of a Lichess ID
                    if (p2 == null)
                    {
                        p2 = pdb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sub1.ToLower());
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
                    groupID = p1.Girone switch
                    {
                        "A" => 0,
                        "B" => 1,
                        "F" => 3,
                        _ => 2,
                    };
                    string[] elab = PullLatestResult(p1.LichessID, p2.LichessID);
                    if (elab[0] == "-1")
                    {
                        res += Strings.gameNotFound;
                        return res;
                    }
                    helper = elab[0];
                    //Push the game link to the games db
                    //Push the result to the group db
                    Models.Gironi.Game game = new Models.Gironi.Game
                    {
                        P1ID = p1.TID,
                        P2ID = p2.TID,
                        Link = elab[1]
                    };

                    DbSet<Girone> dbsetGroup = groupID switch
                    {
                        0 => adb.Girone,
                        1 => bdb.Girone,
                        _ => fdb.Girone,
                    };
                    DbSet<Models.Gironi.Game> dbsetGames = groupID switch
                    {
                        0 => adb.Partite,
                        1 => bdb.Partite,
                        _ => fdb.Partite,
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
                    switch (groupID)
                    {
                        case 0:
                            adb.Database.ExecuteSqlCommand(SQLCommand);
                            break;
                        case 1:
                            bdb.Database.ExecuteSqlCommand(SQLCommand);
                            break;
                        default:
                            fdb.Database.ExecuteSqlCommand(SQLCommand);
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
                        0 => adb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
                        1 => bdb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
                        _ => fdb.Girone.SingleOrDefault(g => g.PlayerID == p2.TID).Results,
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
                            adb.Database.ExecuteSqlCommand(SQLCommand);
                            adb.SaveChanges();
                            break;
                        case 1:
                            bdb.Database.ExecuteSqlCommand(SQLCommand);
                            bdb.SaveChanges();
                            break;
                        default:
                            fdb.Database.ExecuteSqlCommand(SQLCommand);
                            fdb.SaveChanges();
                            break;
                    }
                    res += Strings.insertedResult + Strings.checkResults;
                }
                else
                {
                    if (edb.Quarti.Count() <= 0)
                    {
                        return res;
                    }
                    sub1 = subs[1];
                    sub1.Trim('@');
                    //Check if LichessIDs are valid
                    if (pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower()) == null ||
                        pdb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower()) == null)
                    {
                        res += Strings.inserisciInvalidIDs;
                        return res;
                    }
                    p1 = pdb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower());
                    p2 = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());

                    if (p1.Bracket != p2.Bracket)
                    {
                        res += Strings.notSameBracket;
                        return res;
                    }
                    bracketID = p1.Bracket switch
                    {
                        "Q" => 0,
                        "S" => 1,
                        "F" => 2,
                        _ => 3,
                    };

                    switch (bracketID)
                    {
                        case 0:
                            if (edb.Quarti.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                        case 1:
                            if (edb.Semifinali.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                        case 2:
                            if (edb.Finale.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                        default:
                            if (edb.Consolazione.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                    }

                    string[] elab = PullLatestResult(p1.LichessID, p2.LichessID);
                    if (elab[0] == "-1")
                    {
                        res += Strings.gameNotFound;
                        return res;
                    }
                    helper = elab[0];
                    //Push the game link to the games db
                    //Push the result to the group db
                    Models.Eliminatorie.Game game = new Models.Eliminatorie.Game
                    {
                        P1ID = p1.TID,
                        P2ID = p2.TID,
                        Link = elab[1]
                    };
                    edb.Games.Add(game);

                    player1LichessID = p2.LichessID;
                    player1ID = p2.TID;

                    results = bracketID switch
                    {
                        0 => edb.Quarti.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                        1 => edb.Semifinali.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                        2 => edb.Finale.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                        _ => edb.Consolazione.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                    };
                    subresults = results.Split(",");
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "-1")
                        {
                            subresults[i] = helper;
                            break;
                        }
                    }
                    results = "";
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        results += subresults[i];
                        if (i != subresults.Length - 1)
                        {
                            results += ",";
                        }
                    }
                    Quarti q = edb.Quarti.SingleOrDefault(p => p.PlayerID == player1ID);
                    Semifinali s = edb.Semifinali.SingleOrDefault(p => p.PlayerID == player1ID);
                    Finale f = edb.Finale.SingleOrDefault(p => p.PlayerID == player1ID);
                    Consolazione c = edb.Consolazione.SingleOrDefault(p => p.PlayerID == player1ID);
                    switch (bracketID)
                    {
                        case 0:
                            q = edb.Quarti.SingleOrDefault(p => p.PlayerID == player1ID);
                            q.Results = results;
                            break;
                        case 1:
                            s = edb.Semifinali.SingleOrDefault(p => p.PlayerID == player1ID);
                            s.Results = results;
                            break;
                        case 2:
                            f = edb.Finale.SingleOrDefault(p => p.PlayerID == player1ID);
                            f.Results = results;
                            break;
                        default:
                            c = edb.Consolazione.SingleOrDefault(p => p.PlayerID == player1ID);
                            c.Results = results;
                            break;
                    }

                    helper = helper switch
                    {
                        "0" => "1",
                        "1" => "0",
                        _ => "x",
                    };

                    player2LichessID = p1.LichessID;
                    player2ID = p1.TID;

                    results = bracketID switch
                    {
                        0 => edb.Quarti.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                        1 => edb.Semifinali.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                        2 => edb.Finale.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                        _ => edb.Consolazione.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                    };
                    subresults = results.Split(",");
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "-1")
                        {
                            subresults[i] = helper;
                            break;
                        }
                    }
                    results = "";
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        results += subresults[i];
                        if (i != subresults.Length - 1)
                        {
                            results += ",";
                        }
                    }
                    switch (bracketID)
                    {
                        case 0:
                            q = edb.Quarti.SingleOrDefault(p => p.PlayerID == player2ID);
                            q.Results = results;
                            break;
                        case 1:
                            s = edb.Semifinali.SingleOrDefault(p => p.PlayerID == player2ID);
                            s.Results = results;
                            break;
                        case 2:
                            f = edb.Finale.SingleOrDefault(p => p.PlayerID == player2ID);
                            f.Results = results;
                            break;
                        default:
                            c = edb.Consolazione.SingleOrDefault(p => p.PlayerID == player2ID);
                            c.Results = results;
                            break;
                    }
                    edb.SaveChanges();
                    ManageBracket(player1ID, player2ID);
                    res += Strings.insertedResult + Strings.checkTab;
                    return res;
                }
            }
            else if(IsAdmin(sender))
            {
                if (DateTime.Now < finalsDate || (DateTime.Now > finalsDate && GroupFinals))
                {
                    sub1 = subs[1];
                    sub2 = subs[2];
                    //Check if LichessIDs are valid
                    if (pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower()) == null ||
                        pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower()) == null)
                    {
                        res += Strings.inserisciInvalidIDs;
                        return res;
                    }
                    p1 = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                    p2 = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower());

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
                    helper = subs[3] switch
                    {
                        "1" => "1",
                        "2" => "0",
                        _ => "x",
                    };

                    player1LichessID = subs[1];
                    player1LichessID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).LichessID;
                    player1ID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).TID;
                    player1GroupID = groupID switch
                    {
                        0 => adb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                        1 => bdb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                        _ => fdb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).GID,
                    };

                    player2LichessID = subs[2];
                    player2LichessID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).LichessID;
                    player2ID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).TID;
                    player2GroupID = groupID switch
                    {
                        0 => adb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                        1 => bdb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                        _ => fdb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).GID,
                    };

                    prevResults = groupID switch
                    {
                        0 => adb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
                        1 => bdb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
                        _ => fdb.Girone.SingleOrDefault(g => g.PlayerID == player1ID).Results,
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
                            adb.Database.ExecuteSqlCommand(SQLCommand);
                            break;
                        case 1:
                            bdb.Database.ExecuteSqlCommand(SQLCommand);
                            break;
                        default:
                            fdb.Database.ExecuteSqlCommand(SQLCommand);
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
                        0 => adb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
                        1 => bdb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
                        _ => fdb.Girone.SingleOrDefault(g => g.PlayerID == player2ID).Results,
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
                            adb.Database.ExecuteSqlCommand(SQLCommand);
                            adb.SaveChanges();
                            break;
                        case 1:
                            bdb.Database.ExecuteSqlCommand(SQLCommand);
                            bdb.SaveChanges();
                            break;
                        default:
                            fdb.Database.ExecuteSqlCommand(SQLCommand);
                            fdb.SaveChanges();
                            break;
                    }
                    res += Strings.insertedResult + Strings.checkResults;
                }
                else
                {
                    if(edb.Quarti.Count() <= 0)
                    {
                        return res;
                    }
                    sub1 = subs[1];
                    sub2 = subs[2];
                    //Check if LichessIDs are valid
                    if (pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower()) == null ||
                        pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower()) == null)
                    {
                        res += Strings.inserisciInvalidIDs;
                        return res;
                    }
                    p1 = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub1.ToLower());
                    p2 = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == sub2.ToLower());

                    if (p1.Bracket != p2.Bracket)
                    {
                        res += Strings.notSameBracket;
                        return res;
                    }
                    bracketID = p1.Bracket switch
                    {
                        "Q" => 0,
                        "S" => 1,
                        "F" => 2,
                        _ => 3,
                    };

                    switch (bracketID)
                    {
                        case 0:
                            if (edb.Quarti.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                        case 1:
                            if (edb.Semifinali.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                        case 2:
                            if (edb.Finale.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                        default:
                            if (edb.Consolazione.SingleOrDefault(q => q.PlayerID == p1.TID).OpponentID != p2.TID)
                            {
                                res += Strings.notSameBracket;
                                return res;
                            }
                            break;
                    }

                    //Valid result?
                    if (subs[3] != "1" && subs[3] != "2" && subs[3] != "x")
                    {
                        res += Strings.inserisciInvalidResult;
                        return res;
                    }
                    helper = subs[3] switch
                    {
                        "1" => "1",
                        "2" => "0",
                        _ => "x",
                    };

                    player1LichessID = subs[1];
                    player1LichessID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).LichessID;
                    player1ID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player1LichessID.ToLower()).TID;

                    results = bracketID switch
                    {
                        0 => edb.Quarti.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                        1 => edb.Semifinali.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                        2 => edb.Finale.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                        _ => edb.Consolazione.SingleOrDefault(q => q.PlayerID == player1ID).Results,
                    };
                    subresults = results.Split(",");
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "-1")
                        {
                            subresults[i] = helper;
                            break;
                        }
                    }
                    results = "";
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        results += subresults[i];
                        if (i != subresults.Length - 1)
                        {
                            results += ",";
                        }
                    }
                    Quarti q = edb.Quarti.SingleOrDefault(p => p.PlayerID == player1ID);
                    Semifinali s = edb.Semifinali.SingleOrDefault(p => p.PlayerID == player1ID);
                    Finale f = edb.Finale.SingleOrDefault(p => p.PlayerID == player1ID);
                    Consolazione c = edb.Consolazione.SingleOrDefault(p => p.PlayerID == player1ID);
                    switch (bracketID)
                    {
                        case 0:
                            q = edb.Quarti.SingleOrDefault(p => p.PlayerID == player1ID);
                            q.Results = results;
                            break;
                        case 1:
                            s = edb.Semifinali.SingleOrDefault(p => p.PlayerID == player1ID);
                            s.Results = results;
                            break;
                        case 2:
                            f = edb.Finale.SingleOrDefault(p => p.PlayerID == player1ID);
                            f.Results = results;
                            break;
                        default:
                            c = edb.Consolazione.SingleOrDefault(p => p.PlayerID == player1ID);
                            c.Results = results;
                            break;
                    }

                    helper = helper switch
                    {
                        "0" => "1",
                        "1" => "0",
                        _ => "x",
                    };

                    player2LichessID = subs[2];
                    player2LichessID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).LichessID;
                    player2ID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID.ToLower() == player2LichessID.ToLower()).TID;

                    results = bracketID switch
                    {
                        0 => edb.Quarti.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                        1 => edb.Semifinali.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                        2 => edb.Finale.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                        _ => edb.Consolazione.SingleOrDefault(q => q.PlayerID == player2ID).Results,
                    };
                    subresults = results.Split(",");
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "-1")
                        {
                            subresults[i] = helper;
                            break;
                        }
                    }
                    results = "";
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        results += subresults[i];
                        if (i != subresults.Length - 1)
                        {
                            results += ",";
                        }
                    }
                    switch (bracketID)
                    {
                        case 0:
                            q = edb.Quarti.SingleOrDefault(p => p.PlayerID == player2ID);
                            q.Results = results;
                            break;
                        case 1:
                            s = edb.Semifinali.SingleOrDefault(p => p.PlayerID == player2ID);
                            s.Results = results;
                            break;
                        case 2:
                            f = edb.Finale.SingleOrDefault(p => p.PlayerID == player2ID);
                            f.Results = results;
                            break;
                        default:
                            c = edb.Consolazione.SingleOrDefault(p => p.PlayerID == player2ID);
                            c.Results = results;
                            break;
                    }
                    edb.SaveChanges();
                    ManageBracket(player1ID, player2ID);
                    res += Strings.insertedResult + Strings.checkTab;
                }
            }
            return res;
        }

        /// <summary>
        /// Manage the passage of players to the semifinals/final/consolation bracket
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private static void ManageBracket(int p1, int p2)
        {
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            int bracketID = pdb.Partecipanti.SingleOrDefault(p => p.TID == p1).Bracket switch
            {
                "Q" => 0,
                "S" => 1,
                "F" => 2,
                _ => 3,
            };
            string[] subresults;
            double p1Score = 0;
            double p2Score = 0;
            int gamesPlayed = 0;
            switch (bracketID)
            {
                case 0:
                    Quarti q1 = edb.Quarti.SingleOrDefault(q => q.PlayerID == p1);
                    Quarti q2 = edb.Quarti.SingleOrDefault(q => q.PlayerID == p2);
                    subresults = q1.Results.Split(",");
                    p1Score = 0;
                    p2Score = 0;
                    gamesPlayed = 0;
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "1")
                        {
                            p1Score++;
                            gamesPlayed++;
                        } 
                        else if (subresults[i] == "0")
                        {
                            p2Score++;
                            gamesPlayed++;
                        }
                        else if (subresults[i] == "x")
                        {
                            p1Score += 0.5;
                            p2Score += 0.5;
                            gamesPlayed++;
                        }
                    }
                    if (gamesPlayed == BestOf)
                    {
                        if (p1Score > p2Score)
                        {
                            Partecipante pl1 = pdb.Partecipanti.SingleOrDefault(p => p.TID == p1);
                            pl1.Bracket = "S";
                            pdb.SaveChanges();

                            List<Semifinali> ss = new List<Semifinali>();
                            foreach(Semifinali s in edb.Semifinali)
                            {
                                ss.Add(s);
                            }

                            int sid = (int)Math.Ceiling((double)((double)q1.QID / 2));

                            int opponentID = -1;
                            bool exit = false;
                            foreach(Semifinali s in edb.Semifinali)
                            {
                                switch(sid)
                                {
                                    case 1:
                                        if(s.SID == 2)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    case 2:
                                        if (s.SID == 1)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    case 3:
                                        if (s.SID == 4)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    case 4:
                                        if (s.SID == 3)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (exit) { break; }
                                edb.SaveChanges();
                            }
                            ss.ElementAt(sid - 1).PlayerID = p1;
                            ss.ElementAt(sid - 1).SID = sid;
                            ss.ElementAt(sid - 1).OpponentID = opponentID;
                            edb.SaveChanges();
                        }
                        else if (p1Score < p2Score)
                        {
                            Partecipante pl2 = pdb.Partecipanti.SingleOrDefault(p => p.TID == p2);
                            pl2.Bracket = "S";
                            pdb.SaveChanges();

                            List<Semifinali> ss = new List<Semifinali>();
                            foreach (Semifinali _s in edb.Semifinali)
                            {
                                ss.Add(_s);
                            }

                            int sid = (int)Math.Ceiling((double)((double)q2.QID / 2));

                            int opponentID = -1;
                            bool exit = false;
                            foreach (Semifinali s in edb.Semifinali)
                            {
                                switch (sid)
                                {
                                    case 1:
                                        if (s.SID == 2)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    case 2:
                                        if (s.SID == 1)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    case 3:
                                        if (s.SID == 4)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    case 4:
                                        if (s.SID == 3)
                                        {
                                            opponentID = s.PlayerID;
                                            s.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (exit) { break; }
                                edb.SaveChanges();
                            }
                            ss.ElementAt(sid - 1).PlayerID = p2;
                            ss.ElementAt(sid - 1).SID = sid;
                            ss.ElementAt(sid - 1).OpponentID = opponentID;
                            edb.SaveChanges();
                        }
                        edb.SaveChanges();
                    }
                    break;
                case 1:
                    Semifinali s1 = edb.Semifinali.SingleOrDefault(s => s.PlayerID == p1);
                    Semifinali s2 = edb.Semifinali.SingleOrDefault(s => s.PlayerID == p2);
                    subresults = s1.Results.Split(",");
                    p1Score = 0;
                    p2Score = 0;
                    gamesPlayed = 0;
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "1")
                        {
                            p1Score++;
                            gamesPlayed++;
                        }
                        else if (subresults[i] == "0")
                        {
                            p2Score++;
                            gamesPlayed++;
                        }
                        else if (subresults[i] == "x")
                        {
                            p1Score += 0.5;
                            p2Score += 0.5;
                            gamesPlayed++;
                        }
                    }
                    if (gamesPlayed == BestOf)
                    {
                        if (p1Score > p2Score)
                        {
                            Partecipante pl1 = pdb.Partecipanti.SingleOrDefault(p => p.TID == p1);
                            Partecipante pl2 = pdb.Partecipanti.SingleOrDefault(p => p.TID == p2);
                            pl1.Bracket = "F";
                            pl2.Bracket = "C";
                            pdb.SaveChanges();

                            List<Finale> fs = new List<Finale>();
                            foreach (Finale f in edb.Finale)
                            {
                                fs.Add(f);
                            }

                            int fid = (int)Math.Ceiling((double)((double)s1.SID / 2));

                            int opponentID = -1;
                            bool exit = false;
                            foreach (Finale f in edb.Finale)
                            {
                                switch (fid)
                                {
                                    case 1:
                                        if (f.FID == 2)
                                        {
                                            opponentID = f.PlayerID;
                                            f.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    case 2:
                                        if (f.FID == 1)
                                        {
                                            opponentID = f.PlayerID;
                                            f.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (exit) { break; }
                            }
                            edb.SaveChanges();
                            fs.ElementAt(fid - 1).PlayerID = p1;
                            fs.ElementAt(fid - 1).FID = fid;
                            fs.ElementAt(fid - 1).OpponentID = opponentID;

                            List<Consolazione> cs = new List<Consolazione>();
                            foreach (Consolazione c in edb.Consolazione)
                            {
                                cs.Add(c);
                            }

                            int cid = (int)Math.Ceiling((double)((double)s2.SID / 2));

                            opponentID = -1;
                            exit = false;
                            foreach (Consolazione c in edb.Consolazione)
                            {
                                switch (cid)
                                {
                                    case 1:
                                        if (c.CID == 2)
                                        {
                                            opponentID = c.PlayerID;
                                            c.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    case 2:
                                        if (c.CID == 1)
                                        {
                                            opponentID = c.PlayerID;
                                            c.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (exit) { break; }
                            }
                            edb.SaveChanges();
                            cs.ElementAt(cid - 1).PlayerID = p2;
                            cs.ElementAt(cid - 1).CID = cid;
                            cs.ElementAt(cid - 1).OpponentID = opponentID;
                            edb.SaveChanges();
                        }
                        else if (p1Score < p2Score)
                        {
                            Partecipante pl1 = pdb.Partecipanti.SingleOrDefault(p => p.TID == p1);
                            Partecipante pl2 = pdb.Partecipanti.SingleOrDefault(p => p.TID == p2);
                            pl2.Bracket = "F";
                            pl1.Bracket = "C";
                            pdb.SaveChanges();

                            List<Finale> fs = new List<Finale>();
                            foreach (Finale f in edb.Finale)
                            {
                                fs.Add(f);
                            }

                            int fid = (int)Math.Ceiling((double)((double)s2.SID / 2));

                            int opponentID = -1;
                            bool exit = false;
                            foreach (Finale f in edb.Finale)
                            {
                                switch (fid)
                                {
                                    case 1:
                                        if (f.FID == 2)
                                        {
                                            opponentID = f.PlayerID;
                                            f.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    case 2:
                                        if (f.FID == 1)
                                        {
                                            opponentID = f.PlayerID;
                                            f.OpponentID = p2;
                                            exit = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (exit) { break; }
                            }
                            edb.SaveChanges();
                            fs.ElementAt(fid - 1).PlayerID = p2;
                            fs.ElementAt(fid - 1).FID = fid;
                            fs.ElementAt(fid - 1).OpponentID = opponentID;

                            List<Consolazione> cs = new List<Consolazione>();
                            foreach (Consolazione c in edb.Consolazione)
                            {
                                cs.Add(c);
                            }

                            int cid = (int)Math.Ceiling((double)((double)s1.SID / 2));

                            opponentID = -1;
                            exit = false;
                            foreach (Consolazione c in edb.Consolazione)
                            {
                                switch (cid)
                                {
                                    case 1:
                                        if (c.CID == 2)
                                        {
                                            opponentID = c.PlayerID;
                                            c.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    case 2:
                                        if (c.CID == 1)
                                        {
                                            opponentID = c.PlayerID;
                                            c.OpponentID = p1;
                                            exit = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (exit) { break; }
                            }
                            edb.SaveChanges();
                            cs.ElementAt(cid - 1).PlayerID = p1;
                            cs.ElementAt(cid - 1).CID = cid;
                            cs.ElementAt(cid - 1).OpponentID = opponentID;
                            edb.SaveChanges();
                        }
                    }
                    break;
                case 2:
                    Finale f1 = edb.Finale.SingleOrDefault(f => f.PlayerID == p1);
                    Finale f2 = edb.Finale.SingleOrDefault(f => f.PlayerID == p2);
                    subresults = f1.Results.Split(",");
                    p1Score = 0;
                    p2Score = 0;
                    gamesPlayed = 0;
                    for (int i = 0; i < subresults.Length; ++i)
                    {
                        if (subresults[i] == "1")
                        {
                            p1Score++;
                            gamesPlayed++;
                        }
                        else if (subresults[i] == "0")
                        {
                            p2Score++;
                            gamesPlayed++;
                        }
                        else if (subresults[i] == "x")
                        {
                            p1Score += 0.5;
                            p2Score += 0.5;
                            gamesPlayed++;
                        }
                    }
                    if (gamesPlayed == BestOf)
                    {
                        if(p1Score > p2Score)
                        {
                            Campione c = new Campione()
                            {
                                PlayerID = pdb.Partecipanti.SingleOrDefault(p => p.TID == p1).LichessID
                            };
                            edb.Campione.Add(c);
                            edb.SaveChanges();
                        }
                        else if (p1Score < p2Score)
                        {
                            Campione c = new Campione()
                            {
                                PlayerID = pdb.Partecipanti.SingleOrDefault(p => p.TID == p2).LichessID
                            };
                            edb.Campione.Add(c);
                            edb.SaveChanges();
                        }
                    }
                    break;
                default:
                    break;
            }
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
            string res = "";
            string[] subs = message.Split(" ");
            string p1Lichess;
            string p2Lichess;
            string link;
            //Check if the cooldown has passed
            if (DateTime.Now < lastCommand.AddMinutes(coolDown))
            {
                return "";
            }
            if(subs.Length == 1)
            {
                foreach (Models.Eliminatorie.Game g in edb.Games)
                {
                    p1Lichess = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.P1ID).LichessID;
                    p2Lichess = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.P2ID).LichessID;
                    link = g.Link;
                    res += p1Lichess + " vs " + p2Lichess + "\n" + link + "\n";
                }
                return res;
            }
            else if(subs[1].Length == 1) // /partite (A B o C)
            {
                subs[1] = subs[1].ToUpper();
                int check = subs[1] switch
                {
                    "A" => adb.Partite.Count(),
                    "B" => bdb.Partite.Count(),
                    "F" => fdb.Partite.Count(),
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
                DbSet<Models.Gironi.Game> dbset = subs[1] switch
                {
                    "A" => adb.Partite,
                    "B" => bdb.Partite,
                    _ => fdb.Partite,
                };
                foreach (Models.Gironi.Game g in dbset)
                {
                    p1Lichess = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.P1ID).LichessID;
                    p2Lichess = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.P2ID).LichessID;
                    link = g.Link;
                    res += p1Lichess + " vs " + p2Lichess + "\n" + link + "\n";
                }
            }
            else //player
            {
                string submittedID = subs[1];
                int? playerID = null;
                //Check if the player exists
                if (pdb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID) == null)
                {
                    res += Strings.player404 + Strings.errorContact;
                    return res;
                }
                //Pull the player's tournament and group id
                playerID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).TID;
                int groupID = pdb.Partecipanti.SingleOrDefault(p => p.LichessID == submittedID).Girone switch {
                    "A" => 0,
                    "B" => 1,
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
                if (adb.Partite.Count() <= 0 || (DateTime.Now > finalsDate && fdb.Partite.Count() > 0))
                {
                    res += Strings.notYetPlayedGames;
                    return res;
                }

                string senderLichessID = pdb.Partecipanti.SingleOrDefault(p => p.TID == playerID).LichessID;
                res += "Partite di " + senderLichessID + ":\n";
                DbSet<Models.Gironi.Game> dbset = groupID switch
                {
                    0 => adb.Partite,
                    1 => bdb.Partite,
                    _ => fdb.Partite,
                };
                string prevRes = res;
                foreach (Models.Gironi.Game g in dbset)
                {
                    p1Lichess = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.P1ID).LichessID;
                    p2Lichess = pdb.Partecipanti.SingleOrDefault(p => p.TID == g.P2ID).LichessID;
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
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
            if (IsAdmin(sender)) //Submit whose tg id to use, only if admin and only for testing
            {
                string[] submessage = message.Split(" ");
                if (submessage.Length == 2)
                {
                    sender = submessage[1];
                }
            }
            string res = "";
            Partecipante p = pdb.Partecipanti.SingleOrDefault(p => p.TGID.ToLower() == sender.ToLower());
            if (p == null)
            {
                res += Strings.notRegistered + Strings.errorContact;
                return res;
            }
            string lichessID = p.LichessID;
            if (DateTime.Now < finalsDate || (DateTime.Now > finalsDate && GroupFinals))
            {
                int groupID = p.Girone switch
                {
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
                    0 => adb.Girone,
                    1 => bdb.Girone,
                    _ => fdb.Girone,
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
                            opponents.Add(pdb.Partecipanti.SingleOrDefault(p => p.TID == g.PlayerID).LichessID);
                        }
                        else
                        {
                            opponents.Add(dummy);
                        }
                    }
                }
                bool flag = false;
                foreach (string s in opponents)
                {
                    if (s != null)
                    {
                        flag = true;
                    }
                }
                if (!flag)
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
                        res += pdb.Partecipanti.SingleOrDefault(p => p.LichessID == opponentTG).TGID + ")";
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
            }
            else
            {
                string opponent = "";
                switch(p.Bracket)
                {
                    case "Q":
                        Quarti q = edb.Quarti.SingleOrDefault(q => q.PlayerID == p.TID);
                        res += Strings.opponentBracket;
                        opponent = pdb.Partecipanti.SingleOrDefault(p1 => p1.TID == q.OpponentID).LichessID;
                        res += opponent + Strings.opponentColor;
                        if(q.QID % 2 == 0)
                        {
                            res += "nero";
                        }
                        else
                        {
                            res += "bianco";
                        }
                        res += Strings.opponentClosing;
                        break;
                    case "S":
                        Semifinali s = edb.Semifinali.SingleOrDefault(s => s.PlayerID == p.TID);
                        res += Strings.opponentBracket;
                        opponent = pdb.Partecipanti.SingleOrDefault(p1 => p1.TID == s.OpponentID).LichessID;
                        res += opponent + Strings.opponentColor;
                        if (s.SID % 2 == 0)
                        {
                            res += "bianco";
                        }
                        else
                        {
                            res += "nero";
                        }
                        res += Strings.opponentClosing;
                        break;
                    case "F":
                        Finale f = edb.Finale.SingleOrDefault(f => f.PlayerID == p.TID);
                        res += Strings.opponentBracket;
                        opponent = pdb.Partecipanti.SingleOrDefault(p1 => p1.TID == f.OpponentID).LichessID;
                        res += opponent + Strings.opponentColor;
                        if (f.FID % 2 == 0)
                        {
                            res += "nero";
                        }
                        else
                        {
                            res += "bianco";
                        }
                        res += Strings.opponentClosing;
                        break;
                    default:
                        Consolazione c = edb.Consolazione.SingleOrDefault(c => c.PlayerID == p.TID);
                        res += Strings.opponentBracket;
                        opponent = pdb.Partecipanti.SingleOrDefault(p1 => p1.TID == c.OpponentID).LichessID;
                        res += opponent + Strings.opponentColor;
                        if (c.CID % 2 == 0)
                        {
                            res += "nero";
                        }
                        else
                        {
                            res += "bianco";
                        }
                        res += Strings.opponentClosing;
                        break;
                }
            }
            return res;
        }

        private static string TabelloneCommand(string message, string sender)
        {
            using EliminatorieDbContext edb = new EliminatorieDbContext();
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            string res = "";
            if(DateTime.Now > finalsDate && edb.Quarti.Count() > 0)
            {
                string dir = Directory.GetCurrentDirectory();
                string fpath = dir + "\\bracket.html";
                string[] outStrings = new string[33];
                List<int> pids = new List<int>();
                List<string> lids = new List<string>();
                foreach(Quarti q in edb.Quarti)
                {
                    for(int i = 0; i < MaxFinalists; ++i)
                    {
                        if(!pids.Contains(q.PlayerID))
                        {
                            pids.Add(q.PlayerID);
                            pids.Add(q.OpponentID);
                            lids.Add(pdb.Partecipanti.SingleOrDefault(p => p.TID == q.PlayerID).LichessID);
                            lids.Add(pdb.Partecipanti.SingleOrDefault(p => p.TID == q.OpponentID).LichessID);
                        }
                    }
                }
                outStrings[0] = lids.ElementAt(0);
                outStrings[1] = lids.ElementAt(1);
                outStrings[2] = lids.ElementAt(2);
                outStrings[3] = lids.ElementAt(3);
                outStrings[4] = lids.ElementAt(4);
                outStrings[5] = lids.ElementAt(5);
                outStrings[6] = lids.ElementAt(6);
                outStrings[7] = lids.ElementAt(7); 
                List<Standing> standings = new List<Standing>();
                string[] subresults;
                //Calculate the total points for each player
                foreach (Quarti q in edb.Quarti)
                {
                    Standing stg = new Standing
                    {
                        ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == q.PlayerID).LichessID
                    };
                    subresults = q.Results.Split(",");
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
                string result = "";
                foreach(Standing s in standings)
                {
                    result = "";
                    if (s.Tot % 1 != 0)
                    {
                        if ((int)s.Tot == 0)
                        {
                            result = "&#189;";
                        }
                        else
                        {
                            result = (int)s.Tot + "&#189;";
                        }
                    }
                    else
                    {
                        result = s.Tot.ToString();
                    }
                    for (int i = 0; i < 8; ++i)
                    {
                        if (outStrings[i] == s.ID)
                        {
                            outStrings[i + 8] = result;
                        }
                    }
                }

                pids.Clear();
                lids.Clear();
                standings.Clear();

                foreach (Semifinali s in edb.Semifinali)
                {
                    if (s.PlayerID != -1)
                    {
                        for (int i = 0; i < MaxFinalists; ++i)
                        {
                            if (!pids.Contains(s.PlayerID))
                            {
                                pids.Add(s.PlayerID);
                                pids.Add(s.OpponentID);
                                lids.Add(pdb.Partecipanti.SingleOrDefault(p => p.TID == s.PlayerID).LichessID);
                                lids.Add(s.OpponentID == -1 ? "" : pdb.Partecipanti.SingleOrDefault(p => p.TID == s.OpponentID).LichessID);
                            }
                        }
                    }
                }
                outStrings[16] = (lids.Count() == 0) ? Strings.empty : lids.ElementAt(0);
                outStrings[17] = (lids.Count() < 2) ? Strings.empty : lids.ElementAt(1);
                outStrings[18] = (lids.Count() < 3) ? Strings.empty : lids.ElementAt(2);
                outStrings[19] = (lids.Count() < 4) ? Strings.empty : lids.ElementAt(3);
                //Calculate the total points for each player
                foreach (Semifinali s in edb.Semifinali)
                {
                    if (s.PlayerID != -1)
                    {
                        Standing stg = new Standing
                        {
                            ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == s.PlayerID).LichessID
                        };
                        subresults = s.Results.Split(",");
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
                foreach (Standing s in standings)
                {
                    result = "";
                    if (s.Tot % 1 != 0)
                    {
                        if ((int)s.Tot == 0)
                        {
                            result = "&#189;";
                        }
                        else
                        {
                            result = (int)s.Tot + "&#189;";
                        }
                    }
                    else
                    {
                        result = s.Tot.ToString();
                    }
                    for (int i = 16; i < 20; ++i)
                    {
                        if (outStrings[i] == s.ID)
                        {
                            outStrings[i + 4] = result;
                        }
                    }
                }

                pids.Clear();
                lids.Clear();
                standings.Clear();

                foreach (Finale f in edb.Finale)
                {
                    for (int i = 0; i < MaxFinalists; ++i)
                    {
                        if (f.PlayerID != -1)
                        {
                            if (!pids.Contains(f.PlayerID))
                            {
                                pids.Add(f.PlayerID);
                                pids.Add(f.OpponentID);
                                lids.Add(pdb.Partecipanti.SingleOrDefault(p => p.TID == f.PlayerID).LichessID);
                                lids.Add(f.OpponentID == -1 ? "" : pdb.Partecipanti.SingleOrDefault(p => p.TID == f.OpponentID).LichessID);
                            }
                        }
                    }
                }
                outStrings[24] = (lids.Count() == 0) ? Strings.empty : lids.ElementAt(0);
                outStrings[25] = (lids.Count() == 0) ? Strings.empty : lids.ElementAt(1);
                //Calculate the total points for each player
                foreach (Finale f in edb.Finale)
                {
                    if (f.PlayerID != -1)
                    {
                        Standing stg = new Standing
                        {
                            ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == f.PlayerID).LichessID
                        };
                        subresults = f.Results.Split(",");
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
                foreach (Standing s in standings)
                {
                    result = "";
                    if (s.Tot % 1 != 0)
                    {
                        if ((int)s.Tot == 0)
                        {
                            result = "&#189;";
                        }
                        else
                        {
                            result = (int)s.Tot + "&#189;";
                        }
                    }
                    else
                    {
                        result = s.Tot.ToString();
                    }
                    for (int i = 24; i < 26; ++i)
                    {
                        if (outStrings[i] == s.ID)
                        {
                            outStrings[i + 2] = result;
                        }
                    }
                }

                pids.Clear();
                lids.Clear();
                standings.Clear();

                foreach (Consolazione c in edb.Consolazione)
                {

                    if (c.PlayerID != -1)
                    {
                        for (int i = 0; i < MaxFinalists; ++i)
                        {
                            if (!pids.Contains(c.PlayerID))
                            {
                                pids.Add(c.PlayerID);
                                pids.Add(c.OpponentID);
                                lids.Add(pdb.Partecipanti.SingleOrDefault(p => p.TID == c.PlayerID).LichessID);
                                lids.Add(c.OpponentID == -1 ? "" : pdb.Partecipanti.SingleOrDefault(p => p.TID == c.OpponentID).LichessID);
                            }
                        }
                    }
                }
                outStrings[28] = (lids.Count() == 0) ? Strings.empty : lids.ElementAt(0);
                outStrings[29] = (lids.Count() == 0) ? Strings.empty : lids.ElementAt(1);
                //Calculate the total points for each player
                foreach (Consolazione c in edb.Consolazione)
                {
                    if (c.PlayerID != -1)
                    {
                        Standing stg = new Standing
                        {
                            ID = pdb.Partecipanti.SingleOrDefault(p => p.TID == c.PlayerID).LichessID
                        };
                        subresults = c.Results.Split(",");
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
                foreach (Standing s in standings)
                {
                    result = "";
                    if (s.Tot % 1 != 0)
                    {
                        if ((int)s.Tot == 0)
                        {
                            result = "&#189;";
                        }
                        else
                        {
                            result = (int)s.Tot + "&#189;";
                        }
                    }
                    else
                    {
                        result = s.Tot.ToString();
                    }
                    for (int i = 28; i < 30; ++i)
                    {
                        if (outStrings[i] == s.ID)
                        {
                            outStrings[i + 2] = result;
                        }
                    }
                }

                outStrings[32] = "";
                foreach(Campione c in edb.Campione)
                {
                    outStrings[32] = c.PlayerID;
                }

                string outHTML =
                    Strings.bracketHTMLQ1 + outStrings[0] +
                    Strings.bracketHTMLQ1res + outStrings[8] +
                    Strings.bracketHTMLQ2 + outStrings[1] +
                    Strings.bracketHTMLQ2res + outStrings[9] +
                    Strings.bracketHTMLS1 + outStrings[16] +
                    Strings.bracketHTMLS1res + outStrings[20] +
                    Strings.bracketHTMLS2 + outStrings[17] +
                    Strings.bracketHTMLS2res + outStrings[21] +
                    Strings.bracketHTMLQ3 + outStrings[2] +
                    Strings.bracketHTMLQ3res + outStrings[10] +
                    Strings.bracketHTMLQ4 + outStrings[3] +
                    Strings.bracketHTMLQ4res + outStrings[11] +
                    Strings.bracketHTMLF1 + outStrings[24] +
                    Strings.bracketHTMLF1res + outStrings[26] +
                    Strings.bracketHTMLF2 + outStrings[25] +
                    Strings.bracketHTMLF2res + outStrings[27] +
                    Strings.bracketHTMLQ5 + outStrings[4] +
                    Strings.bracketHTMLQ5res + outStrings[12] +
                    Strings.bracketHTMLQ6 + outStrings[5] +
                    Strings.bracketHTMLQ6res + outStrings[13] +
                    Strings.bracketHTMLS3 + outStrings[18] +
                    Strings.bracketHTMLS3res + outStrings[22] +
                    Strings.bracketHTMLChamp + outStrings[32] +
                    Strings.bracketHTMLS4 + outStrings[19] +
                    Strings.bracketHTMLS4res + outStrings[23] +
                    Strings.bracketHTMLQ7 + outStrings[6] +
                    Strings.bracketHTMLQ7res + outStrings[14] +
                    Strings.bracketHTMLQ8 + outStrings[7] +
                    Strings.bracketHTMLQ8res + outStrings[15] +
                    Strings.bracketHTMLC1 + outStrings[28] +
                    Strings.bracketHTMLC1res + outStrings[30] +
                    Strings.bracketHTMLC2 + outStrings[29] +
                    Strings.bracketHTMLC2res + outStrings[31] +
                    Strings.bracketHTML;
                File.WriteAllText(fpath, String.Empty);
                using (StreamWriter file =
                    new StreamWriter(fpath))
                {
                        file.WriteLine(outHTML);
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/C wkhtmltoimage.exe --load-error-handling ignore --load-media-error-handling ignore --allow \".\" --disable-smart-width " + fpath + " " + dir + "\\bracket.png"
                };
                process.StartInfo = startInfo;
                process.Start();
                Thread.Sleep(1000);
                return "bracket";
            }
            return res;
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            return pdb.Partecipanti.Count();
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
        /// Pulls the provided player's rapid ELO from the db or assigns a new one if he doesnt have one
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int GetELO(string player)
        {
            using MembriDbContext mdb = new MembriDbContext();
            if(mdb.Membri.SingleOrDefault(m => m.LichessID.ToLower() == player.ToLower()) == null)
            {
                Membro m = new Membro()
                {
                    LichessID = player,
                    ELO = 1500,
                };
                mdb.Membri.Add(m);
                mdb.SaveChanges();
                return 1500;
            }
            else
            {
                return mdb.Membri.SingleOrDefault(m => m.LichessID.ToLower() == player.ToLower()).ELO;
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
            using PartecipantiDbContext pdb = new PartecipantiDbContext();
            int max = 0;
            foreach(Partecipante p in pdb.Partecipanti)
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
            using GironeADbContext adb = new GironeADbContext();
            using GironeBDbContext bdb = new GironeBDbContext();
            using GironeFDbContext fdb = new GironeFDbContext();
            int max = 0;
            DbSet<Girone> dbset = Group switch {
                "A" => adb.Girone,
                "B" => bdb.Girone,
                _ => fdb.Girone,
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
        public static void Render(string source, int n = -1, string file = "", int width = 50)
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