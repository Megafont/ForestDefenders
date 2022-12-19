using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public static class Utils_Knockback
{
    private static AnimationCurve _sCurve;



    static Utils_Knockback()
    {
        _sCurve = new AnimationCurve(new Keyframe(0, 0),
                             new Keyframe(1, 1));
        _sCurve.preWrapMode = WrapMode.Clamp;
        _sCurve.postWrapMode = WrapMode.Clamp;
    }



    /// <summary>
    /// Applies a knockback motion effect to an NPC.
    /// 
    /// NOTE: This must be called using StartCoroutine().
    /// </summary>
    /// <param name="objectHit">The NPC to apply knockback motion to.</param>
    /// <param name="knockbackDirection">The direction of the knockback.</param>
    /// <param name="knockbackDistance">The distanc to knock the NPC back.</param>
    /// <param name="knockbackDuration">How long the knockback motion will take.</param>
    public static IEnumerator ApplyKnockbackMotion(GameObject objectHit, Vector3 knockbackDirection, float knockbackDistance = 2, float knockbackDuration = 0.15f)
    {
        // Disable the character while we're applying knockback so AI or player controls can't move it while we're doing the knockback animation.
        EnableObject(objectHit, false);


        Vector3 objectStartPos = objectHit.transform.position;

        float knockbackStartTime = Time.time;
        float elapsedTime = 0.0f;


        //Debug.DrawLine(objectStartPos + Vector3.up, objectStartPos + knockbackDirection * 10 + Vector3.up, Color.red, knockbackDistance);


        // Animate the NPC's position to create the knockback effect.
        while (elapsedTime <= knockbackDuration)
        {
            elapsedTime += Time.deltaTime;

            float curKnockbackAmount = knockbackDistance * _sCurve.Evaluate(elapsedTime / knockbackDuration);
            objectHit.transform.position = objectStartPos + (curKnockbackAmount * knockbackDirection);

            yield return null;
        }


        EnableObject(objectHit, true);
    }

    /// <summary>
    /// Applies a knockback motion effect to the player.
    /// 
    /// NOTE: This must be called using StartCoroutine().
    /// </summary>
    /// <param name="objectHit">The NPC to apply knockback motion to.</param>
    /// <param name="knockbackDirection">The direction of the knockback.</param>
    /// <param name="knockbackDistance">The distanc to knock the NPC back.</param>
    /// <param name="knockbackDuration">How long the knockback motion will take.</param>
    public static IEnumerator ApplyKnockbackMotionOnPlayer(GameObject player, Vector3 knockbackDirection, float knockbackDistance = 2, float knockbackDuration = 0.15f)
    {
        // Disable the NavMeshAgent component so it doesn't move the character while we're applying knockback.
        PlayerController controller = player.GetComponent<PlayerController>();
        controller.enabled = false;


        Vector3 objectStartPos = player.transform.position;

        float knockbackStartTime = Time.time;
        float elapsedTime = 0.0f;


        Debug.DrawLine(objectStartPos + Vector3.up, objectStartPos + knockbackDirection * 10 + Vector3.up, Color.red, knockbackDistance);


        // Animate the NPC's position to create the knockback effect.
        while (elapsedTime <= knockbackDuration)
        {
            elapsedTime += Time.deltaTime;

            float curKnockbackAmount = knockbackDistance * _sCurve.Evaluate(elapsedTime / knockbackDuration);
            player.transform.position = objectStartPos + (curKnockbackAmount * knockbackDirection);

            yield return null;
        }

        Debug.Log("Player knockback ended.");

        // Re-enable the PlayerController.
        controller.enabled = true;
    }

    /// <summary>
    /// Sets the enabled state of the object knockback is being applied to.
    /// </summary>
    /// <param name="obj">The object to set the enabled state of.</param>
    /// <param name="state">The value to set its enabled state to.</param>
    /// <returns>True if it was set successfully, or false if the object is not a supported type.</returns>
    private static bool EnableObject(GameObject obj, bool state)
    {
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent)
        {
            EnableNPC(agent, state);
            return true;
        }

        PlayerController player = obj.GetComponent<PlayerController>();
        if (player)
        {
            EnablePlayer(player, state);
            return true;
        }


        Debug.LogError("The passed in object is not a supported type!");
        return false;
    }


    private static void EnableNPC(NavMeshAgent npc, bool state = true)
    {
        if (state)
        {
            // Re-enable the NavMeshAgent and tell the NPC to reset the NavMeshAgent component's target to what it was before the knockback effect.
            npc.enabled = true;
            npc.GetComponent<AI_WithAttackBehavior>().ResetTarget(); // We are using AI_WithAttackBehavior here so that this code will work on both monsters and villagers (this is a base class for both).

        }
        else
        {
            // Disable the NavMeshAgent component so it doesn't move the character while we're applying knockback.
            npc.enabled = false;
        }

    }

    private static void EnablePlayer(PlayerController player, bool state = true)
    {
        PlayerController controller = player.GetComponent<PlayerController>();

        if (state)
            controller.enabled = true;
        else
            controller.enabled = false;

    }

}
