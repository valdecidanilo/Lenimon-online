using System.Collections.Generic;
using SQLite;
using UnityEngine;

namespace DB.Data
{
    [Table("SplicemonData")]
    public class PokemonModel
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public int UserId { get; set; }

        // Identidade
        public int PokeApiId { get; set; }
        public string Name { get; set; }
        public string NatureName { get; set; }
        public Gender Gender { get; set; }
        public string GrowthRate { get; set; }
        public bool Fainted { get; set; }

        // Nível e Experiência
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceMax { get; set; }
        public int BaseExperience { get; set; }

        // Stats Base
        public int Hp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpAtk { get; set; }
        public int SpDef { get; set; }
        public int Spd { get; set; }

        // IVs
        public int IvHp { get; set; }
        public int IvAtk { get; set; }
        public int IvDef { get; set; }
        public int IvSpAtk { get; set; }
        public int IvSpDef { get; set; }
        public int IvSpd { get; set; }

        // EVs
        public int EvHp { get; set; }
        public int EvAtk { get; set; }
        public int EvDef { get; set; }
        public int EvSpAtk { get; set; }
        public int EvSpDef { get; set; }
        public int EvSpd { get; set; }

        // Moves serializados como JSON
        public string MovesJson { get; set; }

        // Dados auxiliares
        public string AbilityName { get; set; }
        public string HeldItemName { get; set; }

        // Sprites (armazenar paths ou nomes de resources, não o Sprite diretamente)
        public string FrontSpriteName { get; set; }
        public string BackSpriteName { get; set; }
        public string IconSpriteName { get; set; }

        // Cry sound name ou path (caso use no futuro)
        public string CrySound { get; set; }
    }
    [System.Serializable]
    public class MoveListWrapper
    {
        public List<MoveModel> Move;
    }

    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> items;
    }
}