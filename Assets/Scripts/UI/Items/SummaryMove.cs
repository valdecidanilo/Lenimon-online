using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryMove : MonoBehaviour
{
    [SerializeField] private Image type;
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text pp;

    public void SetupMove(MoveModel move)
    {
        type.gameObject.SetActive(move != null);
        if (move != null) type.sprite = PokeDatabase.typeSprites[move.moveType];
        name.text = move?.name ?? "-";
        pp.text = move == null ? "--" : $"pp{move.pp}/{move.maxPP}";
    }
}
