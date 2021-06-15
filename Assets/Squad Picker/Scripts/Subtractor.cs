using UnityEngine;

public class Subtractor : Operator
{
    [SerializeField] private int number = 1;

    protected override void Operate()
    {
        Troop troop = levelManager.Troop;
        troop.RemoveHumans(number);
    }

    protected override void SetText()
    {
        text.text = "-" + number;
    }
}
