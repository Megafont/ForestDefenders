using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[Flags]
public enum DamageTypes
{
    Starvation  = 1,
    Physical    = 2,   
    Electric    = 4,
    Fire        = 8,
    Ice         = 16,
}
