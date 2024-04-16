using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

using UnityEngine;

using DIY_DOOM;


namespace DIY_DOOM.Utils.Textures
{
    public static class TextureUtils
    {

        public static Texture2D CreateBlankDoomTexture(string textureName, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, true);

            texture.name = textureName;
            texture.alphaIsTransparency = DoomEngine.Settings.TextureAlphaIsTransparency;
            texture.filterMode = DoomEngine.Settings.TextureFilterMode;
            texture.wrapMode = DoomEngine.Settings.TextureWrapMode;
            texture.SetAllPixelsToColor(DoomEngine.Settings.NewTextureFillColor);

            return texture;
        }

        public static bool IsNameValid(string textureName)
        {
            return textureName != null &&
                   !string.IsNullOrWhiteSpace(textureName) && 
                   textureName != "-";
        }

    }
}