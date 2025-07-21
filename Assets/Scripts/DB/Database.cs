using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DB.Data;
using Newtonsoft.Json;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

namespace DB
{
    public abstract class Database
    {
        private SQLiteConnection db;
        private const string RegexEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        
        //Local do .sqlite: C:/Users/valde/AppData/LocalLow/
        protected Database()
        {
            var dbPath = Path.Combine(Application.persistentDataPath, "splicemon.db");
            db = new SQLiteConnection(dbPath);
            db.CreateTable<PokemonModel>();
            db.CreateTable<UserData>();
        }

        public (bool, string, UserData) RegisterUser(string email, string password, string nickname)
        {
            if (password.Length < 6) return (false, "Senha muito curta", null);
            if(nickname.Length < 3) return (false, "Nickname muito curto", null);
            if(!System.Text.RegularExpressions.Regex.IsMatch(email, RegexEmail)) return (false, "Email inválido", null);
            if (db.Table<UserData>().Any(u => u.Email == email))
            {
                return (false, "Usuário já existe", null);
            }

            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword(password, salt);

            var newUser = new UserData
            {
                Email = email,
                PasswordHash = hash,
                Salt = salt,
                Nickname = nickname
            };
            // criar primeiro pokemon ou escolher entre 3.

            db.Insert(newUser);
            return (true, "Usuário registrado com sucesso", newUser);
        }

        public UserData LoginUser(string email, string password)
        {
            var user = db.Table<UserData>().FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            var hash = PasswordHasher.HashPassword(password, user.Salt);
            if (hash == user.PasswordHash)
            {
                Logger.Log("Login bem-sucedido.");
                return user;
            }

            Logger.Log("Senha incorreta.");
            return null;
        }

        public void SaveSplicemonForUser(PokemonModel mon, int userId)
        {
            db.Insert(mon);
        }

        public PokemonModel GetFirstSplicemon(int userId)
        {
            return db.Table<PokemonModel>().FirstOrDefault(x => x.UserId == userId);
        }
        
        public List<PokemonModel> GetSplicemonsByUser(int userId)
        {
            return db.Table<PokemonModel>().Where(x => x.UserId == userId).ToList();
        }

        public void DeleteAll()
        {
            db.DeleteAll<PokemonModel>();
        }
        public PokemonModel SaveData(Pokemon poke)
        {
            return new PokemonModel
            {
                Name = poke.name,
                Level = poke.level,
                NatureName = poke.natureName,
                Fainted = poke.fainted,
                Hp = poke.stats.hp,
                Atk = poke.stats.atk,
                Def = poke.stats.def,
                SpAtk = poke.stats.sAtk,
                SpDef = poke.stats.sDef,
                Spd = poke.stats.spd,
                Experience = poke.experience,
                ExperienceMax = poke.experienceMax,
                BaseExperience = poke.data.base_experience,
                FrontSpriteName = poke.data.sprites.front_default,
                BackSpriteName = poke.data.sprites.back_default,
                CrySound = poke.data.cries.latest,
                GrowthRate = poke.growthRate.ToString(),

                MovesJson = JsonConvert.SerializeObject(new MoveListWrapper { Move = poke.moves.ToList() }),
            };
        }
    }
}