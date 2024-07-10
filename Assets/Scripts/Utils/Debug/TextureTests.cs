using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIY_DOOM
{
    public static class TextureTests
    {
        public static void DoTextureTests()
        {
            Debug.Log("");
            Debug.Log("BEGIN TEXTURE TESTS");
            Debug.Log(new string('-', 128));


            AssetManager assetManager = AssetManager.Instance;

            TextureRenderingTester.CreateTextureRenderingTestDisplay(assetManager.GetTexture("BROWN144", 0));
            TextureRenderingTester.CreateTextureRenderingTestDisplay(assetManager.GetTexture("LITE3", 0));
            TextureRenderingTester.CreateTextureRenderingTestDisplay(assetManager.GetTexture("BRNBIGC", 0));
            TextureRenderingTester.CreateTextureRenderingTestDisplay(assetManager.GetTexture("FLOOR0_1", 0));
            TextureRenderingTester.CreateTextureRenderingTestDisplay(assetManager.GetTexture("NUKAGE3", 0));

            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("PISGA0", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("WALL00_6", 0);

            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("AASTINKY", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BROWN1", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BROWNPIP", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BRNBIGC", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BIGDOOR1", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BIGDOOR2", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BIGDOOR4", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("COMP2", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BRNSMAL1", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BRNBIGC", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BRNPOIS", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("BRNPOIS2", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("EXITDOOR", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("SKY1", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("TEKWALL5", 0);
            //_Settings.TextureTestObject_1.material.mainTexture = assetManager.GetTexture("SW1DIRT", 0);

            //_Settings.TextureTestObject_2.material.mainTexture = assetManager.GetTexture("LITE3", 0);


        }
    }
}
