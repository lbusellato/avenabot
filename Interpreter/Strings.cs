namespace avenabot.Interpreter
{
    /// <summary>
    /// This class holds all strings used by the bot
    /// </summary>
    public static class Strings
    {
        public static string startCommand = "/start";
        public static string helpCommand = "/help";
        public static string partecipantiCommand = "/partecipanti";
        public static string iscrivimiCommand = "/iscrivimi";
        public static string rimuoviCommand = "/rimuovi";
        public static string disiscrivimiCommand = "/disiscrivimi";
        public static string aggiungiCommand = "/aggiungi";
        public static string seedCommand = "/seed";
        public static string risultatiCommand = "/risultati";
        public static string classificaCommand = "/classifica";
        public static string inserisciCommand = "/inserisci";
        public static string torneoCommand = "/torneo";
        public static string partiteCommand = "/partite";
        public static string miePartiteCommand = "/miepartite";

        public static string startDescr = "";
        public static string helpDescr = "Mostra questa lista";
        public static string partecipantiDescr = "Mostra la lista di partecipanti attualmente iscritti";
        public static string iscrivimiDescr = "Iscrive l'ID Lichess specificato al torneo\nUtilizzo: /iscrivimi IDLichess";
        public static string rimuoviDescr = "Rimuove il giocatore specificato\nUtilizzo: /rimuovi IDLichess";
        public static string disiscrivimiDescr = "Disicrive dal torneo";
        public static string aggiungiDescr = "Aggiunge il giocatore specificato\nUtilizzo: /aggiungi IDLichess IDTelegram";
        public static string seedDescr = "Popola il girone\nUtilizzo: /seed";
        public static string risultatiDescr = "Mostra i risultati delle partite.\nUtilizzo:\n/risultati\nMostra i risultati di entrambi i gironi.\n/risultati (A B o C)\nMostra i risultati del girone specificato.\n/risultati IDLichess\nMostra i risultati del giocatore specificato.";
        public static string classificaDescr = "Mostra le classifiche.";
        public static string inserisciDescr = "Inserisci il risultato di una partita.\nUtilizzo:\n/inserisci 'Avversario'\n Al posto di 'avversario' scrivi l'id Lichess del tuo avversario, mi occuperò io di recuperare il risultato.";
        public static string torneoDescr = "Visualizza informazioni sul torneo";
        public static string partiteDescr = "Mostra la lista, con i link a Lichess, delle partite giocate finora.\nUtilizzo:\n/partite\nMostra tutte le partite di tutti i gironi.\n/partite (A B o C)\nMostra le partite del girone specificato.\n/partite IDLichess\nMostra le partite del giocatore specificato.";
        public static string miePartiteDescr = "Mostra la lista di partite che devi ancora giocare con il colore che dovrai usare.";


        public static string iscrivimiUsage = "Utilizzo: /iscrivimi IDLichess";
        public static string iscrivimiUsage2 = "Usa /iscrivimi IDLichess per riiscriverti";
        public static string iscrivimiUsage3 = "Usa /iscrivimi IDLichess per iscriverti";
        public static string rimuoviUsage = "Utilizzo: /rimuovi IDLichess";
        public static string aggiungiUsage = "Utilizzo: /aggiungi IDLichess IDTelegram";
        public static string risultatiUsage = "Il giocatore o il girone specificati non sono validi! Controlla di aver scritto giusto.\nUtilizzo:\n/risultati\nMostra i risultati di entrambi i gironi.\n/risultati IDGirone (A o B)\nMostra i risultati del girone specificato.\n/risultati IDLichess\nMostra i risultati del giocatore specificato.";
        public static string inserisciUsage = "Utilizzo:\n/inserisci 'Avversario'\n Al posto di 'avversario' scrivi l'id Lichess del tuo avversario, mi occuperò io di recuperare il risultato.";
        public static string partiteUsage = "Utilizzo:\n/partite\nMostra tutte le partite di tutti i gironi.\n/partite (A B o C)\nMostra le partite del girone specificato.\n/partite IDLichess\nMostra le partite del giocatore specificato.";

        public static string welcomeMsg = "Ciao! Sono AvenaChessBot e gestirò i Tornei Avenoni Scacchisti.\nUsa /help per visualizzare i comandi disponibili, oppure usa /torneo per visualizzare informazioni sul torneo";
        public static string saywhat = "Non ho capito, usa /help per vedere la lista dei comandi disponibili.";
        public static string partecipantiHeader = "Elenco partecipanti:\nID Torneo - Lichess - Telegram - ELO - Girone\n";
        public static string lichess404 = "L'ID Lichess che hai inserito non sembra esistere, controlla di averlo scritto giusto.";
        public static string errorContact = " Se credi sia un errore contatta @lbusellato";
        public static string registered = "Ti ho iscritto!";
        public static string registeredAdmin = "Giocatore iscritto!";
        public static string registeredError = "Risulti già iscritto!";
        public static string registeredAdminError = "Il giocatore risulta già iscritto!";
        public static string checkPartecipanti = " Usa /partecipanti per vedere la lista degli iscritti.";
        public static string closedRegistrations = "Spiacente, le iscrizioni sono chiuse!";
        public static string player404 = "Il giocatore indicato non sembra essere iscritto!";
        public static string notRegistered = "Non mi sembra che tu sia iscritto!";
        public static string removedAdmin = "Giocatore rimosso!";
        public static string removed = "Ti ho rimosso!";
        public static string groupsSeeded = "Gironi popolati.";
        public static string finalGroupSeeded = "Girone finale popolato.";
        public static string groupsAlreadySeeded = "I gironi risultano già popolati!";
        public static string finalGroupAlreadySeeded = "Il girone finale risulta già popolato!";
        public static string torneoInfo = "Il Torneo Avenoni Scacchisti 2: Episodio 2 inizierà il dd/mm/yy." + 
            "Le iscrizioni, tramite il comando /iscrivimi, saranno aperte fino al dd/mm/yy. I partecipanti" +
            " saranno divisi in base all'ELO Rapid in tre gironi preliminari all'italiana da 8 giocatori, nei quali" +
            " ciascuno giocherà una sola partita Rapid 10+5 contro ogni altro avversario nel girone. Dato che i gironi " +
            "saranno divisi per ELO gli aspiranti partecipanti sono obbligati a giocare almeno 10 partite Rapid" + 
            " classificate su Lichess prima dell'inizio del torneo. I migliori 3 di ciascun girone parteciperanno ad " +
            "un girone finale all'italiana, il cui primo classificato sarà il vincitore del torneo. Il torneo è gestito" +
            " interamente da me, AvenaChessBot, a cui i partecipanti invieranno i risultati delle partite con i quali " + 
            "aggiornerò automaticamente classifica e tabellone dei risultati.\nBuona fortuna a tutti!";
        public static string inserisciInvalidIDs = "Uno o entrambi gli ID Lichess che hai inserito non sembrano corrispondere a giocatori iscritti al torneo, ricontrolla per favore.";
        public static string inserisciInvalidIDs2 = "Tu o il tuo avversarion non risultate iscritti al torneo, ricontrolla per favore.";
        public static string inserisciInvalidResult = "Il risultato immesso non è corretto, i risultati accettati sono 1 (vittoria giocatore 1), 2 (vittoria giocatore 2) e x (pareggio).";
        public static string checkResults = " Usa /risultati per vedere il tabellone dei risultati.";
        public static string insertedResult = "Risultato inserito correttamente.";
        public static string risultatiHeaderA = "<pre>Risultati girone A:\n&#32&#32 ";
        public static string risultatiHeaderB = "<pre>Risultati girone B:\n&#32&#32 ";
        public static string risultatiHeaderC = "<pre>Risultati girone C:\n&#32&#32 ";
        public static string risultatiHeaderF = "<pre>Risultati girone finale:\n&#32&#32 ";
        public static string classificaHeader1A = "Classifica Girone A:\nID";
        public static string classificaHeader1B = "Classifica Girone B:\nID";
        public static string classificaHeader1C = "Classifica Girone C:\nID";
        public static string classificaHeader1F = "Classifica Girone Finale:\nID";
        public static string classificaHeader2 = "G:";
        public static string errorNotSeeded = "Errore: i gironi non risultano popolati.";
        public static string internalError = "ERRORE INTERNO, CONTATTA @lbusellato, CODICE ERRORE:";
        public static string inserisciInvalidGroup = "I giocatori che hai immesso non sembrano essere nello stesso girone, prova a ricontrollare.";
        public static string alreadyInserted = "Ho già un risultato registrato per questa partita.";
        public static string notSameGroup = "L'avversario che hai immesso non sembra essere nel tuo gruppo, prova a ricontrollare.";
        public static string partiteHeaderA = "Partite Girone A:\n";
        public static string partiteHeaderB = "Partite Girone B:\n";
        public static string partiteHeaderC = "Partite Girone C:\n";
        public static string noGames = "Il giocatore specificato deve ancora giocare una partita.";
        public static string notYetSeededGroups = "I gironi non sono ancora stati generati. ";
        public static string notYetSeededGames = "Le partite non sono ancora state generate. ";
        public static string notYetSeededStandings = "Le classifiche non sono ancora state generate. ";
        public static string resultsDisclaimer = "NB: per migliorare la visualizzazione i giocatori sono riportati con il loro ID torneo, che puoi visualizzare con il comando /partecipanti.";
        public static string notEnoughPlayers = "Non posso popolare i gironi perché non ci sono abbastanza giocatori iscritti!";
        public static string notYetPlayedGames = "Non è stata ancora giocata nessuna partita.";
        public static string gameNotFound = "Non ho trovato tue partite con l'avversario che hai specificato, ricontrolla per favore.";
    }
}