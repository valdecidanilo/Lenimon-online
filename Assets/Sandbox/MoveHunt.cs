using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHunt : MonoBehaviour
{
    private void Start()
    {
        const string route = PokeAPI.baseRoute + "move/";
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
        }
    }
}
