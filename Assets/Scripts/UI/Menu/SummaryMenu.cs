using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryMenu : ContextMenu<Pokemon>
{
    public override void OpenMenu(Pokemon data)
    {
        gameObject.SetActive(true);
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
