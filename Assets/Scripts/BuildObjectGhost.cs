using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildObjectGhost : MonoBehaviour
{
    public bool CanBuild { get { return _OverlappingObjects.Count == 0; } }


    private List<Collider> _OverlappingObjects = new List<Collider>();


    private void OnEnable()
    {
        // Whenever the player enters build mode, this object gets enabled to show the player where their structure will be built.
        // We need to clear the overlapping objects list to ensure there is no erroneous items in it from the last time the player
        // was in build mode. Otherwise, this can prevent the player from being able to build since this script will think there
        // are still collisions in this case.
        _OverlappingObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Build ghost object collided with GameObject {other.name}!");

        if (other.tag != "Ground" && other.tag != "Player" && other.tag != "EnemyTargetDetector")
        {
            //Debug.Log($"Can't build here! Build ghost object collided with GameObject {other.name}!");

            // Make sure we don't end up with duplicate entries in the list, as this can cause the player to be unable to build things
            // since only one entry gets cleared in OnTriggerExit().
            if (!_OverlappingObjects.Contains(other))
                _OverlappingObjects.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _OverlappingObjects.Remove(other);
    }
    
}
