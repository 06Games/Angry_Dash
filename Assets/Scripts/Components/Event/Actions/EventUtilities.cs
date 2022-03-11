using AngryDash.Image.Reader;
using MoonSharp.Interpreter;
using UnityEngine;

namespace AngryDash.Game.API
{
    public class EventUtilities
    {
        public static void ChangeTexture(DynValue gameObject, string id)
        {
            var go = (Transform)gameObject.UserData.Object;
            var reader = go.GetComponent<UImage_Reader>().SetID(id).LoadAsync();
            go.localScale = new Vector2(100, 100) / reader.FrameSize * 50;
        }
    }
}
