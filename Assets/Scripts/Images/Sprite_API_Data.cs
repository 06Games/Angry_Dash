using UnityEngine;
using System.Collections;

namespace AngryDash.Image
{
    /// <summary>
    /// Contains animation information
    /// </summary>
    [System.Serializable]
    public class Sprite_API_Data
    {
        /// <summary> Array of all the frames of the annimation (If the ressource is only a sprite, the sprite will be returned at the index 0) </summary>
        public Sprite[] Frames = new Sprite[0];
        /// <summary> Delay before each frame </summary>
        public float[] Delay = new float[0];
        /// <summary> Number of repetitions of the animation (0 being infinity) </summary>
        public uint Repeat = 0;

        public Sprite_API_Data()
        {
            Frames = new Sprite[0];
            Delay = new float[0];
            Repeat = 0;
        }
        public Sprite_API_Data(Sprite[] frames, float[] delay, uint repeat)
        {
            Frames = frames;
            Delay = delay;
            Repeat = repeat;
        }
        public override bool Equals(object obj) { return Equals(obj as Sprite_API_Data); }
        public bool Equals(Sprite_API_Data other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            return Frames == other.Frames & Delay == other.Delay & Repeat == other.Repeat;
        }
        public static bool operator ==(Sprite_API_Data left, Sprite_API_Data right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Sprite_API_Data left, Sprite_API_Data right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
