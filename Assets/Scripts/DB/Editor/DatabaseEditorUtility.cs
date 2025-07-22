#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DB.Editor
{
    public static class DatabaseEditorUtility
    {
        [MenuItem("Tools/Database/Clean")]
        public static void CleanDatabaseFile()
        {
            var path = Path.Combine(Application.persistentDataPath, "splicemon.db");
            if (!File.Exists(path))
            {
                Debug.Log("Nenhum banco encontrado para remover.");
                return;
            }

            var db = new DB.Database();
            db.Close();

            try
            {
                File.Delete(path);
                Debug.Log("Banco de dados deletado com sucesso: " + path);
            }
            catch (IOException ex)
            {
                Debug.LogError("Erro ao deletar banco de dados: " + ex.Message);
            }
        }
    }
}
#endif