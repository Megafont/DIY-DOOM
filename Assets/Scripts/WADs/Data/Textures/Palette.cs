using UnityEngine;


namespace DIY_DOOM.WADs.Data.Textures
{
    // TEXTURE RELATED TYPES
    // ========================================================================================================================================================================================================

    /// <summary>
    /// This struct holds a 256 color DOOM palette.
    /// </summary>
    public struct Palette
    {
        private Color32[] _Colors;



        public Palette(Color32[] colors)
        {
            _Colors = colors;
        }



        public Color32 this[int i]
        {
            get
            {
                return _Colors[i];
            }
            set
            {
                _Colors[i] = value;
            }
        }
    }


}