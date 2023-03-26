
I created these rocks using the process from CG Geek's video:
https://www.youtube.com/watch?v=4EqLyGsu3AA

NOTE: These models are all in the Structures.blend file.


==============================================================================================================

SETUP:
---------
1. Create a cube
2. Shift up one meter (Z-axis) so it sits on the floor.
3. Apply location


CREATE ROCK:
---------------
1. Add a subdivision surface modifier
2. Set divisions to 4 or 5 (I used 5)
3. Add a displacement modifier
4. Set Strength to 0.6 (or adjust as desired) and leave Midlevel at 0.5.
5. Click the New button on the displacement modifier to create a new texture.
6. Switch to the texture tab and change the texture type to Voronoi
	NOTE: This texture can be used repeatedly for every rock you create, as it is only used in
	generating the geometry.
7. Set the voronoi texture's Distance Metric to Distance Squared.
8. Set its Intensity to 0.8 (adjust as desired).
9. Set its Size to 1.2.
10. Return to the modifiers tab and click on the Displace modifier's Edit Mode button.
11. Drag the bounding box vertices around to change the shape of the rock.
	NOTE: Moving the entire rock will also change the shape, as we are creating it with voronoi noise.


FINALIZE ROCK:
-----------------
1. Apply the Subdivision Surface modifier first.
2. Apply the Displace modifier.
3. Add a decimate modifier.
4. Set its type to Un-Subdivide.
5. Increase Iterations by one at a time until you are happy with the number of faces remaining.
6. Apply the Decimate modifier.


==============================================================================================================


The rest of these steps are my own.


TEXTURE THE ROCK:
--------------------
1. Apply the desired rock material to the rock.
2. Switch to Edit mode.
3. Select all faces.
4. Open the UV menu and select Cylinder Projection or Sphere Projection (sometimes one looks better than
       the other on a given rock)
5. Scale the UV map up or down a bit to adjust the amount of detail on the rock as desired.


EXPORT TO .FBX FILE
----------------------

NOTE: These steps ensure proper rotation in Unity. They come from the following URL: 
	(https://www.immersivelimit.com/tutorials/blender-to-unity-export-correct-scale-rotation)

1. Rotate -90 on X axis.
2. Apply rotation.
3. Rotate 90 on X axis.
4. Export to .FBX file.

