using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Linq;

public class GenericBuild : MonoBehaviour
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        foreach (var dir in Directory.GetDirectories(Application.dataPath + "/../Library/", "il2cpp_*", SearchOption.TopDirectoryOnly)) Directory.Delete(dir, true);
        Directory.Delete(Application.dataPath + "/../Temp/StagingArea/", true);
    }
}