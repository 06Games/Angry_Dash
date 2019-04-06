﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary> All function additions to Unity native classes </summary>
namespace Tools
{
    public static class StringExtensions
    {
        /// <summary>
        /// Format a string
        /// </summary>
        /// <param name="str">The string to format</param>
        /// <returns></returns>
        public static string Format(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str.Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t");
            }
            else return str;
        }

        /// <summary>
        /// Unformat a string
        /// </summary>
        /// <param name="str">The string to unformat</param>
        public static string Unformat(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str.Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
            }
            else return str;
        }

        /// <summary>
        /// Parse a string to an other type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        public static T ParseTo<T>(this string str) { return (T)System.Convert.ChangeType(str, typeof(T)); }

        public static byte[] ToByte(this string str) { return ToByte(str, Encoding.UTF8); }
        public static byte[] ToByte(this string str, Encoding encoding) { return encoding.GetBytes(str); }

        public static string HtmlEncode(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Replace("'", "\\\'")
                    .Replace("\"", "\\\"")
                    .Replace("#", "!DIESE!");
            }
            else return s;
        }

        public static string HtmlDecode(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Replace("\\'", "'")
                    .Replace("\\\"", "\"")
                    .Replace("!DIESE!", "#");
            }
            else return s;
        }
    }

    public static class Texture2DExtensions
    {
        public static Texture2D PremultiplyAlpha(this Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Premultiply(pixels[i]);
            texture.SetPixels(pixels);
            return texture;
        }

        private static Color32 Premultiply(Color color)
        {
            return new Color(color.r * color.a, color.g * color.a, color.b * color.a, color.a);
        }
    }

    public static class ArrayExtensions
    {
        /// <summary> Delete an array element </summary>
        /// <param name="source">The array to edit</param>
        /// <param name="index">The index of the element</param>
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                System.Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                System.Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        ///<summary> Delete array elements</summary>
        /// <param name="source">The array to edit</param>
        /// <param name="from">The index of the first element to delete</param>
        /// <param name="to">The index of the last element to delete</param>
        public static T[] RemoveAt<T>(this T[] source, int from, int to)
        {
            T[] dest = new T[source.Length - (to - from) - 1];
            if (from > 0)
                System.Array.Copy(source, 0, dest, 0, from);

            if (to < source.Length - 1)
                System.Array.Copy(source, to + 1, dest, from, source.Length - to - 1);

            return dest;
        }

        /// <summary> Extract a smaller array from an array </summary>
        /// <param name="list">The source array</param>
        /// <param name="from">The index of the first element to copy</param>
        /// <param name="to">The index of the last element to copy</param>
        public static T[] Get<T>(this T[] list, int from, int to)
        {
            if (from < 0) throw new System.Exception("From index can not be less than 0");
            else if (to < from) throw new System.Exception("To index can not be less than From index");
            else if (to >= list.Length) throw new System.Exception("To index can not be more than the list length");

            return list.Skip(from).Take(to - from).ToArray();
        }
    }

    public delegate void BetterEventHandler(object sender, BetterEventArgs e);
    public class BetterEventArgs : System.EventArgs
    {
        public object UserState { get; }
        public BetterEventArgs() { }
        public BetterEventArgs(object userToken) { UserState = userToken; }
    }

    public static class DateExtensions
    {
        /// <summary> Return a DateTime as a string </summary>
        /// <param name="DT"></param>
        public static string Format(this System.DateTime DT)
        {
            string a = "dd'/'MM'/'yyyy";
            return DT.ToString(a);
        }
    }

    public static class ScrollRectExtensions
    {
        /// <summary> Scroll to a child of the content </summary>
        /// <param name="scroll">The ScrollRect</param>
        /// <param name="target">The child to scroll to</param>
        /// <param name="myMonoBehaviour">Any MonoBehaviour script</param>
        public static void SnapTo(this UnityEngine.UI.ScrollRect scroll, Transform target, MonoBehaviour myMonoBehaviour) { myMonoBehaviour.StartCoroutine(Snap(scroll, target)); }
        static IEnumerator Snap(UnityEngine.UI.ScrollRect scroll, Transform target)
        {
            float Frames = 30;
            float normalizePosition = 1 - (target.GetSiblingIndex() - 1F) / (scroll.content.childCount - 2F);
            float actualNormalizePosition = scroll.verticalNormalizedPosition;
            for (int i = 0; i < Frames; i++)
            {
                scroll.verticalNormalizedPosition = scroll.verticalNormalizedPosition + ((normalizePosition - actualNormalizePosition) / Frames);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public static class SpriteExtensions
    {
        /// <summary> Get the sprite size </summary>
        public static Vector2 Size(this Sprite sp)
        {
            if (sp == null) return new Vector2();
            Rect rect = sp.rect; return new Vector2(rect.width, rect.height);
        }

        public static Sprite Flip(this Sprite sp)
        {
            Texture2D original = sp.texture;
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;


            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
            flipped.Apply();

            return Sprite.Create(flipped, new Rect(0, 0, flipped.width, flipped.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, sp.border);
        }
    }
    public static class TransformExtensions
    {
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }

        public static bool IsHover(this RectTransform transform, Vector2 hoverObj)
        {
            Vector2 localMousePosition = transform.InverseTransformPoint(hoverObj);
            return transform.rect.Contains(localMousePosition);
        }
    }

    public static class Vector2Extensions
    {
        public static Vector2 Parse(string s)
        {
            Vector2 vector = new Vector2();
            if (TryParse(s, out vector)) return vector;
            else throw new System.FormatException("The string entered wasn't in a correct format ! : {s}");
        }
        public static bool TryParse(string s, out Vector2 vector)
        {
            string[] temp = s.Substring(1, s.Length - 2).Split(',');

            bool success = true;
            if (!float.TryParse(temp[0], out vector.x)) success = false;
            if (!float.TryParse(temp[1], out vector.y)) success = false;
            if (!success) vector = new Vector2();
            return success;
        }

        public static Vector2 Round(this Vector2 v, float round)
        { return new Vector2((int)(v.x / round), (int)(v.y / round)) * round; }

        public static float Distance(Vector2 first, Vector2 second)
        { return Mathf.Sqrt(Mathf.Pow(first.x - second.x, 2) + Mathf.Pow(first.y - second.y, 2)); }

        public static Vector2 Center(Vector2 first, Vector2 second)
        { return new Vector2((first.x + second.x) / 2F, (first.y + second.y) / 2F); }
    }
    public static class Vector3Extensions
    {
        public static Vector3 Parse(string s)
        {
            Vector3 vector = new Vector3();
            if (TryParse(s, out vector)) return vector;
            else throw new System.FormatException("The string entered wasn't in a correct format ! : {s}");
        }
        public static bool TryParse(string s, out Vector3 vector)
        {
            string[] temp = s.Substring(1, s.Length - 2).Split(',');

            bool success = true;
            if (temp.Length != 3) { vector = new Vector3(); success = false; }
            else
            {
                if (!float.TryParse(temp[0], out vector.x)) success = false;
                if (!float.TryParse(temp[1], out vector.y)) success = false;
                if (!float.TryParse(temp[2], out vector.z)) success = false;
            }
            if (!success) vector = new Vector3();
            return success;
        }

        public static Vector3 Round(this Vector3 v, float round)
        { return new Vector3((int)(v.x / round), (int)(v.y / round), (int)(v.z / round)) * round; }
    }
    public static class QuaternionExtensions
    {
        public static Quaternion SetEuler(float x, float y, float z = 0) { return SetEuler(new Vector3(x, y, z)); }
        public static Quaternion SetEuler(Vector3 vector) { return new Quaternion { eulerAngles = vector }; }
    }

    public static class ColorExtensions
    {
        /// <summary> Convert an RGBA based color to Hex </summary>
        /// <param name="color">RGBA color</param>
        public static string ToHex(this Color color) { return ToHex((Color32)color); }

        /// <summary> Convert an RGBA based color to Hex </summary>
        /// <param name="color">RGBA color</param>
        public static string ToHex(this Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString();
            return hex;
        }

        /// <summary> Convert an Hex based color to RGBA </summary>
        /// <param name="hex">Hex color</param>
        public static Color ParseHex(string hex)
        {
            if (hex == null) return new Color32(190, 190, 190, 255);
            else if (hex.Length < 6 | hex.Length > 9) return new Color32(190, 190, 190, 255);

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(6), System.Globalization.NumberStyles.Number);
            return new Color32(r, g, b, a);
        }
    }

    public static class EnumerableExtensions
    {
        public static int CompareNatural(string strA, string strB)
        {
            return CompareNatural(strA, strB, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreCase);
        }

        public static int CompareNatural(string strA, string strB, System.Globalization.CultureInfo culture, System.Globalization.CompareOptions options)
        {
            System.Globalization.CompareInfo cmp = culture.CompareInfo;
            int iA = 0;
            int iB = 0;
            int softResult = 0;
            int softResultWeight = 0;
            while (iA < strA.Length && iB < strB.Length)
            {
                bool isDigitA = System.Char.IsDigit(strA[iA]);
                bool isDigitB = System.Char.IsDigit(strB[iB]);
                if (isDigitA != isDigitB)
                {
                    return cmp.Compare(strA, iA, strB, iB, options);
                }
                else if (!isDigitA && !isDigitB)
                {
                    int jA = iA + 1;
                    int jB = iB + 1;
                    while (jA < strA.Length && !System.Char.IsDigit(strA[jA])) jA++;
                    while (jB < strB.Length && !System.Char.IsDigit(strB[jB])) jB++;
                    int cmpResult = cmp.Compare(strA, iA, jA - iA, strB, iB, jB - iB, options);
                    if (cmpResult != 0)
                    {
                        // Certain strings may be considered different due to "soft" differences that are
                        // ignored if more significant differences follow, e.g. a hyphen only affects the
                        // comparison if no other differences follow
                        string sectionA = strA.Substring(iA, jA - iA);
                        string sectionB = strB.Substring(iB, jB - iB);
                        if (cmp.Compare(sectionA + "1", sectionB + "2", options) ==
                            cmp.Compare(sectionA + "2", sectionB + "1", options))
                        {
                            return cmp.Compare(strA, iA, strB, iB, options);
                        }
                        else if (softResultWeight < 1)
                        {
                            softResult = cmpResult;
                            softResultWeight = 1;
                        }
                    }
                    iA = jA;
                    iB = jB;
                }
                else
                {
                    char zeroA = (char)(strA[iA] - (int)System.Char.GetNumericValue(strA[iA]));
                    char zeroB = (char)(strB[iB] - (int)System.Char.GetNumericValue(strB[iB]));
                    int jA = iA;
                    int jB = iB;
                    while (jA < strA.Length && strA[jA] == zeroA) jA++;
                    while (jB < strB.Length && strB[jB] == zeroB) jB++;
                    int resultIfSameLength = 0;
                    do
                    {
                        isDigitA = jA < strA.Length && System.Char.IsDigit(strA[jA]);
                        isDigitB = jB < strB.Length && System.Char.IsDigit(strB[jB]);
                        int numA = isDigitA ? (int)System.Char.GetNumericValue(strA[jA]) : 0;
                        int numB = isDigitB ? (int)System.Char.GetNumericValue(strB[jB]) : 0;
                        if (isDigitA && (char)(strA[jA] - numA) != zeroA) isDigitA = false;
                        if (isDigitB && (char)(strB[jB] - numB) != zeroB) isDigitB = false;
                        if (isDigitA && isDigitB)
                        {
                            if (numA != numB && resultIfSameLength == 0)
                            {
                                resultIfSameLength = numA < numB ? -1 : 1;
                            }
                            jA++;
                            jB++;
                        }
                    }
                    while (isDigitA && isDigitB);
                    if (isDigitA != isDigitB)
                    {
                        // One number has more digits than the other (ignoring leading zeros) - the longer
                        // number must be larger
                        return isDigitA ? 1 : -1;
                    }
                    else if (resultIfSameLength != 0)
                    {
                        // Both numbers are the same length (ignoring leading zeros) and at least one of
                        // the digits differed - the first difference determines the result
                        return resultIfSameLength;
                    }
                    int lA = jA - iA;
                    int lB = jB - iB;
                    if (lA != lB)
                    {
                        // Both numbers are equivalent but one has more leading zeros
                        return lA > lB ? -1 : 1;
                    }
                    else if (zeroA != zeroB && softResultWeight < 2)
                    {
                        softResult = cmp.Compare(strA, iA, 1, strB, iB, 1, options);
                        softResultWeight = 2;
                    }
                    iA = jA;
                    iB = jB;
                }
            }
            if (iA < strA.Length || iB < strB.Length)
            {
                return iA < strA.Length ? 1 : -1;
            }
            else if (softResult != 0)
            {
                return softResult;
            }
            return 0;
        }

        public class Comparer<T> : IComparer<T>
        {
            private System.Comparison<T> _comparison;
            public Comparer(System.Comparison<T> comparison) { _comparison = comparison; }
            public int Compare(T x, T y) { return _comparison(x, y); }
        }
    }

    namespace Dictionary
    {
        [System.Serializable]
        public class Serializable<TKey, TValue> : System.IEquatable<Serializable<TKey, TValue>>
        {
            [System.Xml.Serialization.XmlIgnore]
            public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            public Pair<TKey, TValue>[] pair
            {
                get
                {
                    List<Pair<TKey, TValue>> list = new List<Pair<TKey, TValue>>();
                    foreach (KeyValuePair<TKey, TValue> keyValue in dictionary)
                        list.Add(new Pair<TKey, TValue>() { Key = keyValue.Key, Value = keyValue.Value });
                    return list.ToArray();
                }
                set
                {
                    dictionary = new Dictionary<TKey, TValue>();
                    foreach (var item in value) { if (item.Key != null & item.Value != null) dictionary.Add(item.Key, item.Value); }
                }
            }

            public TValue this[TKey key]
            {
                get { if (dictionary.ContainsKey(key)) return dictionary[key]; else return default; }
                set
                {
                    if (dictionary.ContainsKey(key)) dictionary[key] = value;
                    else dictionary.Add(key, value);
                }
            }
            public bool TryGetValue(TKey key, out TValue value) { return dictionary.TryGetValue(key, out value); }
            public bool ContainsKey(TKey key) { return dictionary.ContainsKey(key); }
            public bool ContainsValue(TValue value) { return dictionary.ContainsValue(value); }
            public int Count => dictionary.Count;
            public void Add(TKey key, TValue value) { dictionary.Add(key, value); }
            public bool Remove(TKey key) { return dictionary.Remove(key); }
            public void Clear() { dictionary.Clear(); }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return dictionary.GetEnumerator(); }

            public bool Equals(Serializable<TKey, TValue> other)
            {
                if (other is null) return false; //If parameter is null, return false.
                if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
                if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

                bool match = true;
                Pair<TKey, TValue>[] pSelf = pair;
                Pair<TKey, TValue>[] pOther = other.pair;
                if (pSelf.Length != pOther.Length) match = false;
                else
                {
                    for (int i = 0; i < pSelf.Length & i < pOther.Length; i++)
                    {
                        if (!pSelf[i].Equals(pOther[i])) match = false;
                    }
                }
                return match;
            }
            public override bool Equals(object obj) { return Equals(obj as Serializable<TKey, TValue>); }
            public static bool operator ==(Serializable<TKey, TValue> left, Serializable<TKey, TValue> right)
            {
                if (left is null & right is null) return true;
                else if (left is null | right is null) return false;
                else return left.Equals(right);
            }
            public static bool operator !=(Serializable<TKey, TValue> left, Serializable<TKey, TValue> right) { return !(left == right); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public void CopyTo(out Serializable<TKey, TValue> other)
            {
                other = new Serializable<TKey, TValue>();
                foreach (KeyValuePair<TKey, TValue> kv in dictionary) other.Add(kv.Key, kv.Value);
            }
        }

        [System.Serializable]
        public class Pair<TKey, TValue> : System.IEquatable<Pair<TKey, TValue>>
        {
            public TKey Key;
            public TValue Value;

            public bool Equals(Pair<TKey, TValue> other)
            {
                if (other is null) return false; //If parameter is null, return false.
                if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
                if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.
                if (Key == null | Value == null) return (Key == null & other.Key == null) & (Value == null & other.Value == null);

                return Key.ToString() == other.Key.ToString() & Value.ToString() == other.Value.ToString();
            }
            public override bool Equals(object obj) { return Equals(obj as Pair<TKey, TValue>); }
            public static bool operator ==(Pair<TKey, TValue> left, Pair<TKey, TValue> right)
            {
                if (left is null & right is null) return true;
                else if (left is null | right is null) return false;
                else return left.Equals(right);
            }
            public static bool operator !=(Pair<TKey, TValue> left, Pair<TKey, TValue> right) { return !(left == right); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
    }

    public static class PathExtensions
    {
        public static string GetRelativePath(string p_fullDestinationPath, string p_startPath)
        {
            p_fullDestinationPath = p_fullDestinationPath.Replace("\\", "/").Trim('/');
            p_startPath = p_startPath.Replace("\\", "/").Trim('/');

            if (p_fullDestinationPath.StartsWith(p_startPath)) return p_fullDestinationPath.Remove(0, p_startPath.Length);
            else
            {
                string[] l_startPathParts = p_startPath.Split('/');
                string[] l_destinationPathParts = p_fullDestinationPath.Split('/');

                int l_sameCounter = 0;
                while ((l_sameCounter < l_startPathParts.Length) && (l_sameCounter < l_destinationPathParts.Length) && l_startPathParts[l_sameCounter].Equals(l_destinationPathParts[l_sameCounter], System.StringComparison.InvariantCultureIgnoreCase))
                {
                    l_sameCounter++;
                }

                if (l_sameCounter == 0) return p_fullDestinationPath; // There is no relative link.

                StringBuilder l_builder = new StringBuilder();
                for (int i = l_sameCounter; i < l_startPathParts.Length; i++) l_builder.Append("../");

                for (int i = l_sameCounter; i < l_destinationPathParts.Length; i++) l_builder.Append(l_destinationPathParts[i] + "/");

                if (l_builder.Length > 0) l_builder.Length--;
                return l_builder.ToString();
            }
        }
    }
}