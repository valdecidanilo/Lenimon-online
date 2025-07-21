using System.Threading.Tasks;
using DB;
using DB.Data;
using UnityEngine;

namespace Auth
{
    public class AuthController : MonoBehaviour
    {
        public Database Database;
        //public NetworkRunner runner;
        
        // pra chamar nos botões da UI
        public UserData Login(string email, string password)
        {
            return Database.LoginUser(email, password);
        }
        public (bool,string, UserData) Register(string email, string password, string nickname)
        {
            return Database.RegisterUser(email, password, nickname);
        }
        
        //habilitar esse quando adicionar o fusion pun
        public Task<bool> StartSharedMode()
        {
            /*runner.ProvideInput = true;

            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = "world",
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (result.Ok)
            {
                Debug.Log("Jogo iniciado em modo Shared!");
                return true;
            }
            else
            {
                Debug.LogError($"Falha ao iniciar: {result.ShutdownReason}");
                return false;
            }
            */
            return Task.FromResult<bool>(this);
        }
    }
}