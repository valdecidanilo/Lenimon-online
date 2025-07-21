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
    public class Database
    {
        private SQLiteConnection db;
        //Local : C:/Users/valde/AppData/LocalLow/
        public Database()
        {
            var dbPath = Path.Combine(Application.persistentDataPath, "splicemon.db");
            db = new SQLiteConnection(dbPath);
            db.CreateTable<SplicemonData>();
            db.CreateTable<UserData>();
        }
        string regexEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        public (bool, string, UserData) RegisterUser(string email, string password, string nickname)
        {
            if (password.Length < 6) return (false, "Senha muito curta", null);
            if(nickname.Length < 3) return (false, "Nickname muito curto", null);
            if(!System.Text.RegularExpressions.Regex.IsMatch(email, regexEmail)) return (false, "Email inválido", null);
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

            db.Insert(newUser);
            return (true, "Usuário registrado com sucesso", newUser);
        }

        public UserData LoginUser(string email, string password)
        {
            var user = db.Table<UserData>().FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            string hash = PasswordHasher.HashPassword(password, user.Salt);
            if (hash == user.PasswordHash)
            {
                Logger.Log("Login bem-sucedido.");
                return user;
            }

            Logger.Log("Senha incorreta.");
            return null;
        }

        public void SaveSplicemonForUser(SplicemonData mon, int userId)
        {
            db.Insert(mon);
        }

        public SplicemonData GetFirstSplicemon(int userId)
        {
            return db.Table<SplicemonData>().FirstOrDefault(x => x.UserId == userId);
        }
        
        public List<SplicemonData> GetSplicemonsByUser(int userId)
        {
            return db.Table<SplicemonData>().Where(x => x.UserId == userId).ToList();
        }

        public void DeleteAll()
        {
            db.DeleteAll<SplicemonData>();
        }
        public SplicemonData SaveData(Pokemon poke)
        {
            return new SplicemonData
            {
                Name = poke.name,
                Level = poke.level,
                Nature = poke.natureName,
                Hp = poke.stats.hp,
                Attack = poke.stats.atk,
                Defense = poke.stats.def,
                SpAttack = poke.stats.sAtk,
                SpDefense = poke.stats.sDef,
                Speed = poke.stats.spd,
                Experience = poke.experience,
                BaseExperience = poke.data.base_experience,
                FrontSprite = poke.frontSprite,
                BackSprite = poke.backSprite,
                CrySound = poke.data.cries.latest,
                GrowthRate = "replace here",

                MovesJson = JsonConvert.SerializeObject(new MoveListWrapper { Move = poke.moves.ToList() }),
            };
        }
    }
}