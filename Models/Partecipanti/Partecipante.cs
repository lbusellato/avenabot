namespace avenabot.Models.Partecipanti
{
    public class Partecipante
    {
        public int ID { get; set; }
        public int TID { get; set; }
        public string LichessID { get; set; }
        public string TGID { get; set; }
        public int ELO { get; set; }
        public string Girone { get; set; }
    }
}