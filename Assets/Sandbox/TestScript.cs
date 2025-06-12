using AddressableAsyncInstances;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestScript : MonoBehaviour
{
    [SerializeField] private BattlePokemon ally, enemy;
    private void Start()
    {
        /*const string route = PokeAPI.baseRoute + "move/";
        Checklist moveChecklist = new(1);
        WebConnection.GetRequest<ApiRequestList>(route, (data) =>
        {
            Debug.Log(data.count);
            moveChecklist.AddStep(919);
            moveChecklist.FinishStep();
            CheckMove();
        });

        void CheckMove()
        {
            Debug.LogWarning($"checking {moveChecklist.currentSteps}");
            WebConnection.GetRequest<MoveData>($"{route}{moveChecklist.currentSteps}", (data) =>
            {
                if (data.meta == null) Debug.Log($"{data.name} has null meta");
                else if (data.meta.category == null) Debug.Log($"{data.name} has null meta category");
                moveChecklist.FinishStep();
                if (moveChecklist.isDone) return;
                CheckMove();
            });
        }*/
        
        AAAsset<Sprite>.LoadAsset("pokeball", (sprite) =>
        {
            PokeDatabase.pokeBallSprite = sprite;
        });
    }

    private void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            StartCoroutine(ally.SwitchOutAnimation());
        }
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            StartCoroutine(enemy.SwitchOutAnimation(-1));
        }
    }
}
