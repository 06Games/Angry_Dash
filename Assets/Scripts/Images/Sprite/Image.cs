using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AngryDash.Image
{
    internal class Rect
    {
        public long x { get; set; }
        public long y { get; set; }
        public long width { get; set; }
        public long height { get; set; }

        public Rect() { }
        public Rect(uint[] offset, uint[] size)
        {
            x = offset[0];
            y = offset[1];
            width = size[0];
            height = size[1];
        }
        public Rect(uint xOffset, uint yOffset, uint xSize, uint ySize)
        {
            x = xOffset;
            y = yOffset;
            width = xSize;
            height = ySize;
        }

        public override string ToString() { return $"({x}, {y}, {width}, {height})"; }

        public static bool operator ==(Rect left, Rect right)
        {
            if (left is null & right is null) return true;
            if (left is null | right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(Rect left, Rect right) { return !(left == right); }
        public bool Equals(Rect other) { return x == other.x & y == other.y & width == other.width & height == other.height; }
        public override bool Equals(object obj) { return Equals(obj as Rect); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    internal class Texture
    {
        private List<int> pixels { get; set; } = new List<int>();
        public uint width { get; private set; }
        public uint height { get; private set; }
        public int colorBand { get; private set; }
        public ColorType colorType { get; private set; } = ColorType.RGB;
        public enum ColorType { grayscale = 0, grayscaleA = 4, RGB = 2, RGBA = 6, palette = 3 }

        public Texture(int x, int y, ColorType colorType)
        {
            width = (uint)x;
            height = (uint)y;
            Draw(colorType);
        }

        private void Draw(ColorType colorType)
        {
            var background = new int[4];
            if (colorType == ColorType.grayscale) background = new int[3];
            else if (colorType == ColorType.grayscaleA) background = new int[4];
            else if (colorType == ColorType.RGB) background = new int[3];
            else if (colorType == ColorType.RGBA) background = new int[4];
            else if (colorType == ColorType.palette) background = new int[3];
            colorBand = background.Length;
            this.colorType = colorType;

            for (var i = 0; i < width * height; i++) pixels.AddRange(background);
        }

        public List<int> GetPixels() { return pixels; }
        public List<int> GetPixels(Rect rect)
        {
            if (rect.x == 0 & rect.y == 0 & rect.width == width & rect.height == height) return pixels;
            var list = new List<int>();
            WorkOnPixels(rect, (index, count) => list.AddRange(pixels.GetRange(index, count)));
            return list;
        }
        public void SetPixels(Rect rect, List<int> colors)
        {
            if (pixels.Count == colors.Count) pixels = colors;
            else
            {
                var i2 = 0;
                WorkOnPixels(rect, (index, count) =>
                {
                    pixels.RemoveRange(index, count);
                    pixels.InsertRange(index, colors.GetRange(i2, count));
                    i2 += count;
                });
            }
        }

        private void WorkOnPixels(Rect rect, Action<int, int> pixel)
        {
            for (var y = rect.height - 1; y >= 0; y--) pixel((int)((height - 1 - rect.y - y) * width + rect.x) * colorBand, (int)rect.width * colorBand);
        }
        public void Clear()
        {
            pixels.Clear();
            Draw(colorType);
        }

        public Sprite ToSprite(Vector4 border)
        {
            var size = new Vector2Int((int)width, (int)height);
            var format = TextureFormat.RGB24;
            if (colorBand == 3) format = TextureFormat.RGB24;
            else if (colorBand == 4) format = TextureFormat.RGBA32;
            else Debug.LogError("Unkown format: " + colorType);
            var texture = new Texture2D(size.x, size.y, format, false);

            try { texture.LoadRawTextureData(pixels.Select(i => (byte)i).ToArray()); }
            catch { throw new Exception("[" + colorType + " - " + format + "] Espected : " + texture.GetRawTextureData().Length + ", Got : " + pixels.Count); }
            FlipTextureVertically(texture);

            texture.Apply();
            return Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        }

        private void FlipTextureVertically(Texture2D original)
        {
            var originalPixels = original.GetPixels();
            var newPixels = new Color[originalPixels.Length];
            for (var x = 0; x < original.width; x++)
                for (var y = 0; y < original.height; y++)
                    newPixels[x + y * original.width] = originalPixels[x + (original.height - y - 1) * original.width];
            original.SetPixels(newPixels);
        }
    }
}
