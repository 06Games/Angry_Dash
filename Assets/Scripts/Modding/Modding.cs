using AngryDash.Extensibility;
using System.CodeDom.Compiler;
using System.IO;
using Tools;
using UnityEngine;

public class Modding : MonoBehaviour
{
    private void Start()
    {
        foreach (string file in Directory.GetFiles(Application.persistentDataPath + "/Mods/", "*.cs")) LoadMod(file);
    }

    public void LoadMod(string filePath)
    {
        CompilerResults result = LoadScript(filePath);
        if (result.Errors.HasErrors)
        {
            StringBuilder errors = new StringBuilder();
            string filename = Path.GetFileName(filePath);
            foreach (CompilerError err in result.Errors)
            {
                errors.Append(string.Format("\r\n{0}({1},{2}): {3}: {4}",
                            filename, err.Line, err.Column,
                            err.ErrorNumber, err.ErrorText));
            }
            string str = "Error loading script\r\n" + errors.ToString();
            throw new System.ApplicationException(str);
        }
        else
        {
            GetPlugins(result.CompiledAssembly);
        }
    }

    private CompilerResults LoadScript(string filepath)
    {
        string language = CodeDomProvider.GetLanguageFromExtension(Path.GetExtension(filepath));
        CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(language);
        CompilerParameters compilerParams = new CompilerParameters();
        compilerParams.GenerateExecutable = false;
        compilerParams.GenerateInMemory = true;
        compilerParams.IncludeDebugInformation = false;

#if UNITY_EDITOR
        string dllPath = Application.dataPath + "/../Library/";
        compilerParams.ReferencedAssemblies.Add($"{dllPath}ScriptAssemblies/Assembly-CSharp.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityAssemblies/UnityEngine.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityAssemblies/UnityEngine.UI.dll");
#else
        string dllPath = Application.dataPath + "/Managed/";
        compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityEngine.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityEngine.UI.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}Assembly-CSharp.dll");
#endif

        return codeDomProvider.CompileAssemblyFromFile(compilerParams, filepath);
    }

    private void GetPlugins(System.Reflection.Assembly assembly)
    {
        foreach (System.Type type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsNotPublic) continue;
            System.Type[] interfaces = type.GetInterfaces();
            if (((System.Collections.Generic.IList<System.Type>)interfaces).Contains(typeof(IScript)))
            {
                IScript iScript = (IScript)System.Activator.CreateInstance(type);
                iScript.Start();

                Debug.Log(string.Format("{0} ({1})\r\n", iScript.Name, iScript.Description));
            }
        }
    }
}
