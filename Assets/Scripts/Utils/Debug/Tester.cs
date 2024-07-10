using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIY_DOOM
{
    public static class Tester 
    {
        public static void RunTests()
        {
            Debug.Log(new string('=', 128));

            MathTests.DoMathTests();
            TextureTests.DoTextureTests();

            Debug.Log(new string('=', 128));
        }
    }
}
