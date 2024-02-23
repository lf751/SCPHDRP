using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class MenuMouseProcessor : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static MenuMouseProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<MenuMouseProcessor>();
    }
    /*[Tooltip("Number to add to incoming values.")]
    public float valueShift = 0;*/

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        Vector2 pos = Mouse.current.position.ReadValue() + value;
        Mouse.current.WarpCursorPosition(pos);
        Debug.Log("Pos: " + pos);
        return pos;
    }
}
