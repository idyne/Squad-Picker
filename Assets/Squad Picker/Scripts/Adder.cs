
using UnityEngine;


public class Adder : Operator
{
    [SerializeField] private int number = 1;

    protected override void Operate()
    {
        Troop troop = levelManager.Troop;
        troop.AddHumans(number);
    }

    protected override void SetText()
    {
        text.text = "+" + number;
    }

}
