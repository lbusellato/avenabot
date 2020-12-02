namespace avenabot.Interpreter
{
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
       
        public static string iscrivimiUsage = "Utilizzo: /iscrivimi IDLichess";
        public static string iscrivimiUsage2 = "Usa /iscrivimi IDLichess per riiscriverti";
        public static string rimuoviUsage = "Utilizzo: /rimuovi IDLichess";
        public static string aggiungiUsage = "Utilizzo: /aggiungi IDLichess IDTelegram";

        public static string welcomeMsg = "Ciao! Sono il Bot che gestisce il Torneo Avenoni Scacchisti.\nUsa /help per visualizzare i comandi disponibili";
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
    }
}
