using System.Reflection;
using UnityEngine;

public class GuidHelper
{
    private const string ObjectGuid = "GUID";
    private const string SceneGuid = "SceneGUID";

    public static string GetGUID(Object target)
    {
        return (string)typeof(SerializedMonoBehaviour)
            .GetField(ObjectGuid, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(target);
    }

    public static void SetNewGUID(object target, string GUID)
    {
        typeof(SerializedMonoBehaviour)
            .GetField(ObjectGuid, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, GUID);
    }


    public static string GetSceneGUID(Object target)
    {
        return (string)typeof(SerializedMonoBehaviour)
            .GetField(SceneGuid, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(target);
    }

    public static void SetNewSceneGUID(object target, string GUID)
    {
        typeof(SerializedMonoBehaviour)
            .GetField(SceneGuid, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, GUID);
    }
}