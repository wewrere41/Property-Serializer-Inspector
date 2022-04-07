using System;
using System.Collections.Generic;
using System.Reflection;


[Serializable]
public class DeserializedDataBase
{
    public static DeserializedDataBase TempDeserializedData;
    public static DeserializedDataBase CachedData;


    public HashSet<SceneData> DeserializedDataSet = new HashSet<SceneData>();
}

[Serializable]
public class SceneData
{
    public string SceneGUID;
    public HashSet<SerializedMonoData> SerializedMonoDataSet;
}

[Serializable]
public class SerializedMonoData
{
    public string GUID;
    public List<PropertyInfoData> PropertyDatas;
}

[Serializable]
public class PropertyInfoData
{
    public string TypeName;
    public string PropertyName;
    public object Value;

    public PropertyInfoData()
    {
    }

    public PropertyInfoData(PropertyInfo p, object value)
    {
        TypeName = p.DeclaringType?.AssemblyQualifiedName;
        PropertyName = p.Name;
        Value = value;
    }


    public PropertyInfo ToProperty()
    {
        return Type.GetType(TypeName)?.GetProperty(PropertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}