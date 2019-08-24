using System.Runtime.CompilerServices;
using UnityEngine;

namespace AngryDash.Game.API
{
    public class EventUtilities
    {
        public static void ChangeTexture(MoonSharp.Interpreter.DynValue gameObject, string id)
        {
            Transform go = (Transform)gameObject.UserData.Object;
            Image.Reader.UImage_Reader reader = go.GetComponent<Image.Reader.UImage_Reader>().SetID(id).Load();
            go.localScale = new Vector2(100, 100) / reader.FrameSize * 50;
        }
    }
}
