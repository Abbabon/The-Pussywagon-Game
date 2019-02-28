using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions
{
    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
