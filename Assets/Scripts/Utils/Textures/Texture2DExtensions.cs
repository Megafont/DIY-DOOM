using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.Utils.Textures
{
    public static class Texture2DExtensions
    {
        public static void SetAllPixelsToColor(this Texture2D texture, Color32 color)
        {
            for (int x = 0; x < texture.width; ++x)
            {
                for (int y = 0; y < texture.height; ++y)
                {
                    texture.SetPixel(x, y, color);
                }
            }

        }
    }

}