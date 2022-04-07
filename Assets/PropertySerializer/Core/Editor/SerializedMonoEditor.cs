using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(SerializedMonoBehaviour), true), CanEditMultipleObjects]
public class SerializedMonoEditor : Editor
{
    private PropertyInfo[] _propertyInfos;
    private static string SceneGUID;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var properties = target.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


        if (_propertyInfos == null)
        {
            _propertyInfos = properties
                .Where(p => p.GetCustomAttributes(typeof(SerializedPropertyAttribute), true).Any()).ToArray();
            DeserializedDataBase.TempDeserializedData ??= new DeserializedDataBase();
        }


        if (_propertyInfos != null)
        {
            foreach (var propertyInfo in _propertyInfos)
            {
                if (targets.Length != 1 && !targets.All(t =>
                    {
                        var valuaA = propertyInfo.GetValue(t);
                        var valueB = propertyInfo.GetValue(targets[0]);

                        return valuaA != null && valuaA.GetType().IsValueType
                            ? valuaA.Equals(valueB)
                            : Equals(valuaA, valueB);
                    }))
                {
                    EditorGUI.showMixedValue = true;
                }


                var currentValue = propertyInfo.GetValue(target, null);

                EditorGUI.BeginChangeCheck();
                var nextValue = propertyInfo.GetValueAndDrawProperty(currentValue);
                EditorGUI.showMixedValue = false;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(target, "_");

                    foreach (var currentTarget in targets)
                    {
                        propertyInfo.SetValue(currentTarget, nextValue);

                        if (EditorApplication.isPlaying is false)
                        {
                            var dataHolder = DeserializedDataBase.TempDeserializedData.DeserializedDataSet
                                .FirstOrDefault(x => x.SceneGUID == SceneGUID)
                                ?.SerializedMonoDataSet
                                .FirstOrDefault(x => x.GUID == GuidHelper.GetGUID(currentTarget));

                            //Target Have PropertyData
                            if (dataHolder?.PropertyDatas.FirstOrDefault(x =>
                                    x.PropertyName == propertyInfo.Name)
                                is { } property)

                            {
                                property.Value = nextValue;
                            }

                            //Target Have not PropertyData
                            else
                            {
                                dataHolder?.PropertyDatas.Add(new PropertyInfoData(propertyInfo, nextValue));
                            }
                        }
                    }

                    JsonUtility.SaveEditorData();
                }
            }
        }
    }


    /// <summary>
    /// This func called when object deleted or duplicated
    /// </summary>
    private void OnDestroy()
    {
        RemoveEmptySerializedDatas();
        RemoveEmptyProperties();
        SolveDuplicatedObjects();
        JsonUtility.SaveEditorData();
    }

    [InitializeOnLoadMethod]
    private static void OnRecompile()
    {
        SceneGUID ??= AssetDatabase.GUIDFromAssetPath(SceneManager.GetActiveScene().path).ToString();


        ///When component attached to object
        ObjectFactory.componentWasAdded += component =>
        {
            if (component.GetType().BaseType == typeof(SerializedMonoBehaviour))
            {
                DeserializedDataBase.TempDeserializedData ??= new DeserializedDataBase();
                var currentDeserializedData = DeserializedDataBase.TempDeserializedData;


                //Guid setup
                var guid = Guid.NewGuid().ToString();
                GuidHelper.SetNewGUID(component, guid);
                GuidHelper.SetNewSceneGUID(component, SceneGUID);

                var currentSceneData = currentDeserializedData.DeserializedDataSet.FirstOrDefault(x =>
                    x.SceneGUID == SceneGUID);

                //Dataset has current scene data
                if (currentSceneData != null)
                {
                    currentSceneData.SerializedMonoDataSet.Add(new SerializedMonoData
                    {
                        GUID = guid,
                        PropertyDatas = new List<PropertyInfoData>()
                    });
                }
                else
                {
                    currentDeserializedData.DeserializedDataSet.Add(
                        new SceneData
                        {
                            SceneGUID = SceneGUID,
                            SerializedMonoDataSet = new HashSet<SerializedMonoData>
                            {
                                new SerializedMonoData
                                {
                                    GUID = guid,
                                    PropertyDatas = new List<PropertyInfoData>()
                                }
                            },
                        });
                }
            }
        };

        //Deserialize data when scene opened
        EditorSceneManager.sceneOpening +=
            (arg0, mode) => DeserializedDataBase.TempDeserializedData = JsonUtility.TryDeserializeAndGetData();

        //Get scene guid when scene opened
        EditorSceneManager.sceneOpened += (scene, mode) =>
            SceneGUID = AssetDatabase.GUIDFromAssetPath(SceneManager.GetActiveScene().path).ToString();


        //Serialize data when scene saved
        EditorSceneManager.sceneSaved += scene =>
        {
            JsonUtility.SerializeData(DeserializedDataBase.TempDeserializedData);
            JsonUtility.SaveEditorData();
        };


        //Normally uses static data temporarily.
        //But since the static data is reset when recompile ,then it keeps the editor data as a backup.  
        DeserializedDataBase.TempDeserializedData ??= JsonUtility.TryDeserializeEditorData();


        //Before recompile some properties may have been deleted from scripts.
        RemoveEmptyProperties();
    }


    /// <summary>
    /// When any object deleted in scene remove it from SerializedDataSet
    /// </summary>
    private void RemoveEmptySerializedDatas()
    {
        if (EditorApplication.isPlaying is false)
        {
            var sceneObjects = FindObjectsOfType<SerializedMonoBehaviour>();

            var currentSceneData = DeserializedDataBase.TempDeserializedData.DeserializedDataSet.FirstOrDefault(x =>
                x.SceneGUID == SceneGUID);
            foreach (var target in targets)
            {
                if (target == null && sceneObjects.Any(x => x.GetInstanceID() == target.GetInstanceID()) is false)
                {
                    var serializedMonoData = currentSceneData?.SerializedMonoDataSet
                        .FirstOrDefault(x => x.GUID == GuidHelper.GetGUID(target));
                    currentSceneData?.SerializedMonoDataSet.Remove(serializedMonoData);
                }
            }

            if (currentSceneData?.SerializedMonoDataSet.Count == 0)
            {
                DeserializedDataBase.TempDeserializedData.DeserializedDataSet.Remove(currentSceneData);
            }
        }
    }


    /// <summary>
    /// When any SerializedProperty deleted in script remove it from current datas
    /// </summary>
    private static void RemoveEmptyProperties()
    {
        //For a editor(not serialized yet) data
        var tempDeserializedData = DeserializedDataBase.TempDeserializedData;
        for (var i = 0; i < tempDeserializedData?.DeserializedDataSet.Count; i++)
        {
            var sceneData = tempDeserializedData.DeserializedDataSet.ElementAt(i);

            foreach (var serializedMonoData in sceneData.SerializedMonoDataSet)
            {
                serializedMonoData.PropertyDatas.RemoveAll(x => x.ToProperty() == null);
            }
        }

        //For a already serialized data cleanup

        var serializedData = JsonUtility.TryDeserializeAndGetData();

        if (serializedData?.DeserializedDataSet?.Count > 0)
        {
            for (int i = 0; i < serializedData.DeserializedDataSet.Count; i++)
            {
                var currentDataHolder = serializedData.DeserializedDataSet.ElementAt(i);

                foreach (var serializedMonoData in currentDataHolder.SerializedMonoDataSet)
                {
                    serializedMonoData.PropertyDatas.RemoveAll(x => x.ToProperty() == null);
                }
            }

            JsonUtility.SerializeData(serializedData);
        }
    }


    /// <summary>
    /// When any object(s) duplicated with ctrl+d etc , create new GUID for it
    /// </summary>
    private void SolveDuplicatedObjects()
    {
        var sceneObjects = FindObjectsOfType<SerializedMonoBehaviour>();

        var selectedMonoBehaviours = Selection.gameObjects.Where(x => x.GetComponent<SerializedMonoBehaviour>() != null)
            .SelectMany(x => x.GetComponents<SerializedMonoBehaviour>());

        var sceneData = DeserializedDataBase.TempDeserializedData.DeserializedDataSet.FirstOrDefault(x =>
            x.SceneGUID == SceneGUID);
        foreach (var o in selectedMonoBehaviours)
        {
            var duplicatedCount = sceneObjects.Count(x => GuidHelper.GetGUID(x) == GuidHelper.GetGUID(o));
            if (duplicatedCount > 1)
            {
                var duplicatedObject = sceneObjects.FirstOrDefault(x => GuidHelper.GetGUID(x) == GuidHelper.GetGUID(o));
                var currentDataHolder = sceneData.SerializedMonoDataSet.FirstOrDefault(x =>
                    x.GUID == GuidHelper.GetGUID(duplicatedObject));
                CreateNewDataHolderAndCopyFields(duplicatedObject, currentDataHolder);
            }
        }

        void CreateNewDataHolderAndCopyFields(object duplicatedObject,
            SerializedMonoData currentDataHolder)
        {
            var guid = Guid.NewGuid().ToString();
            GuidHelper.SetNewGUID(duplicatedObject, guid);
            var dataHolder = new SerializedMonoData
            {
                GUID = guid,
                PropertyDatas = new List<PropertyInfoData>()
            };

            for (var i = 0; i < currentDataHolder.PropertyDatas.Count; i++)
            {
                var propertyInfo = currentDataHolder.PropertyDatas[i];
                dataHolder.PropertyDatas.Add(new PropertyInfoData(propertyInfo.ToProperty(),
                    propertyInfo.Value));
            }

            sceneData.SerializedMonoDataSet.Add(dataHolder);
        }
    }
}