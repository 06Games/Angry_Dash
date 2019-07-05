using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Tools;
using UnityEngine;

namespace AngryDash.Mod
{
    public class Modding : MonoBehaviour
    {
        private void Start()
        {
            foreach (string file in Directory.GetFiles(Application.persistentDataPath + "/Mods/", "*.cs")) LoadMod(file);
        }

        public void LoadMod(string filePath) { LoadMod(File.ReadAllText(filePath), Path.GetFileName(filePath)); }
        public void LoadMod(string code, string filename)
        {
            CompilerResults result = LoadScript(code);
            if (result.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder();
                foreach (CompilerError err in result.Errors)
                {
                    errors.Append(string.Format("\n{0}({1},{2}): {3}: {4}",
                                filename, err.Line, err.Column,
                                err.ErrorNumber, err.ErrorText));
                }
                string str = "Error loading script\n" + errors.ToString();
                throw new System.ApplicationException(str);
            }
            else
            {
                GetPlugins(result.CompiledAssembly);
            }
        }

        public static CompilerResults LoadScript(string code)
        {
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("c#");
            CompilerParameters compilerParams = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false
            };

#if UNITY_EDITOR
            string dllPath = Application.dataPath + "/../Library/";
            //compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityAssemblies/UnityEngine.dll");
            compilerParams.ReferencedAssemblies.Add($"{dllPath}PlayerDataCache/Win64/Data/Managed/UnityEngine.CoreModule.dll");
            compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityAssemblies/UnityEngine.UI.dll");
            compilerParams.ReferencedAssemblies.Add($"{dllPath}ScriptAssemblies/Assembly-CSharp.dll");
#else
        string dllPath = Application.dataPath + "/Managed/";
        //compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityEngine.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityEngine.CoreModule.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}UnityEngine.UI.dll");
        compilerParams.ReferencedAssemblies.Add($"{dllPath}Assembly-CSharp.dll");
#endif
            var Result = codeDomProvider.CompileAssemblyFromSource(compilerParams, code);
            foreach (var Error in Result.Errors) Debug.LogError(Error);
            return Result;
        }

        public static object[] GetPlugins(System.Reflection.Assembly assembly, bool autoStart = true)
        {
            if (assembly == null) return null;
            List<object> objs = new List<object>();
            foreach (System.Type type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsNotPublic) continue;
                System.Type[] interfaces = type.GetInterfaces();
                if (((IList<System.Type>)interfaces).Contains(typeof(IScript)))
                {
                    var obj = System.Activator.CreateInstance(type);
                    objs.Add(obj);

                    IScript iScript = (IScript)obj;
                    if(autoStart) iScript.Start();

                    //Debug.Log(string.Format("{0} ({1})", iScript.Name, iScript.Description));
                }
            }
            return objs.ToArray();
        }
    }
}
