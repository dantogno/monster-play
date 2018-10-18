using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableManager : MonoBehaviour
{
    private static BreakableManager instance;
    private Slice2DLayer slice2DLayer;
    public Slice2DLayer Slice2DLayer => slice2DLayer;
    public static BreakableManager Instance
    {
        get
        {
            if (instance == null)
            {
                var gameObject = new GameObject(name: nameof(BreakableManager));
                instance = gameObject.AddComponent<BreakableManager>();
                DontDestroyOnLoad(gameObject);
                instance.slice2DLayer = Slice2DLayer.Create();
            }
            return instance;
        }
    }
}
