using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.WADs;
using DIY_DOOM.Maps;
using System.Xml.Linq;


namespace DIY_DOOM
{
    public class AssetManager
    {
        public static AssetManager Instance;


        private WAD_Loader _WAD_Loader;

        private List<PaletteDef> _Palettes;
        private Dictionary<string, Patch> _RawTexturePatchDataLookup;
        private Dictionary<string, Texture2D> _TextureLookup;



        public AssetManager(WAD_Loader wadLoader)
        {
            if (wadLoader == null)
                throw new ArgumentNullException("The passed in WAD_Loader is null!");


            if (Instance != null)
            {
                // We do not call GameObject.Destroy() since this class is not a Unity object.
                // There are no references being created to it since we simply return after logging an error.
                // Thus the garbage collector should get rid of this object for us.
                Debug.LogError("There is already an AssetManager present. Self destructing!");
                return;
            }


            Instance = this;


            _WAD_Loader = wadLoader;


            _Palettes = new List<PaletteDef>();
            _RawTexturePatchDataLookup = new Dictionary<string, Patch>();
            _TextureLookup = new Dictionary<string, Texture2D>();
        }


        /// <summary>
        /// Clears all assets out of the asset manager.
        /// </summary>
        public void Clear()
        {
            _Palettes.Clear();

            _TextureLookup.Clear();
            _RawTexturePatchDataLookup.Clear();
        }

        public bool AddPalette(PaletteDef palette)
        {
            if (!_Palettes.Contains(palette))
            {
                _Palettes.Add(palette);
                return true;
            }
            else
            {
                Debug.LogError($"AssetManager failed to add palette, as it has already been added.");
                return false;
            }
        }

        public bool AddTexture(string name, Texture2D texture)
        {
            try
            {
                _TextureLookup.Add(name, texture);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AssetManager failed to add texture \"{name}\" due to this error: \"{e.Message}\"");
                return false;
            }

        }

        public Texture2D GetTexture(string name, int paletteIndex)
        {
            string patchLookupName = BuildPatchLookupName(name, paletteIndex);


            if (_TextureLookup.TryGetValue(patchLookupName, out Texture2D texture))
            {
                return texture;
            }
            else
            {
                if (_RawTexturePatchDataLookup.TryGetValue(name, out Patch patch))
                {
                    _TextureLookup.Add(patchLookupName, patch.RenderToTexture2D(_Palettes[paletteIndex]));

                    return _TextureLookup[patchLookupName];
                }

                // The raw patch data for the request texture is not present in the lookup, so load it.
                else if (_WAD_Loader.ReadPatchData(name, out Patch loadedPatch))
                {
                    _RawTexturePatchDataLookup.Add(name, loadedPatch);
                    _TextureLookup.Add(patchLookupName, 
                                       loadedPatch.RenderToTexture2D(_Palettes[paletteIndex]));

                    return _TextureLookup[patchLookupName];
                }
            }


            Debug.Log($"AssetManager failed to get texture \"{patchLookupName}\"!");
            return null;
        }

        private string BuildPatchLookupName(string patchName, int paletteIndex)
        {
            return patchName + "_" + paletteIndex.ToString();
        }

        private string GetInfoFromPatchLookupName(string patchNameWithIndex, out int paletteIndex)
        {

            int lastUnderscoreIndex = patchNameWithIndex.LastIndexOf('_');
            int temp = lastUnderscoreIndex + 1;
            paletteIndex = int.Parse(patchNameWithIndex.Substring(temp, patchNameWithIndex.Length - temp));

            return patchNameWithIndex.Substring(0, lastUnderscoreIndex + 1);
        }

    } // end class AssetManager

}
