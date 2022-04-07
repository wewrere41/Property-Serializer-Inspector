using System;
using UnityEngine;

public class SerializedTestB : SerializedMonoBehaviour

{
    private enum Enums
    {
        NONE,
        IDLE
    }

    [SerializedProperty]
    private Enums _enumProperty
    {
        get => _enumPropertyField;
        set => _enumPropertyField = value;
    }


    [SerializedProperty]
    private bool _boolProperty
    {
        get => _boolField;
        set => _boolField = value;
    }

    [SerializedProperty]
    private string _stringProperty
    {
        get => _stringField;
        set => _stringField = value;
    }


    [SerializedProperty]
    public int _intProperty

    {
        get => _intPropertyField;
        set { _intPropertyField = value; }
    }


    [SerializedProperty]
    private float _floatProperty
    {
        get => _floatPropertyField;
        set => _floatPropertyField = value;
    }

    [SerializedProperty]
    private Vector2 _v2Property
    {
        get => _v2Field;
        set => _v2Field = value;
    }

    [SerializedProperty]
    private Vector3 _v3Property
    {
        get => _v3Field;
        set => _v3Field = value;
    }

    #region FIELDS

    private Enums _enumPropertyField;
    private bool _boolField;
    private string _stringField;
    private int _intPropertyField;
    private float _floatPropertyField;
    private Vector2 _v2Field;
    private Vector3 _v3Field;

    #endregion
}