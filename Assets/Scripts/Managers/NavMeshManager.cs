using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


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
    [SerializeField]
    private List<GameObject> _NavMeshSurfaceObjects;

    private List<NavMeshSurface> _NavMeshSurfaces;



    private void Awake()
    {
        _NavMeshSurfaces = new List<NavMeshSurface>();

        GetNavMeshSurfacesFromObjectsList();
    }

    // Start is called before the first frame update
    void Start()
    {
        RegenerateAllNavMeshes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void RegenerateAllNavMeshes()
    {        
        for (int i = 0; i < _NavMeshSurfaces.Count; i++)
        {
            _NavMeshSurfaces[i].BuildNavMesh();
        }
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
    /// <param name="addSurfaces">Whether or not to add NavMeshSurfaces from obj into _NavMeshSurfaces. If this parameter is false, the NavMeshSurfaces of obj will be removed from _NavMeshSurfaces instead.</param>
    /// <param name="regenerateNavMeshes">Whether or not surfaces added will also have their nav meshes regenerated at the same time.</param>
    private void ProcessGameObject(GameObject obj, bool addSurfaces, bool regenerateNavMeshes = false)
    {
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

    
}
