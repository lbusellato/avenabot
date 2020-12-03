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

        public static string startDescr = "";
        public static string helpDescr = "Mostra la lista di comandi disponibili";
        public static string partecipantiDescr = "Mostra la lista di partecipanti attualmente iscritti";
        public static string iscrivimiDescr = "Iscrive l'ID Lichess specificato al torneo\nUtilizzo: /iscrivimi IDLichess";
        public static string rimuoviDescr = "Rimuove il giocatore specificato\nUtilizzo: /rimuovi IDLichess";
        public static string disiscrivimiDescr = "Disicrive dal torneo";
        public static string aggiungiDescr = "Aggiunge il giocatore specificato\nUtilizzo: /aggiungi IDLichess IDTelegram";
        public static string seedDescr = "Popola il girone\nUtilizzo: /seed IDGirone(A o B)";
        public static string risultatiDescr = "Mostra i risultati delle partite.\nUtilizzo:\n/risultati\nMostra i risultati di entrambi i gironi.\n/risultati IDGirone (A o B)\nMostra i risultati del girone specificato.\n/risultati IDLichess\nMostra i risultati del giocatore specificato.";
        public static string classificaDescr = "Mostra le classifiche.\nUtilizzo:\n/classifica\nMostra le classifiche di entrambi i gironi.\n/classifica IDGirone (A o B)\nMostra la classifica del girone specificato.";
        public static string inserisciDescr = "Inserisci il risultato di una partita.\nUtilizzo:\n/inserisci Giocatore1 Giocatore2 Risultato\nRisultato può essere 1 per vittoria del giocatore 1, 2 per vittoria del giocatore 2 o x per un pareggio.";
        public static string torneoDescr = "Visualizza informazioni sul torneo";

        public static string iscrivimiUsage = "Utilizzo: /iscrivimi IDLichess";
        public static string iscrivimiUsage2 = "Usa /iscrivimi IDLichess per riiscriverti";
        public static string rimuoviUsage = "Utilizzo: /rimuovi IDLichess";
        public static string aggiungiUsage = "Utilizzo: /aggiungi IDLichess IDTelegram";
        public static string risultatiUsage = "Il giocatore o il girone specificati non sono validi! Controlla di aver scritto giusto.\nUtilizzo:\n/risultati\nMostra i risultati di entrambi i gironi.\n/risultati IDGirone (A o B)\nMostra i risultati del girone specificato.\n/risultati IDLichess\nMostra i risultati del giocatore specificato.";
        public static string inserisciUsage = "Utilizzo:\n/inserisci Giocatore1 Giocatore2 Risultato\nRisultato può essere 1 per vittoria del giocatore 1, 2 per vittoria del giocatore 2 o x per un pareggio.";

        public static string welcomeMsg = "Ciao! Sono AvenaChessBot e gestirò i Tornei Avenoni Scacchisti.\nUsa /help per visualizzare i comandi disponibili, oppure usa /torneo per visualizzare informazioni sul torneo";
        public static string saywhat = "Non ho capito, usa /help per vedere la lista dei comandi disponibili.";
        public static string partecipantiHeader = "Elenco partecipanti:\nID Torneo - ID Lichess - ID Telegram - ELO\n";
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
        public static string groupSeeded = "Girone popolato.";
        public static string groupAlreadySeeded = "Il girone risulta già popolato!";
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
        public static string inserisciInvalidResult = "Il risultato immesso non è corretto, i risultati accettati sono 1 (vittoria giocatore 1), 2 (vittoria giocatore 2) e x (pareggio).";
        public static string checkResults = " Usa /risultati per vedere il tabellone dei risultati.";
        public static string insertedResult = "Risultato inserito correttamente.";
        public static string risultatiHeader = "<pre>Risultati girone A:\n&#32&#32 ";
        public static string classificaHeader1 = "Classifica Girone A:\nID";
        public static string classificaHeader2 = "G: 1 2 3 4 5 6 7 P\n";
    }
}
