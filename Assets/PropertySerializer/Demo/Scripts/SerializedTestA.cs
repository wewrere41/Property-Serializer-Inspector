using UnityEngine;

public class SerializedTestA : SerializedMonoBehaviour
{
    private enum Enums
    {
        NONE,
        IDLE
    }

    [SerializedProperty] private Enums _enumProperty { get; set; }

    [SerializedProperty] private bool _boolProperty { get; set; }

    [SerializedProperty] private string _stringProperty { get; set; }

    [SerializedProperty] public int _intProperty { get; set; }

    [SerializedProperty] private float _floatProperty { get; set; }

    [SerializedProperty] private Vector2 _v2Property { get; set; }

    [SerializedProperty] private Vector3 _v3Property { get; set; }

  
}

