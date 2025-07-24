using System;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Models;
using DB.Data;
using Newtonsoft.Json;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

namespace DB
{
    public class Database
    {
        private SQLiteConnection db;
        private const string RegexEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        
        //Local do .sqlite: C:/Users/valde/AppData/LocalLow/
        public Database()
        {
            var dbPath = Path.Combine(Application.persistentDataPath, "splicemon.db");
            db = new SQLiteConnection(dbPath);
            db.CreateTable<PokemonModel>();
            db.CreateTable<UserData>();
        }
        public void Close()
        {
            if (db != null)
            {
                db.Close();
                db.Dispose();
                db = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public (bool, string, UserData) RegisterUser(string email, string password, string nickname, int idSlicemon = 0)
        {
            if (password.Length < 6) return (false, "Senha muito curta", null);
            if (nickname.Length < 3) return (false, "Nickname muito curto", null);
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, RegexEmail)) return (false, "Email inválido", null);
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
            Debug.Log(newUser.Id);
            if (idSlicemon > 0)
            {
                PokeAPI.GetPokemonData(idSlicemon, data =>
                {
                    Pokemon.GetLoadedPokemon(data,5, poke =>
                    {
                        var model = SaveData(poke);
                        model.UserId = newUser.Id;
                        db.Insert(model);
                    });
                });
            }

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
        public void AddPokemonToUser(int userId, int pokemonId, Action<List<Pokemon>> onFinished)
        {
            PokeAPI.GetPokemonData(pokemonId, data =>
            {
                Pokemon.GetLoadedPokemon(data, 5, poke =>
                {
                    var model = SaveData(poke);
                    model.UserId = userId;
                    db.Insert(model);

                    var pokemonsModel = GetSplicemonsByUser(userId);
                    List<Pokemon> party = new();
                    Checklist loader = new(pokemonsModel.Count);

                    foreach (var p in pokemonsModel)
                    {
                        Pokemon.GetLoadedPokemon(p, loaded =>
                        {
                            party.Add(loaded);
                            loader.FinishStep();
                        });
                    }

                    loader.onCompleted += () =>
                    {
                        onFinished?.Invoke(party);
                    };
                });
            });
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
            Logger.Log($"{poke.moves}");
            return new PokemonModel
            {
                Name = poke.name,
                Level = poke.level,
                PokeApiId = poke.data.id,
                NatureName = poke.natureName,
                Gender = poke.gender,
                CurrentHp = poke.battleStats.hp,
                Experience = poke.experience,
                ExperienceMax = poke.experienceMax,
                BaseExperience = poke.data.base_experience,
                GrowthRate = (int)poke.growthRate.growthRate,

                IvJson = JsonConvert.SerializeObject(poke.iv),
                EvJson = JsonConvert.SerializeObject(poke.ev),
                MovesJson = JsonConvert.SerializeObject(new MoveListWrapper(poke.moves)),
            };
        }
    }
}