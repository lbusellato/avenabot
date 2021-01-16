namespace avenabot.Interpreter
{
    /// <summary>
    /// This class holds all strings used by the bot
    /// </summary>
    public static class Strings
    {
        public static readonly string welcomeMsg = "Ciao! Sono AvenaChessBot e gestirò i Tornei Avenoni Scacchisti.\nUsa /help per visualizzare i comandi disponibili, oppure usa /torneo per visualizzare informazioni sul torneo";

        public static readonly string tournamentInfo = "Il Torneo Avenoni Scacchisti 2: Episodio II inizierà il dd/mm/yy.\n" +
            "Le iscrizioni, tramite il comando /iscrivimi, saranno aperte fino al dd/mm/yy.\nI partecipanti" +
            " saranno divisi in base all'ELO Rapid in due gironi preliminari all'italiana da 4 giocatori, nei quali" +
            " ciascuno giocherà una sola partita Rapid 10+5 contro ogni altro avversario nel girone.\n" +
            "I migliori 2 di ciascun girone parteciperanno ad " +
            "un girone finale all'italiana, il cui primo classificato sarà il vincitore del torneo.\nIl torneo è gestito" +
            " interamente da me, @AvenaChessBot, a cui i partecipanti invieranno i risultati delle partite con i quali " +
            "aggiornerò automaticamente classifica e tabellone dei risultati.\nBuona fortuna a tutti!";

        public static readonly string saywhat = "Non ho capito, usa /help per vedere la lista dei comandi disponibili.";

        public static readonly string startCommand = "/start";
        public static readonly string startDescr = "\n";

        public static readonly string helpCommand = "/help";
        public static readonly string helpDescr = "\nMostra questa lista";

        public static readonly string partecipantiCommand = "/partecipanti";
        public static readonly string partecipantiDescr = "\nMostra la lista di partecipanti attualmente iscritti";

        public static readonly string iscrivimiCommand = "/iscrivimi";
        public static readonly string iscrivimiDescr = " IDLichess\nIscrive al torneo il giocatore corrispondente all'ID Lichess specificato";
        public static readonly string iscrivimiUsage = "Utilizzo: /iscrivimi IDLichess";
        public static readonly string registeredError = "Risulti già iscritto!";
        public static readonly string registered = "Ti ho iscritto!";
        public static readonly string closedRegistrations = "Spiacente, le iscrizioni sono chiuse!";
        public static readonly string usernameNeeded = "È necessario avere un username Telegram valido per potersi iscrivere!";

        public static readonly string rimuoviCommand = "/rimuovi";
        public static readonly string rimuoviDescr = " IDLichess\nRimuove il giocatore specificato dal torneo";
        public static readonly string rimuoviUsage = "Utilizzo: /rimuovi IDLichess";
        public static readonly string removedAdmin = "Giocatore rimosso!";

        public static readonly string aggiungiCommand = "/aggiungi";
        public static readonly string aggiungiDescr = " IDLichess IDTelegram\n Iscrive il giocatore specificato al torneo";
        public static readonly string aggiungiUsage = "Utilizzo: /aggiungi IDLichess IDTelegram";
        public static readonly string registeredAdmin = "Giocatore iscritto!";
        public static readonly string registeredAdminError = "Il giocatore risulta già iscritto!";

        public static readonly string seedCommand = "/seed";
        public static readonly string seedDescr = "\nPopola i gironi";
        public static readonly string groupsSeeded = "Gironi popolati.";
        public static readonly string knockoutsSeeded = "Eliminatorie popolate.";
        public static readonly string finalGroupSeeded = "Girone finale popolato.";
        public static readonly string groupsAlreadySeeded = "I gironi risultano già popolati!";
        public static readonly string finalGroupAlreadySeeded = "Il girone finale risulta già popolato!";
        public static readonly string knockGroupAlreadySeeded = "Le eliminatorie risultano già popolate!";
        public static readonly string errorNotSeeded = "Errore: i gironi non risultano popolati.";
        public static readonly string notEnoughPlayers = "Non posso popolare i gironi perché non ci sono abbastanza giocatori iscritti!";

        public static readonly string risultatiCommand = "/risultati";
        public static readonly string risultatiDescr = " A/B\nMostra i risultati delle partite del girone specificato o di tutti i gironi se non viene specificato un girone";
        public static readonly string notYetSeededGroups = "I gironi non sono ancora stati generati. ";
        public static readonly string risultatiUsage = "Utilizzo:\n/risultati\nMostra i risultati di entrambi i gironi.\n/risultati A/B\nMostra i risultati del girone specificato.";

        public static readonly string classificaCommand = "/classifica";
        public static readonly string classificaDescr = "\nMostra le classifiche";
        public static readonly string notYetSeededStandings = "Le classifiche non sono ancora state generate. ";

        public static readonly string inserisciCommand = "/inserisci";
        public static readonly string inserisciDescr = " IDLichess\nRegistra automaticamente il risultato dell'ultima tua partita giocata contro il giocatore specificato";
        public static readonly string inserisciUsage = "Utilizzo:\n/inserisci 'Avversario'\n Al posto di 'avversario' scrivi l'id Lichess del tuo avversario, mi occuperò io di recuperare il risultato.";
        public static readonly string inserisciInvalidIDs = "Uno o entrambi gli ID Lichess che hai inserito non sembrano corrispondere a giocatori iscritti al torneo, ricontrolla per favore."; public static readonly string errorInvalidMessage = "Bravo porcodio bravo, ma vai a fare in culo."; public static readonly string invalidMessage = "a38";
        public static readonly string inserisciInvalidIDs2 = "Tu o il tuo avversario non risultate iscritti al torneo, ricontrolla per favore.";
        public static readonly string inserisciInvalidResult = "Il risultato immesso non è corretto, i risultati accettati sono 1 (vittoria giocatore 1), 2 (vittoria giocatore 2) e x (pareggio).";
        public static readonly string inserisciInvalidGroup = "I giocatori che hai immesso non sembrano essere nello stesso girone, prova a ricontrollare.";
        public static readonly string alreadyInserted = "Ho già un risultato registrato per questa partita.";
        public static readonly string notSameGroup = "L'avversario che hai immesso non sembra essere nel tuo gruppo, prova a ricontrollare.";
        public static readonly string gameNotFound = "Non ho trovato tue partite con l'avversario che hai specificato, se hai appena giocato attendi un paio di minuti che Lichess carichi la partita.";
        public static readonly string insertedResult = "Risultato inserito correttamente.";
        public static readonly string notValidGroup = "Il girone specificato non esiste!";

        public static readonly string torneoCommand = "/torneo";
        public static readonly string torneoDescr = "\nVisualizza informazioni sul torneo";

        public static readonly string partiteCommand = "/partite";
        public static readonly string partiteDescr = " A/B/C\nMostra la lista di link alle partite giocate finora nel girone specificato";
        public static readonly string partiteUsage = "Utilizzo:\n/partite A/B\nMostra le partite del girone specificato.";
        public static readonly string partiteHeader = "Partite Girone ";
        public static readonly string notYetPlayedGames = "Non è stata ancora giocata nessuna partita nel girone.";

        public static readonly string miePartiteCommand = "/miepartite";
        public static readonly string miePartiteDescr = "\nMostra la lista di partite che devi ancora giocare con il colore che dovrai usare";
        public static readonly string noGamesToPlay = "Hai giocato tutte le partite che dovevi giocare, attendi che anche gli altri giocatori facciano lo stesso.";
        public static readonly string noGames = "Il giocatore specificato deve ancora giocare una partita.";

        public static readonly string player404 = "Il giocatore indicato non sembra essere iscritto!";
        public static readonly string lichess404 = "L'ID Lichess che hai inserito non sembra esistere, controlla di averlo scritto giusto.";
        public static readonly string errorContact = " Se credi sia un errore contatta @lbusellato";
        public static readonly string notRegistered = "Non mi sembra che tu sia iscritto!";
        public static readonly string checkPartecipanti = " Usa /partecipanti per vedere la lista degli iscritti.";
        public static readonly string checkResults = " Usa /risultati per vedere il tabellone dei risultati.";
        public static readonly string trueString = "true";
        public static readonly string falseString = "false";
    }
}