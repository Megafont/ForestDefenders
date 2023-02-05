using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.AI;



public static class StatusEffect_Ice
{    
    private static float _EffectMagnitude = 0.5f; // The amount of the original speed this effect slows the character down to.
    private static float _EffectDuration = 5.0f; // How long this status effect will last.



    public static IEnumerator StartStatusEffect(GameObject target)
    {
        IMonster monster = target.GetComponent<Monster_Base>();        
        if (monster != null)
        {
            if ((monster.StatusEffectsFlags & StatusEffectsFlags.Ice) != 0)
                yield break; // simply exit this coroutine. We don't apply this status effect since the monster already has it.


            NavMeshAgent navMeshAgent = target.GetComponent<NavMeshAgent>();
            SkinnedMeshRenderer[] renderers = monster.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            float originalSpeed = navMeshAgent.speed;
            Material originalMaterial = renderers[0].material;


            ApplyEffect(monster, navMeshAgent, renderers);

            yield return new WaitForSeconds(_EffectDuration);

            RemoveEffect(monster, navMeshAgent, renderers, originalSpeed, originalMaterial);
        }
        else
        {
            Debug.LogError($"Cannot apply \"{MethodBase.GetCurrentMethod().DeclaringType.Name}\" to GameObject \"{target.name}\", because it is not a supported type!");
        }

    }
    
    private static void ApplyEffect(IMonster monster, NavMeshAgent navMeshAgent, SkinnedMeshRenderer[] renderers)
    {
        monster.StatusEffectsFlags |= StatusEffectsFlags.Ice;

        navMeshAgent.speed *= _EffectMagnitude;

        
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = monster.IcyMaterial;
    }

    private static void RemoveEffect(IMonster monster, NavMeshAgent navMeshAgent, SkinnedMeshRenderer[] renderers, float originalSpeed, Material originalMaterial)
    {
        navMeshAgent.speed = originalSpeed;


        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = originalMaterial;


        monster.StatusEffectsFlags &= ~StatusEffectsFlags.Ice;
    }


}

