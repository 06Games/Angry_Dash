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

            interpreter.DoString(script);
            if (interpreter.Globals.Get("Start").IsNotNil()) interpreter.Call(interpreter.Globals["Start"]);

            Player.userPlayer.onRespawn += (s, e) => {
                if (interpreter.Globals.Get("Respawn").IsNotNil()) interpreter.Call(interpreter.Globals["Respawn"]);
            };
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != Player.userPlayer) return;
            if(interpreter.Globals.Get("Collision").IsNotNil()) interpreter.Call(interpreter.Globals["Collision"]);
        }
    }
}
