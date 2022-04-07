using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using UnityEditor;
using UnityEngine;

public static class JsonUtility
{
    private static JsonSerializer _jsonSerializer;


#if UNITY_EDITOR

    private static readonly string _dataPath = Path.Combine(GetScriptPath() + "/Resources", "Data.json");

    #region MenuItems

    [MenuItem("Tools/SerializedProperty/Clear Serialized Data")]
    private static void ResetData()
    {
        DeserializedDataBase.TempDeserializedData = new DeserializedDataBase();
        SerializeData(DeserializedDataBase.TempDeserializedData);
    }

    [MenuItem("Tools/SerializedProperty/Open Serialized Data")]
    private static void OpenSerializedData()
    {
        Process.Start("notepad.exe", _dataPath);
    }

    #endregion

    public static DeserializedDataBase TryDeserializeAndGetData()
    {
        return File.Exists(_dataPath)
            ? string.IsNullOrEmpty(File.ReadAllText(_dataPath)) ? new DeserializedDataBase() : DeserializeAndGetData()
            : null;
    }

    private static DeserializedDataBase DeserializeAndGetData()
    {
        using var reader = new StreamReader(_dataPath);
        var jsonTextReader = new JsonTextReader(reader);
        _jsonSerializer ??= JsonSerializer.Create(JsonSerializerSettings());
        return _jsonSerializer.Deserialize(jsonTextReader, typeof(DeserializedDataBase)) as DeserializedDataBase;
    }

    public static void SerializeData(DeserializedDataBase currentDeserializedData)
    {
        if (File.Exists(_dataPath) is false)
        {
            var rootPath = GetScriptPath() + "/Resources";
            if (Directory.Exists(rootPath) is false)
            {
                AssetDatabase.CreateFolder(GetScriptPath(), "Resources");
            }
        }

        using var writer = new StreamWriter(_dataPath);
        using var jsonWriter = new JsonTextWriter(writer);
        _jsonSerializer ??= JsonSerializer.Create(JsonSerializerSettings());
        _jsonSerializer.Serialize(jsonWriter, currentDeserializedData);
        jsonWriter.Flush();
    }


    public static DeserializedDataBase TryDeserializeEditorData()
    {
        var dataString = EditorPrefs.GetString("Data");
        var jsonTextReader = new JsonTextReader(new StringReader(dataString));
        _jsonSerializer ??= JsonSerializer.Create(JsonSerializerSettings());
        return _jsonSerializer.Deserialize(jsonTextReader, typeof(DeserializedDataBase)) as DeserializedDataBase;
    }

    public static void SaveEditorData()
    {
        _jsonSerializer ??= JsonSerializer.Create(JsonSerializerSettings());

        var serializeObject =
            JsonConvert.SerializeObject(DeserializedDataBase.TempDeserializedData, JsonSerializerSettings());
        EditorPrefs.SetString("Data", serializeObject);
    }

    public static string GetScriptPath()
    {
        var scriptGuid = AssetDatabase.FindAssets($"{nameof(JsonUtility)} t:script").FirstOrDefault();
        if (scriptGuid != null)

        {
            var assetPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
            var path = assetPath.Split('/').Take(assetPath.Split('/').Length - 1).ToArray();
            var rootPath = string.Join("/", path);

            return rootPath;
        }

        return null;
    }


#endif



#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void DeserializeOnBuild()
    {
        //Ahead of time hashset deserialization support 
        AotHelper.EnsureList<SceneData>();
        AotHelper.EnsureList<SerializedMonoData>();


        var file = Resources.Load<TextAsset>("Data");

        DeserializedDataBase.CachedData =
            JsonConvert.DeserializeObject<DeserializedDataBase>(file.text, JsonSerializerSettings());
    }
#endif


    #region SerializerSettings

    private static JsonSerializerSettings JsonSerializerSettings()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new JsonConverter[]
            {
                new StringEnumConverter()
            },

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
            //Formatting = Formatting.Indented, For a lower memory usage
        };
        return settings;
    }

    #endregion
}