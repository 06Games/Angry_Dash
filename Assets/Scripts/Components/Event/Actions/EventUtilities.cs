using System.Runtime.CompilerServices;
using UnityEngine;

namespace AngryDash.Game.Event.Action
{
    public class EventUtilities: MonoBehaviour
    {
        public void ChangeTexture(string id)
        {
            Image.Reader.UImage_Reader reader = GetComponent<Image.Reader.UImage_Reader>().SetID(id).Load();
            transform.localScale = new Vector2(100, 100) / reader.FrameSize * 50;
        }
    }
}
