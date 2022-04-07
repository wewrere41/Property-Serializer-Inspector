using System.Linq;
using UnityEditor;

public class ScriptPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromPath)
    {
        for (int i = 0; i < movedAssets.Length; i++)
        {
            if (movedFromPath.Last().EndsWith(".cs"))
            {
                var movedFrom = movedFromPath.First().Split('/');
                var movedFromRootArray = movedFrom.Take(movedFrom.Length - 1).ToArray();
                var movedFromRootPath = string.Join("/", movedFromRootArray);


                var moved = movedAssets.First().Split('/');
                var movedRootArray = moved.Take(moved.Length - 1).ToArray();
                var movedRootPath = string.Join("/", movedRootArray);

                if (movedRootPath == movedFromRootPath)
                {
                    var oldName = movedFrom.Last().Split('.').First();
                    var newName = moved.Last().Split('.').First();

                    EditorSerializerHelper.ChangeScriptName(oldName, newName);
                }
            }
        }
    }
}