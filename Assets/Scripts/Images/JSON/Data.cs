using System;
using UnityEngine;

namespace AngryDash.Image.JSON
{
    [Serializable]
    public class Data
    {
        [Header("Textures")]
        public Texture[] textures = new Texture[4];

        [Space(10)]
        [Header("Text Components")]
        [NonSerialized] public Color32 textColor = new Color32(255, 255, 255, 255);
        public FontStyle textStyle = FontStyle.Normal;
        public TextAnchor textAnchor = TextAnchor.MiddleLeft;
        public bool textResize;
        public int textSize; //Only if textResize = false or for compatibility with layout groups
        public int[] textResizeMinAndMax = new int[2];  //Only works if textResize = true
    }

    [Serializable]
    public class Texture
    {
        public string path;
        [NonSerialized] public Vector4 border;
        public enum Type { Basic, Hover, Pressed, Disabled }
        public Type type;
        public enum Display { Simple, Sliced, Tiled, Fit, Envelope }
        public Display display;
    }
}
