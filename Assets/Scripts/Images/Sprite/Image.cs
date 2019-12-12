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
        public Rect(uint[] offset, uint[] size) {
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
    }

    internal class Texture
    {
        List<int> pixels { get; set; } = new List<int>();
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
        void Draw(ColorType colorType)
        {
            var background = new int[4];
            if (colorType == ColorType.grayscale) background = new int[1];
            else if (colorType == ColorType.grayscaleA) background = new int[2];
            else if (colorType == ColorType.RGB) background = new int[3];
            else if (colorType == ColorType.RGBA) background = new int[4];
            else if (colorType == ColorType.palette) background = new int[3];
            colorBand = background.Length;
            this.colorType = colorType;

            for (int i = 0; i < width * height; i++) pixels.AddRange(background);
        }

        public List<int> GetPixels() { return pixels; }
        public List<int> GetPixels(Rect rect)
        {
            if (rect.x == 0 & rect.y == 0 & rect.width == width & rect.height == height) return pixels;
            else
            {
                var bounds = WorkOnPixels(rect.x, rect.y, rect.width, rect.height);
                return pixels.GetRange(bounds.Item1, bounds.Item2 - bounds.Item1);
            }
        }
        public void SetPixels(Rect rect, List<int> colors)
        {
            if (pixels.Count == colors.Count) pixels = colors;
            else
            {
                var bounds = WorkOnPixels(rect.x, rect.y, rect.width, rect.height);
                for (int i = bounds.Item1; i < bounds.Item2 & i < colors.Count - bounds.Item1; i++) pixels[i] = colors[i - bounds.Item1];
            }
        }
        (int, int) WorkOnPixels(float xOffset, float yOffset, float xSize, float ySize)
        {
            float minIndex = width * yOffset + xOffset;
            float maxIndex = width * (yOffset + ySize - 1) + (xOffset + xSize);
            return ((int)minIndex * colorBand, (int)maxIndex * colorBand);
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
            if (colorBand == 1) format = TextureFormat.Alpha8;
            else if (colorBand == 3) format = TextureFormat.RGB24;
            else if (colorBand == 4) format = TextureFormat.RGBA32;
            var texture = new Texture2D(size.x, size.y, format, false);

            try { texture.LoadRawTextureData(pixels.Select(i => (byte)i).ToArray()); }
            catch { throw new System.Exception("[" + colorType + " - " + format + "] Espected : " + texture.GetRawTextureData().Length + ", Got : " + pixels.Count); }
            FlipTextureVertically(texture);

            texture.Apply();
            return Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        }

        void FlipTextureVertically(Texture2D original)
        {
            var originalPixels = original.GetPixels();

            Color[] newPixels = new Color[originalPixels.Length];

            int width = original.width;
            int rows = original.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
                }
            }

            original.SetPixels(newPixels);
            original.Apply();
        }
    }
}
