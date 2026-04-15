using System.ComponentModel.DataAnnotations.Schema;

namespace PlayMaker.Models.LiveScoreModel
{
    public class Event
    {
        public List<Team> T1 { get; set; } // Ev sahibi takım
        public List<Team> T2 { get; set; } // Deplasman takımı
        public int Tr1 { get; set; } // Ev sahibi skoru
        public int Tr2 { get; set; } // Deplasman skoru
        public string Eps { get; set; } // Maç Durumu

        [NotMapped] 
        public int PollId { get; set; }
        public int Vote1 { get; set; }
        public int VoteX { get; set; }
        public int Vote2 { get; set; }

    }
}
