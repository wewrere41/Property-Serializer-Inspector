using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;

public class AssetModificationHelper : UnityEditor.AssetModificationProcessor
{
    private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
    {
        //Remove scene data when scene removed.
        if (path.EndsWith(".unity") && opt == RemoveAssetOptions.MoveAssetToTrash)
        {
            var sceneGuid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            EditorSerializerHelper.RemoveDeletedSceneData(sceneGuid);
        }

        //When Resources folder or json deleted, serialize data 
        if (path.StartsWith(JsonUtility.GetScriptPath() + "/Resources") && opt == RemoveAssetOptions.MoveAssetToTrash)
        {
            _ = SerializeTempData();

            static async Task SerializeTempData()
            {
                await Task.Yield();
                JsonUtility.SerializeData(DeserializedDataBase.TempDeserializedData);
            }
        }

        return AssetDeleteResult.DidNotDelete;
    }


    private static void OnWillCreateAsset(string assetName)
    {
        if (assetName.EndsWith(".unity"))
        {
            _ = EditorSerializerHelper.DuplicateSceneData(assetName);
        }
    }

  
    /// Deserialize tempData after build. 
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        DeserializedDataBase.TempDeserializedData = JsonUtility.TryDeserializeAndGetData();
    }
}