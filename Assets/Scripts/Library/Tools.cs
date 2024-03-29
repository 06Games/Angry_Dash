﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

            return str;
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

            return str;
        }

        /// <summary>
        /// Parse a string to an other type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        public static T ParseTo<T>(this string str) { return (T)Convert.ChangeType(str, typeof(T)); }

        public static string[] Split(this string s, params string[] delimiter) { return s.Split(delimiter, StringSplitOptions.None); }
        public static string[] Split(this string s, StringSplitOptions options, params string[] delimiter) { return s.Split(delimiter, options); }

        public static bool Contains(this string s, params string[] values)
        {
            foreach (var value in values)
                if (s.Contains(value)) return true;
            return false;
        }

        public static string Trim(this string target, string trimString) { return target.TrimStart(trimString).TrimEnd(trimString); }
        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;
            if (string.IsNullOrEmpty(target)) return "";

            var result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
        public static string TrimEnd(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            var result = target;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

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

            return s;
        }

        public static string HtmlDecode(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var s2 = s;
                try
                {
                    var code = s.Split(new[] { "&" }, StringSplitOptions.None);
                    var newString = code[0];
                    if (code.Length > 1)
                    {
                        for (var i = 1; i < code.Length; i++)
                        {
                            var t = s.Split(new[] { ";" }, StringSplitOptions.None);
                            var c = Regex.Replace(s, "[^0-9]", "");
                            newString = newString + code[i].Replace(code[i], char.ConvertFromUtf32(int.Parse(c))) + t[2];
                        }
                        s = newString;
                    }

                    s2 = s;
                }
                catch { }

                return s2.Replace("\\'", "'")
                    .Replace("\\\"", "\"")
                    .Replace("!DIESE!", "#");
            }

            return s;
        }

        public static string RemoveSpecialCharacters(this string s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var ch in s.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>Returns the input string with the first character converted to uppercase</summary>
        public static string FirstLetterToUpperCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder SetPrefix(this System.Text.StringBuilder builder, string prefix) { return new StringBuilder(builder, prefix); }
    }

    public class StringBuilder
    {
        private readonly string _prefix;
        private readonly System.Text.StringBuilder _builder;

        public bool appendEmptyStrings { get; set; } = false;

        public StringBuilder() { _prefix = ""; _builder = new System.Text.StringBuilder(); }
        public StringBuilder(string prefix) { _prefix = prefix; _builder = new System.Text.StringBuilder(); }
        public StringBuilder(System.Text.StringBuilder builder, string prefix) { _prefix = prefix; _builder = builder; }

        public StringBuilder Append(string line) { _builder.Append(_prefix).Append(line); return this; }
        public StringBuilder AppendLine() { _builder.Append($"\n{_prefix}"); return this; }
        public StringBuilder AppendLine(string line)
        {
            if (!string.IsNullOrEmpty(line) | appendEmptyStrings) _builder.Append(_builder.Length > 0 ? "\n" : "").Append(_prefix).Append(line);
            return this;
        }
        public StringBuilder AppendLines(string lines) { return AppendLines(lines.Split("\n")); }
        public StringBuilder AppendLines(string[] lines) { foreach (var line in lines) AppendLine(line); return this; }

        public StringBuilder Merge(System.Text.StringBuilder stringBuilder) { return AppendLines(stringBuilder.ToString()); }
        public StringBuilder Merge(StringBuilder stringBuilder) { return AppendLines(stringBuilder.ToString()); }
        public override string ToString() { return _builder.ToString(); }

        public int LineCount => Regex.Matches(_builder.ToString(), "\n").Count;
        public int Length { get => _builder.Length;
            set => _builder.Length = value;
        }
        public string this[int line] => _builder.ToString().Split("\n")[line];
    }

    public static class TypeExtensions
    {
        public static Type[] GetTypesInNamespace(string nameSpace)
        {
            return
              Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
    }

    public static class Texture2DExtensions
    {
        public static Texture2D PremultiplyAlpha(this Texture2D texture)
        {
            var pixels = texture.GetPixels();
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Premultiply(pixels[i]);
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
            var dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        ///<summary> Delete array elements</summary>
        /// <param name="source">The array to edit</param>
        /// <param name="from">The index of the first element to delete</param>
        /// <param name="to">The index of the last element to delete</param>
        public static T[] RemoveAt<T>(this T[] source, int from, int to)
        {
            var dest = new T[source.Length - (to - from) - 1];
            if (from > 0)
                Array.Copy(source, 0, dest, 0, from);

            if (to < source.Length - 1)
                Array.Copy(source, to + 1, dest, from, source.Length - to - 1);

            return dest;
        }

        /// <summary> Extract a smaller array from an array </summary>
        /// <param name="list">The source array</param>
        /// <param name="from">The index of the first element to copy</param>
        /// <param name="to">The index of the last element to copy</param>
        public static T[] Get<T>(this T[] list, int from, int to)
        {
            if (from < 0) throw new Exception("From index can not be less than 0");
            if (to < from) throw new Exception("To index can not be less than From index");
            if (to >= list.Length) throw new Exception("To index can not be more than the list length");

            return list.Skip(from).Take(to - from).ToArray();
        }
    }

    public static class DateExtensions
    {
        /// <summary> Return a DateTime as a string </summary>
        /// <param name="DT"></param>
        public static string Format(this DateTime DT)
        {
            var a = "dd'/'MM'/'yyyy";
            return DT.ToString(a);
        }
    }

    public static class ScrollRectExtensions
    {
        /// <summary> Scroll to a child of the content </summary>
        /// <param name="scroll">The ScrollRect</param>
        /// <param name="target">The child to scroll to</param>
        /// <param name="myMonoBehaviour">Any MonoBehaviour script</param>
        public static void SnapTo(this ScrollRect scroll, Transform target, MonoBehaviour myMonoBehaviour) { myMonoBehaviour.StartCoroutine(Snap(scroll, target)); }

        private static IEnumerator Snap(ScrollRect scroll, Transform target)
        {
            float Frames = 30;
            var normalizePosition = 1 - (target.GetSiblingIndex() - 1F) / (scroll.content.childCount - 2F);
            var actualNormalizePosition = scroll.verticalNormalizedPosition;
            for (var i = 0; i < Frames; i++)
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
            var rect = sp.rect; return new Vector2(rect.width, rect.height);
        }

        public static Sprite Flip(this Sprite sp)
        {
            var original = sp.texture;
            var flipped = new Texture2D(original.width, original.height);

            var xN = original.width;
            var yN = original.height;


            for (var i = 0; i < xN; i++)
            {
                for (var j = 0; j < yN; j++)
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

        /// <summary>Find a parent of the transform/summary>
        /// <param name="n">the name of the parent</param>
        public static Transform FindParent(this Transform transform, string n)
        {
            var parent = transform.parent;
            while (parent != null)
            {
                if (parent.name == n) return parent;
                parent = parent.parent;
            }
            return null;
        }

        /// <summary>Get all childs of the transform</summary>
        public static List<Transform> GetChilds(this Transform transform)
        {
            var list = new List<Transform>();
            foreach (Transform go in transform) list.Add(go);
            return list;
        }
    }
    public static class RectTransformExtensions
    {
        public static bool IsOver(this RectTransform transform, Vector2 hoverObj)
        {
            Vector2 localPosition = transform.InverseTransformPoint(hoverObj);
            return transform.rect.Contains(localPosition);
        }
        public static bool IsOver(this RectTransform transform, RectTransform hoverTransform)
        {
            Rect container = transform.GetWorldRect();
            Rect other = hoverTransform.GetWorldRect();
            return container.Overlaps(other);
        }

        /// <summary>Converts RectTransform.rect's local coordinates to world space</summary>
        /// <returns>The world rect.</returns>
        /// <param name="rt">RectangleTransform we want to convert to world coordinates.</param>
        static public Rect GetWorldRect(this RectTransform rt) { return GetWorldRect(rt, Vector2.one); }
        /// <summary>Converts RectTransform.rect's local coordinates to world space</summary>
        /// <returns>The world rect.</returns>
        /// <param name="rt">RectangleTransform we want to convert to world coordinates.</param>
        /// <param name="scale">Optional scale pulled from the CanvasScaler. Default to using Vector2.one.</param>
        static public Rect GetWorldRect(this RectTransform rt, Vector2 scale)
        {
            // Convert the rectangle to world corners and grab the top left
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var topLeft = corners[0];

            // Rescale the size appropriately based on the current Canvas scale
            var scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);

            return new Rect(topLeft, scaledSize);
        }

        public static void SetRect(this RectTransform transform, Rect rect)
        {
            var flipX = transform.anchorMax.x == 1 & transform.anchorMin.x == 1;
            var flipY = transform.anchorMax.y == 1 & transform.anchorMin.y == 1;
            transform.anchoredPosition = (rect.position + (rect.size / 2F)) * new Vector2(flipX ? -1 : 1, flipY ? -1 : 1);
            transform.sizeDelta = rect.size;
        }

        public static void SetStretchSize(this RectTransform transform, Rect rect)
        {
            transform.anchorMin = new Vector2(0, 0);
            transform.anchorMax = new Vector2(1, 1);
            transform.offsetMin = rect.position;
            rect.size = transform.parent.GetComponent<RectTransform>().rect.size - rect.size - rect.position;
            transform.offsetMax = rect.size * -1;
        }
    }

    public static class Vector2Extensions
    {
        public static Vector2 Parse(string s)
        {
            if (TryParse(s, out var vector)) return vector;
            throw new FormatException("The string entered wasn't in a correct format ! : {s}");
        }
        public static bool TryParse(string s, out Vector2 vector)
        {
            var temp = s.Substring(1, s.Length - 2).Split(',');

            var success = true;
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
            var vector = new Vector3();
            if (TryParse(s, out vector)) return vector;
            throw new FormatException("The string entered wasn't in a correct format ! : {s}");
        }
        public static bool TryParse(string s, out Vector3 vector)
        {
            var temp = s.Substring(1, s.Length - 2).Split(',');

            var success = true;
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

    public static class RectExtensions
    {
        public static void Set(this Rect output, Rect input) { output.Set(input.x, input.y, input.width, input.height); }
        public static void Set(this Rect output, Vector2 offset, Vector2 size) { output.Set(offset.x, offset.y, size.x, size.y); }

        public static Rect Multiply(this Rect rect, float multiplier)
        {
            return new Rect(
                rect.x * multiplier,
                rect.y * multiplier,
                rect.width * multiplier,
                rect.height * multiplier
            );
        }
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
            var hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a;
            return hex;
        }

        /// <summary> Convert an Hex based color to RGBA </summary>
        /// <param name="hex">Hex color</param>
        public static Color ParseHex(string hex)
        {
            if (hex == null) return new Color32(190, 190, 190, 255);
            if (hex.Length < 6 | hex.Length > 9) return new Color32(190, 190, 190, 255);

            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            var a = byte.Parse(hex.Substring(6), NumberStyles.Number);
            return new Color32(r, g, b, a);
        }
    }

    public static class TExtensions
    {
        public static T DeepClone<T>(this T obj)
        {
            if (obj == null) return default;
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static bool In<T>(this T obj, params T[] args) { return args.Contains(obj); }
    }

    public static class EnumerableExtensions
    {
        /// <summary>Converts a IEnumerator to IEnumerable</summary>
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator) { while (enumerator.MoveNext()) yield return enumerator.Current; }
        /// <summary>Converts a IEnumerator to IEnumerable</summary>
        public static IEnumerable ToIEnumerable(this IEnumerator enumerator) { while (enumerator.MoveNext()) yield return enumerator.Current; }

        /// <summary>Performs the specified action on each element of the enumerable</summary>
        /// <param name="source">The enumerable to loop through</param>
        /// <param name="action">The Action delegate to perform on each element of the enumerable</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>Compare two strings in a natural way (ex. 1,2,3,..,10 and not 1,10,2,3,...)</summary>
        public static int CompareNatural(string strA, string strB)
        {
            return CompareNatural(strA, strB, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
        }

        /// <summary>Compare two strings in a natural way (ex. 1,2,3,..,10 and not 1,10,2,3,...)</summary>
        public static int CompareNatural(string strA, string strB, CultureInfo culture, CompareOptions options)
        {
            var cmp = culture.CompareInfo;
            var iA = 0;
            var iB = 0;
            var softResult = 0;
            var softResultWeight = 0;
            while (iA < strA.Length && iB < strB.Length)
            {
                var isDigitA = Char.IsDigit(strA[iA]);
                var isDigitB = Char.IsDigit(strB[iB]);
                if (isDigitA != isDigitB)
                {
                    return cmp.Compare(strA, iA, strB, iB, options);
                }

                if (!isDigitA && !isDigitB)
                {
                    var jA = iA + 1;
                    var jB = iB + 1;
                    while (jA < strA.Length && !Char.IsDigit(strA[jA])) jA++;
                    while (jB < strB.Length && !Char.IsDigit(strB[jB])) jB++;
                    var cmpResult = cmp.Compare(strA, iA, jA - iA, strB, iB, jB - iB, options);
                    if (cmpResult != 0)
                    {
                        // Certain strings may be considered different due to "soft" differences that are
                        // ignored if more significant differences follow, e.g. a hyphen only affects the
                        // comparison if no other differences follow
                        var sectionA = strA.Substring(iA, jA - iA);
                        var sectionB = strB.Substring(iB, jB - iB);
                        if (cmp.Compare(sectionA + "1", sectionB + "2", options) ==
                            cmp.Compare(sectionA + "2", sectionB + "1", options))
                        {
                            return cmp.Compare(strA, iA, strB, iB, options);
                        }

                        if (softResultWeight < 1)
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
                    var zeroA = (char)(strA[iA] - (int)Char.GetNumericValue(strA[iA]));
                    var zeroB = (char)(strB[iB] - (int)Char.GetNumericValue(strB[iB]));
                    var jA = iA;
                    var jB = iB;
                    while (jA < strA.Length && strA[jA] == zeroA) jA++;
                    while (jB < strB.Length && strB[jB] == zeroB) jB++;
                    var resultIfSameLength = 0;
                    do
                    {
                        isDigitA = jA < strA.Length && Char.IsDigit(strA[jA]);
                        isDigitB = jB < strB.Length && Char.IsDigit(strB[jB]);
                        var numA = isDigitA ? (int)Char.GetNumericValue(strA[jA]) : 0;
                        var numB = isDigitB ? (int)Char.GetNumericValue(strB[jB]) : 0;
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

                    if (resultIfSameLength != 0)
                    {
                        // Both numbers are the same length (ignoring leading zeros) and at least one of
                        // the digits differed - the first difference determines the result
                        return resultIfSameLength;
                    }
                    var lA = jA - iA;
                    var lB = jB - iB;
                    if (lA != lB)
                    {
                        // Both numbers are equivalent but one has more leading zeros
                        return lA > lB ? -1 : 1;
                    }

                    if (zeroA != zeroB && softResultWeight < 2)
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

            if (softResult != 0)
            {
                return softResult;
            }
            return 0;
        }

        public class Comparer<T> : IComparer<T>
        {
            private Comparison<T> _comparison;
            public Comparer(Comparison<T> comparison) { _comparison = comparison; }
            public int Compare(T x, T y) { return _comparison(x, y); }
        }
    }

    namespace Dictionary
    {
        [Serializable]
        public class Serializable<TKey, TValue> : IEquatable<Serializable<TKey, TValue>>
        {
            [XmlIgnore]
            public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            public Pair<TKey, TValue>[] pair
            {
                get
                {
                    var list = new List<Pair<TKey, TValue>>();
                    foreach (var keyValue in dictionary)
                        list.Add(new Pair<TKey, TValue> { Key = keyValue.Key, Value = keyValue.Value });
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
                get
                {
                    if (dictionary.ContainsKey(key)) return dictionary[key];
                    return default;
                }
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

                var match = true;
                var pSelf = pair;
                var pOther = other.pair;
                if (pSelf.Length != pOther.Length) match = false;
                else
                {
                    for (var i = 0; i < pSelf.Length & i < pOther.Length; i++)
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
                if (left is null | right is null) return false;
                return left.Equals(right);
            }
            public static bool operator !=(Serializable<TKey, TValue> left, Serializable<TKey, TValue> right) { return !(left == right); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public void CopyTo(out Serializable<TKey, TValue> other)
            {
                other = new Serializable<TKey, TValue>();
                foreach (var kv in dictionary) other.Add(kv.Key, kv.Value);
            }
        }

        [Serializable]
        public class Pair<TKey, TValue> : IEquatable<Pair<TKey, TValue>>
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
                if (left is null | right is null) return false;
                return left.Equals(right);
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
            var l_startPathParts = p_startPath.Split('/');
            var l_destinationPathParts = p_fullDestinationPath.Split('/');

            var l_sameCounter = 0;
            while ((l_sameCounter < l_startPathParts.Length) && (l_sameCounter < l_destinationPathParts.Length) && l_startPathParts[l_sameCounter].Equals(l_destinationPathParts[l_sameCounter], StringComparison.InvariantCultureIgnoreCase))
            {
                l_sameCounter++;
            }

            if (l_sameCounter == 0) return p_fullDestinationPath; // There is no relative link.

            var l_builder = new System.Text.StringBuilder();
            for (var i = l_sameCounter; i < l_startPathParts.Length; i++) l_builder.Append("../");

            for (var i = l_sameCounter; i < l_destinationPathParts.Length; i++) l_builder.Append(l_destinationPathParts[i] + "/");

            if (l_builder.Length > 0) l_builder.Length--;
            return l_builder.ToString();
        }
    }

    public static class InputExtensions
    {
        /// <summary>Detects if the device has a physical keyboard</summary>
        public static bool isHardwareKeyboardAvailable
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject currentResources = currentActivity.Call<AndroidJavaObject>("getResources");
                AndroidJavaObject currentConfiguration = currentResources.Call<AndroidJavaObject>("getConfiguration");
                int keyboard = currentConfiguration.Get<int>("keyboard");
                return keyboard != 1; //Keyboard = 1 means no physical keyboard. See https://developer.android.com/reference/android/content/res/Configuration.html#keyboard
#else
                return !TouchScreenKeyboard.isSupported; //There is no API to retrieve this information, so we consider that all devices with a touch screen use a virtual keyboard.
#endif
            }
        }
    }

    public static class SceneManagerExtensions
    {
        /// <summary>Searches through the Scenes loaded for scenes with the given name.</summary>
        /// <param name="name">Name of scenes to find.</param>
        /// <returns>A list of reference to the Scenes.</returns>
        public static IEnumerable<Scene> GetScenesByName(string name)
        {
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name == name) yield return scene;
            }
        }
    }
}
