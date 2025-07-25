using SQLite;

namespace DB.Data
{
    [Table("SplicemonData")]
    public class PokemonModel
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public int UserId { get; set; }
        public int PartyIndex { get; set; }

        // Identidade
        public int PokeApiId { get; set; }
        public string Name { get; set; }
        public string NatureName { get; set; }
        public Gender Gender { get; set; }
        public int GrowthRate { get; set; }

        // Nível e Experiência
        
        public int CurrentHp { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceMax { get; set; }
        public int BaseExperience { get; set; }

        public string IvJson { get; set; }
        public string EvJson { get; set; }
        public string MovesJson { get; set; }
        
    }
}