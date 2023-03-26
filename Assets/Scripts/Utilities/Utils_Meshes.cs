using System;
using System.Collections.Generic;

using UnityEngine;


public static class Utils_Meshes
{
    public static Mesh CombineMeshes(MeshFilter[] meshFilters, Material combineMaterial)
    {
        if (meshFilters == null)
            throw new Exception("The passed in MeshFilter array is null!");
        else if (meshFilters.Length == 0)
            throw new Exception("The passed in MeshFilter array is empty!");
        else if (combineMaterial == null)
            throw new Exception("The passed in material is null!");


        List<CombineInstance> meshesToCombine = new List<CombineInstance>();

        int i = 0;
        while (i < meshFilters.Length)
        {
            
            // Create a CombineInstance object for every submesh in the current mesh since we want to combine all submeshes.
            // We need to do this so we can use the subMeshIndex property on each instance to tell it which submesh to
            // combine into the final mesh.
            for (int j = 0; j < meshFilters[i].mesh.subMeshCount; j++)
            {
                CombineInstance cInstance = new CombineInstance();

                cInstance.mesh = meshFilters[i].sharedMesh;
                cInstance.subMeshIndex = j;
                cInstance.transform = meshFilters[i].transform.localToWorldMatrix;

                meshesToCombine.Add(cInstance);
            }


            meshFilters[i].gameObject.SetActive(false);

            i++;
        }


        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(meshesToCombine.ToArray(), true);

        return newMesh;
    }


}
