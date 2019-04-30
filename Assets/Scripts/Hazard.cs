using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HazardType
{
    Driver,
    CopsBarricade,
    Hole,
    Papparazi
}

public class Hazard : MonoBehaviour
{
    public int Lane;
    public HazardType HazardType;

    public int Cost()
    {
        switch (HazardType)
        {
            case HazardType.CopsBarricade:
                return 400;
            case HazardType.Driver:
                return 200;
            case HazardType.Hole:
                return 100;
            default:
                return 0;
        }
    }
}
