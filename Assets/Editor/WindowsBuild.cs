using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class WindowsBuild : MonoBehaviour
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.StandaloneWindows | buildTarget == BuildTarget.StandaloneWindows64)
        {
            FileInfo build = new FileInfo(pathToBuiltProject);
            Directory.Delete(build.DirectoryName + Path.DirectorySeparatorChar + "Angry Dash_BackUpThisFolder_ButDontShipItWithYourGame", true);
        }
    }
}