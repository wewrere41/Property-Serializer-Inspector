using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorExtension
{
    public static object GetValueAndDrawProperty(this PropertyInfo propertyInfo, object currentValue)
    {
        return propertyInfo.PropertyType switch
        {
            Type t when t.BaseType == typeof(Enum) => EditorGUILayout.EnumPopup(propertyInfo.Name, (Enum)currentValue),
            Type t when t == typeof(bool) => EditorGUILayout.Toggle(propertyInfo.Name, (bool)currentValue),
            Type t when t == typeof(string) => EditorGUILayout.TextField(propertyInfo.Name, (string)currentValue),
            Type t when t == typeof(int) => EditorGUILayout.IntField(propertyInfo.Name, (int)currentValue),
            Type t when t == typeof(float) => EditorGUILayout.FloatField(propertyInfo.Name, (float)currentValue),
            Type t when t == typeof(Vector2) => EditorGUILayout.Vector2Field(propertyInfo.Name, (Vector2)currentValue),
            Type t when t == typeof(Vector3) => EditorGUILayout.Vector3Field(propertyInfo.Name, (Vector3)currentValue),
            _ => null
        };
    }
}