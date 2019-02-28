using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OptionType
{
    Compliment,
    Chocolate,
    FakeBrand,
    Tickets,
    Watch,
    iPhone,
    Jewlery,
    Dog,
    HelloKitty,
    CandyBracelet,
    Candy,
    Slime
}

[Serializable]
public class Option
{
    [SerializeField]
    private int cost;
    public int Cost { get => cost; set => cost = value; }
    [SerializeField]
    private OptionType type;
    internal OptionType Type { get => type; set => type = value; }


    public Option(int cost, OptionType type)
    {
        this.cost = cost;
        this.type = type;
    }
}
