using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class EditorSerializerHelper
{
    public static void ChangeScriptName(string oldName, string newName)
    {
        //For a editor(not serialized yet) data
        var tempDeserializedData = DeserializedDataBase.TempDeserializedData;
        ChangeTypeName(tempDeserializedData);

        //For a already serialized data 
        var serializedData = JsonUtility.TryDeserializeAndGetData();
        ChangeTypeName(serializedData);

        JsonUtility.SerializeData(serializedData);
        JsonUtility.SaveEditorData();

        
        void ChangeTypeName(DeserializedDataBase deserializedDataBase)
        {
            foreach (var serializedDataPropertyData in deserializedDataBase.DeserializedDataSet.Select(serializedData =>
                         serializedData.SerializedMonoDataSet.Where(x =>
                             x.PropertyDatas.Any(x => x.TypeName.Split(',')[0] == oldName)).ToList()).SelectMany(
                         oldNamedDatas => oldNamedDatas.SelectMany(serializedMonoData =>
                             serializedMonoData.PropertyDatas)))
            {
                serializedDataPropertyData.TypeName = serializedDataPropertyData.TypeName.Replace(oldName, newName);
            }
        }
    }


    public static void RemoveDeletedSceneData(string sceneGuid)
    {
        var currentDeserializedData = DeserializedDataBase.TempDeserializedData;

        if (currentDeserializedData == null) return;

        currentDeserializedData.DeserializedDataSet
            .Remove(currentDeserializedData.DeserializedDataSet.FirstOrDefault(
                x => x.SceneGUID == sceneGuid));
        JsonUtility.SerializeData(currentDeserializedData);
    }

    public static async Task DuplicateSceneData(string path)
    {
        await Task.Yield();
        var currentScenePath = SceneManager.GetActiveScene().path;
        var duplicatedScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        SolveDuplicatedSceneObjects(currentScenePath, duplicatedScene);
    }

    private static void SolveDuplicatedSceneObjects(string currentScenePath, Scene duplicatedScene)
    {
        var tempData = DeserializedDataBase.TempDeserializedData;

        var toBeCopiedSceneData =
            tempData?.DeserializedDataSet.FirstOrDefault(x =>
                x.SceneGUID == GuidHelper.GetSceneGUID(Object.FindObjectOfType<SerializedMonoBehaviour>()));

        if (toBeCopiedSceneData != null)
        {
            var newSceneData = new SceneData
            {
                SceneGUID = AssetDatabase.GUIDFromAssetPath(duplicatedScene.path).ToString(),
                SerializedMonoDataSet = new HashSet<SerializedMonoData>()
            };

            var serializedMonoBehaviours = Object.FindObjectsOfType<SerializedMonoBehaviour>();


            var pairedDatas = toBeCopiedSceneData.SerializedMonoDataSet
                .Join(serializedMonoBehaviours, x => x.GUID,
                    GuidHelper.GetGUID, (x, y)
                        => new { x, y });


            var duplicatedSceneGuid = AssetDatabase.GUIDFromAssetPath(duplicatedScene.path).ToString();

            foreach (var pairedData in pairedDatas)
            {
                CreateNewMonoDataAndCopyFields(pairedData.x, pairedData.y);
            }


            tempData.DeserializedDataSet.Add(newSceneData);

            EditorSceneManager.SaveScene(duplicatedScene);
            EditorSceneManager.OpenScene(currentScenePath);


            void CreateNewMonoDataAndCopyFields(SerializedMonoData currentDataHolder, SerializedMonoBehaviour x)
            {
                var guid = Guid.NewGuid().ToString();
                GuidHelper.SetNewGUID(x, guid);
                GuidHelper.SetNewSceneGUID(x, duplicatedSceneGuid);
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

                newSceneData.SerializedMonoDataSet.Add(dataHolder);
            }
        }
    }
}