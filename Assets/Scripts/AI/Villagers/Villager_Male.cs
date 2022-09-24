using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Villager_Male : Villager_Base
{
    protected override void InitAI()
    {
        base.InitAI();
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();
    }


    /*
    protected override void DoAttack()
    {
        base.DoAttack();

        int n = Random.Range(1, 3);

        string trigger = $"Attack {n}";
        _Animator.ResetTrigger(trigger);
        _Animator.SetTrigger(trigger);

    }
    */

}
