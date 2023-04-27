using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.AI.Navigation;



/// <summary>
/// This class updates the nav mesh when necessary. It will also look at the children of objects added into the list.
/// </summary>
/// <remarks>
/// The NavMeshComponent and several others are found in this Unity github repository since they are not in the package manager as of yet:
/// https://github.com/Unity-Technologies/NavMeshComponents
/// There is also a Unity Learning page for basic usage of the NavMeshSurface component:
/// https://learn.unity.com/tutorial/runtime-navmesh-generation#
/// </remarks>
public class NavMeshManager : MonoBehaviour
{
    [Tooltip("When the game starts up, the nav mesh generation will be delayed by this amount of time (in seconds).")]
    [Range(0f, 60f)]
    [SerializeField] private float _StartupNavMeshGenerationDelay = 5.0f;

    [SerializeField] private List<GameObject> _NavMeshSurfaceObjects;



    private List<NavMeshSurface> _NavMeshSurfaces;



    private void Awake()
    {
        _NavMeshSurfaces = new List<NavMeshSurface>();

        GetNavMeshSurfacesFromObjectsList();
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine("DoDelayedNavMeshGeneration");
    }


    public void RegenerateAllNavMeshes()
    {
        for (int i = 0; i < _NavMeshSurfaces.Count; i++)
        {
            // NOTE: When using the following debug code, you may notice duplicate output.
            //       This is because the "Level01" gameobject (a child of the "Terrain" gameobject)
            //       has two NavMeshSurface components on it (one for both types of AI monster agents).
            //Debug.Log($"#{i}: Regenerating Nav Mesh for GameObject \"{_NavMeshSurfaces[i].name}\"    |  HasCollider: { (_NavMeshSurfaces[i].GetComponent<Collider>() != null) }");

            // NOTE: You DO NOT need a NavMeshSurface component on every object. The ones on the "Terrain"
            //       GameObject are set to generate a navmesh from all gameobjects in the scene.
            _NavMeshSurfaces[i].BuildNavMesh();

        } // end for i

    }   

    public void AddNavMeshSurfaces(GameObject obj)
    {
        ProcessGameObject(obj, true);
    }

    public void RemoveNavMeshSurfaces(GameObject obj)
    {
        ProcessGameObject(obj, false);
    }

    private void GetNavMeshSurfacesFromObjectsList()
    {
        _NavMeshSurfaces.Clear();

        for (int i = 0; i < _NavMeshSurfaceObjects.Count; i++)
        {
            ProcessGameObject(_NavMeshSurfaceObjects[i], true);

        } // end foreach GameObject

    }


    /// <summary>
    /// Gets all NavMeshSurfaces from the specified GameObject and its children.
    /// This method can also be called to regenerate the nav meshes in just the passed in object,
    /// since objects already in the lists are not added again to prevent duplicates.
    /// </summary>
    /// <remarks>
    /// All NavMeshSurfaces in the passed in GameObject and its children will be added or removed from _NavMeshSurfaces.
    /// The GameObject itself will also be removed from the GameObjects list.
    /// 
    /// NOTE: Objects and NavMeshes that are already in the lists will not be added again to prevent duplicates, which would
    ///       waste time when generating nav meshes for objects if the RegenerateAllNavMeshes() method gets called.
    ///       
    /// When a NavMeshSurface gets added to _NavMeshSurfaces, we will also tell it to build its nav mesh if the
    /// regenerateNavMeshes parameter is true. This way we don't rebuild the nav meshes for every object in the list again
    /// for no reason.
    /// </remarks>
    /// <param name="obj">The GameObject to scan.</param>
    /// <param name="addSurfaces">Whether or not to add NavMeshSurfaces from obj into _NavMeshSurfaces. If this parameter is false, the NavMeshSurfaces of obj will be removed from _NavMeshSurfaces instead if any are already in the list.</param>
    /// <param name="regenerateNavMeshes">Whether or not surfaces added will also have their nav meshes regenerated at the same time.</param>
    private void ProcessGameObject(GameObject obj, bool addSurfaces, bool regenerateNavMeshes = false)
    {
        // If this game object is a monster, the player, or a villager, then simply return. Otherwise it may mess up the nav meshes.
        if (obj.layer == LayerMask.NameToLayer("Monsters") || obj.layer == LayerMask.NameToLayer("Player") || obj.layer == LayerMask.NameToLayer("Villagers"))
            return;


        // Add the object to our list if it is not already in it.
        if (addSurfaces && !_NavMeshSurfaceObjects.Contains(obj))
            _NavMeshSurfaceObjects.Add(obj);


        // Find every GameObject in the passed in object's hierarchy, and
        // remove it from our list depending on the addSurfaces parameter.
        List<Transform> transforms = new List<Transform>();
        obj.GetComponentsInChildren<Transform>(transforms);

        foreach (Transform t in transforms)
        {
            GameObject go = t.gameObject;

            if (!addSurfaces)
                _NavMeshSurfaceObjects.Remove(go);
        }


        // Find every NavMeshSurface component in the passed in object's hierarchy, and either
        // add or remove it from our list depending on the addSurfaces parameter.
        NavMeshSurface[] surfaces = obj.GetComponentsInChildren<NavMeshSurface>();
        
        for (int i = 0; i < surfaces.Length; i++)
        {
            NavMeshSurface s = surfaces[i];

            if (addSurfaces && !_NavMeshSurfaces.Contains(s))
            {
                _NavMeshSurfaces.Add(s);
            }
            else if (!addSurfaces)
            {
                _NavMeshSurfaces.Remove(s);
            }


            /* This code doesn't work currently, because each object in our list gets a separate nav mesh built for it if we do it this way.
             * So I replaced it with the code below this loop.
            if (regenerateNavMeshes)
            {
                // Tell this NavMeshSurface to rebuild its nav mesh.                
                s.BuildNavMesh();
            }
            */

        } // end foreach NavMeshSurface


        if (regenerateNavMeshes)
            RegenerateAllNavMeshes();

    }

    private IEnumerator DoDelayedNavMeshGeneration()
    {
        yield return new WaitForSeconds(_StartupNavMeshGenerationDelay);

        RegenerateAllNavMeshes();
    }
    
}
