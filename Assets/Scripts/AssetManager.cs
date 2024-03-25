using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.WADs;
using DIY_DOOM.WADs.Data.Textures;


namespace DIY_DOOM
{
    public class AssetManager
    {
        public static AssetManager Instance;


        private WAD_Loader _WAD_Loader;

        private List<Palette> _Palettes;
        private List<string> _PatchNames;

        private Dictionary<string, Patch> _RawPatchDataLookup;
        private Dictionary<string, TextureData> _RawTextureDataLookup;
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


            _Palettes = new List<Palette>();
            _PatchNames = new List<string>();

            _RawPatchDataLookup = new Dictionary<string, Patch>();
            _RawTextureDataLookup = new Dictionary<string, TextureData>();

            _TextureLookup = new Dictionary<string, Texture2D>();
        }


        /// <summary>
        /// Clears all assets out of the asset manager.
        /// </summary>
        public void Clear()
        {
            _Palettes.Clear();

            _RawPatchDataLookup.Clear();
            _RawTextureDataLookup.Clear();

            _TextureLookup.Clear();
        }

        public bool AddPalette(Palette palette)
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

        public bool AddPatchName(string patchName)
        {
            if (!_PatchNames.Contains(patchName))
            {
                _PatchNames.Add(patchName);

                return true;
            }
            else
            {
                Debug.LogError($"AssetManager failed to add patch name \"{patchName}\", as it has already been added.");
                return false;
            }
        }

        public bool AddRawPatchData(string name, Patch patch)
        {
            try
            {                
                _RawPatchDataLookup.Add(name, patch);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AssetManager failed to add raw patch data for patch \"{name}\" due to this error: \"{e.Message}\"");
                return false;
            }

        }

        public bool AddRawTextureData(string name, TextureData textureData)
        {
            try
            {                
                _RawTextureDataLookup.Add(name, textureData);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AssetManager failed to add raw texture data for texture \"{name}\" due to this error: \"{e.Message}\"");
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
            string textureLookupName = BuildPatchLookupName(name, paletteIndex);


            if (_TextureLookup.TryGetValue(textureLookupName, out Texture2D texture))
            {
                return texture;
            }
            else
            {
                // Check if there is a texture data entry with the specified name.
                if (_RawTextureDataLookup.TryGetValue(name, out TextureData textureData))
                {
                    texture = textureData.RenderToTexture2D(_Palettes[paletteIndex]);
                    _TextureLookup.Add(textureLookupName, texture);
                    return texture;
                }

                // Check if there is a patch in the raw patch lookup with the specified name. If so, generate the texture, add it to the textures list, and return.
                else if (_RawPatchDataLookup.TryGetValue(name, out Patch patch))
                {
                    _TextureLookup.Add(textureLookupName, patch.RenderToTexture2D(_Palettes[paletteIndex]));

                    return _TextureLookup[textureLookupName];
                }

                // No raw patch data was found with the specified name, so try to load one. If this works, add it to the raw patches lookup, generate the texture, add it to the textures list, and return.
                else if (_WAD_Loader.ReadPatchData(name, out Patch loadedPatch))
                {
                    // NOTE: We don't add loadedPatch to _RawPatchDataLookup here, as the ReadPatchData() function already did that.

                    _TextureLookup.Add(textureLookupName,
                                       loadedPatch.RenderToTexture2D(_Palettes[paletteIndex]));

                    return _TextureLookup[textureLookupName];
                }

            }


            Debug.Log($"AssetManager failed to get texture \"{textureLookupName}\"!");
            return null;
        }

        public Palette GetPalette(int paletteIndex)
        {
            return _Palettes[paletteIndex];
        }

        public string GetPatchName(int index)
        {
            return _PatchNames[index];
        }

        public Patch GetRawPatchData(string name)
        {
            return _RawPatchDataLookup[name];
        }

        public TextureData GetRawTextureData(string name)
        {
            return _RawTextureDataLookup[name];
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



        public int PaletteCount { get { return _Palettes.Count; } } 
        public int PatchCount { get { return _TextureLookup.Values.Count; } }
        public int PatchNamesCount { get { return _PatchNames.Count; } }

    } // end class AssetManager

}
