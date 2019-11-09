using MoonSharp.Interpreter;
using System.Reflection;
using UnityEngine;

namespace AngryDash.Game.Events
{
    public class Event : MonoBehaviour
    {
        public string script;
        Script interpreter;

        void Start()
        {
            if (string.IsNullOrEmpty(script)) return;

            interpreter = new Script();
            interpreter.Globals.Set("go", UserData.Create(transform, UserData.GetDescriptorForObject(transform)));

            string Namespace = "AngryDash.Game.API";
            foreach (System.Type type in Tools.TypeExtensions.GetTypesInNamespace(Namespace))
            {
                foreach (var methodInfo in type.GetMethods())
                {
                    string methodName = GetMethodNameRelativeTo(methodInfo, Namespace).Replace(".", "_"); //Replace the dots with underscores because the Lua interpreter does not understand the structure of the C#
                    interpreter.Globals[methodName] = CallbackFunction.FromMethodInfo(interpreter, methodInfo);
                }
            }
            //Returns the path to the method from the given namespace
            string GetMethodNameRelativeTo(MethodInfo methodInfo, string parentNamespace)
            {
                string _namespace = methodInfo.DeclaringType.FullName;
                if (_namespace.StartsWith(parentNamespace)) _namespace = _namespace.Remove(0, parentNamespace.Length + 1);
                return _namespace + "." + methodInfo.Name;
            }

            interpreter.Globals.Set("sleep", DynValue.NewCallback((ctx, args) =>
            {
                System.Threading.Thread.Sleep((int)(args[0].ToObject<float>() * 1000));
                return DynValue.NewNil();
            }));


            interpreter.DoString(script);

            Execute("Start");
            Player.userPlayer.onRespawn += () => Execute("Respawn");
        }

        public async void Execute(string voidName)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                if (interpreter.Globals.Get(voidName).IsNotNil()) interpreter.Call(interpreter.Globals[voidName]);
            });
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != Player.userPlayer) return;
            if (interpreter.Globals.Get("Collision").IsNotNil()) interpreter.Call(interpreter.Globals["Collision"]);
        }
    }
}
