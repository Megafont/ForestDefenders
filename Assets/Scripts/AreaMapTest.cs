using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace Test
{
    /// <summary>
    /// This test script draws the player's path on-screen to test that Utils_World.GetCoordinateInAreasMapSpace()
    /// is working right.
    /// </summary>
    /// <remarks>
    /// The prefab containing this script can just be pasted into the scene as a child of the HUD game object
    /// and it will work.
    /// </remarks>
    public class AreaMapTest : MonoBehaviour
    {
        GameManager _GameManager;

        Texture2D _AreaMapTexture;
        RawImage _RawImage;



        // Start is called before the first frame update
        void Start()        
        {
            _GameManager = GameManager.Instance;
            Texture2D mapTexture = Resources.Load<Texture2D>("Areas Map Test");

            if (mapTexture == null)
                throw new Exception("Failed to load the areas map texture!");


            _RawImage = GetComponent<RawImage>();
            _RawImage.material = new Material(Shader.Find("UI/Default"));
            _AreaMapTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            
            // Copy the texture into the RawImage component. This way we don't use the actual
            // texture resource. Otherwise changes made to it get saved, which we don't want.
            Graphics.CopyTexture(mapTexture, _AreaMapTexture);

            _RawImage.material.mainTexture = _AreaMapTexture;
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 coord = Utils_World.GetCoordinateInAreasMapSpace(_GameManager.Player.transform.position, _GameManager.TerrainBounds, _AreaMapTexture.Size());
            
            //Debug.Log($"COORD: {coord}");
            
            _AreaMapTexture.SetPixel(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), Color.red);
            _AreaMapTexture.Apply();

        }

    }

}
