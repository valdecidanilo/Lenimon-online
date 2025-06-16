using AddressableAsyncInstances;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestScript : MonoBehaviour
{
    [SerializeField] private BattlePokemon ally, enemy;

    private Sprite pokemonSprite;
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
        
        AAAsset<Sprite>.LoadAsset("MissingNoFront", (sprite) =>
        {
            pokemonSprite = sprite;
        });
    }

    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            StartCoroutine(ally.DefaultPhysicalMoveAnimation(enemy));
        }
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            StartCoroutine(enemy.DefaultPhysicalMoveAnimation(ally));
        }
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            StartCoroutine(ally.DefaultSpecialMoveAnimation(enemy));
        }
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            StartCoroutine(enemy.DefaultSpecialMoveAnimation(ally));
        }
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ally.ResetBattlePokemon();
            enemy.ResetBattlePokemon();
        }
    }
}
