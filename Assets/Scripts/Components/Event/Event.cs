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
            string code =
                "using AngryDash.Game.Event;" + 
                "\nusing AngryDash.Game.Event.Action;" +
                "\nusing UnityEngine;" +
                "\n" +
                "\npublic class Script : Interface" +
                "\n{" +
                "\npublic string Name { get { return \"Event\"; } }" +
                "\npublic string Description { get { return \"An event script\"; } }" +
                $"\n{script}" +
                "\n}";

            object[] mods = Modding.GetPlugins(Modding.LoadScript(code).CompiledAssembly);
            if (mods == null) Debug.LogError($"The script is invalid:\n{script}");
            else foreach (object mod in mods) interfaces.Add((Interface)mod);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != Player.userPlayer) return;
            foreach (Interface @interface in interfaces) @interface.Collision();
        }
    }
}
