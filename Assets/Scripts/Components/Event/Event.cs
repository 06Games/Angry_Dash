using UnityEngine;
using AngryDash.Mod;
using System.Collections.Generic;

namespace AngryDash.Game.Event
{
    public class Event : MonoBehaviour
    {
        public string script;
        List<Interface> interfaces = new List<Interface>();

        void Start()
        {
            List<string> variables = new List<string>();
            foreach (System.Type type in Tools.TypeExtensions.GetTypesInNamespace("AngryDash.Game.Event.Action"))
            {
                if (GetComponent(type) == null) gameObject.AddComponent(type);
                variables.Add($"{type.Name} _{type.Name} {{ get {{ return _Event.GetRef<{type.Name}>(); }} }}");
            }

            string code =
                "using AngryDash.Game.Event;" +
                "\nusing AngryDash.Game.Event.Action;" +
                "\nusing UnityEngine;" +
                "\n" +
                "\npublic class Script : Interface" +
                "\n{" +
                "\npublic AngryDash.Game.Event.Event _Event { get; set; }" + 
                $"\n{string.Join("\n", variables)}\n" +
                "\npublic string Name { get { return \"Event\"; } }" +
                "\npublic string Description { get { return \"An event script\"; } }" +
                $"\n{script}" +
                "\n}";

            object[] mods = Modding.GetPlugins(Modding.LoadScript(code).CompiledAssembly, false);
#if UNITY_EDITOR
            if (mods == null) Debug.LogError($"The script is invalid:\n{code}");
#else
            if (mods == null) Debug.LogError($"The script is invalid:\n{script}");
#endif
            else foreach (object mod in mods) interfaces.Add((Interface)mod);

            foreach (Interface @interface in interfaces)
            {
                @interface._Event = this;
                @interface.Start();
            }
        }

        public T GetRef<T>() { return GetComponent<T>(); }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != Player.userPlayer) return;
            foreach (Interface @interface in interfaces) @interface.Collision();
        }
    }
}
