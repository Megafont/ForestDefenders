using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildObjectGhost : MonoBehaviour
{
    public bool CanBuild { get { return _OverlappingObjects.Count == 0; } }

    private List<Collider> _OverlappingObjects = new List<Collider>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Build ghost object collided with GameObject {other.name}!");

        if (other.tag != "Ground" && other.tag != "Player" && other.tag != "EnemyTargetDetector")
        {
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
