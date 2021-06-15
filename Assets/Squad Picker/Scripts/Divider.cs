
using UnityEngine;


public class Divider : Operator
{
    [SerializeField] private int divider = 1;

    protected override void Operate()
    {
        Troop troop = levelManager.Troop;
        troop.RemoveHumans(Mathf.CeilToInt(troop.Size * (1 - (1f / divider))));
    }

    protected override void SetText()
    {
        text.text = "÷" + divider;
    }

}
