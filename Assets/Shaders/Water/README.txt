I created this water shader by following this YouTube tutorial by Binary Lunar:

3D Stylized Water with Refraction and Foam Shader Graph - Unity Tutorial
https://www.youtube.com/watch?v=MHdDUqJHJxM

NOTE: In order for these shaders to work properly, you must go into Project Settings->Graphics.
      Double click on the reference to the URP settings asset at the top.
      It opens in the Unity inspector. In here, check the Depth Texture and Opaque Texture boxes.

I also created a lit version of the shader graph after I had it all working. I then went on to a number
of materials that are all using these shaders with different parameters.

----------------------------------------------------------------------------------------------------

The waterfall base ripples shader graph was created by me separately using several resources.

The following tutorial helped me create the special geometry and get ripples working in Blender
first, and then I had to figure out how to do it in Unity's shader graph. However, in Unity I ended
up using gradient noise rather than voronoi, as I couldn't get it working with voronoi.
https://www.youtube.com/watch?v=zZsfr5f273c

Next, the following tutorial helped me get it to fade out at the edges.
https://forum.unity.com/threads/make-edges-transparent.988627/

I also had to look up various nodes a number of times, and monkey around with it to iron out
all the last remaining kinks. It's not perfect, but looks pretty good with the rest of the
waterfall VFX I created.

NOTE: You may notice that given the same alpha value as the foam color on the other stylized water
      shaders, this one results in a noticeably lighter hue. This is because the stylized water shaders
      lerp the foam color and water color together. This shader isn't doing that since it can't,
      and so the resulting foam of the waterfall ripples looks a bit lighter than the river foam.
      It looks good this way anyway, so it works out! Also, this can be easily worked around by lowering
      the alpha property of the shader just a bit.

The smoothness property on the lit version of the waterfall base ripples material is set 0, because
otherwise the sun reflection doesn't quite line up right between this and the river surface beneath it.
This resolves that issue with no side effects.

NOTE: The lit version of the waterfall base ripples material has one other slight issue. If you look closely,
      you can see the edge of the circular geometry the ripples are on, as if there is as sheen. I immediately
      experimented with setting smoothness to 0, but this did not remove it for some reason as I expected.
      For the river shader, this is how you can get rid of that sheen on the surface if you want the water to
      look flat like in games like Zelda: The Wind Waker. As mentioned above, if smoothness is set to 1,
      the sun's reflection doesn't line up quite right with the one on the river's surface. Even if you set
      smoothness to 0 on both the river and waterfall base ripples materials, you can still see the edge
      of the circular geometry slightly. The unlit version of the shader does not have this issue at all.
