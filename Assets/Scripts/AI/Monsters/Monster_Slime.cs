using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class Monster_Slime : Monster_Base
{
    protected override void InitAI()
    {
        base.InitAI();
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();
    }


    protected override void AnimateAttack()
    {
        int n = Random.Range(1, 3);

        string trigger = $"Attack {n}";
        _Animator.ResetTrigger(trigger);
        _Animator.SetTrigger(trigger);
    }

}
