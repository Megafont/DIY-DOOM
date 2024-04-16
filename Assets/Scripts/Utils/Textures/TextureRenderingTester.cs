using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class TextureRenderingTester : MonoBehaviour
{
    public static TextureRenderingTester Instance { get; private set; }



    [Tooltip("The prefab used for texture test objects.")]
    [SerializeField]
    private GameObject _TextureTestObjectPrefab;

    [Tooltip("The amount of spacing (in Unity units) betweem the centers of the test objects.")]
    [SerializeField]
    private float _Spacing = 3f;


    private List<GameObject> _TextureTestObjects = new List<GameObject>();



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("An instance of TextureRenderingTester already exists. Self destructing...");

            Destroy(gameObject);
            return;
        }


        Instance = this;


        Spacing = _Spacing;
        TextureTestObjectPrefab = _TextureTestObjectPrefab;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateTextureRenderingTestObject(Texture2D texture)
    {
        GameObject testObj = Instantiate(TextureTestObjectPrefab, Instance.transform);

        testObj.transform.Translate(Spacing * _TextureTestObjects.Count, 0, 0);
        testObj.name = $"TextureTestObject ({texture.name})";
        testObj.GetComponentInChildren<Renderer>().material.mainTexture = texture;

        _TextureTestObjects.Add(testObj);
        TextureTestObjectCount = _TextureTestObjects.Count;
    }

    public static void CreateTextureRenderingTestDisplay(Texture2D texture)
    {
        Instance.CreateTextureRenderingTestObject(texture);
    }



    public static float Spacing { get; private set; }
    public static GameObject TextureTestObjectPrefab { get; private set; }
    public static int TextureTestObjectCount { get; private set; }
    
}
