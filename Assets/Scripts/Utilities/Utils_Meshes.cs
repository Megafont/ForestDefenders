using System;

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


        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }


        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine, true);

        return newMesh;
    }


}
