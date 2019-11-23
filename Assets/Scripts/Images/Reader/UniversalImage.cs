using UnityEngine;
using _Image = UnityEngine.UI.Image;

namespace AngryDash.Image.Reader
{
    internal class UniversalImage
    {
        public SpriteRenderer spriteR;
        public _Image image;
        public GameObject gameObject
        {
            get
            {
                if (spriteR != null) return spriteR.gameObject;
                else return image != null ? image.gameObject : null;
            }
        }
        bool Null { get { return spriteR == null | image == null; } }

        public UniversalImage(GameObject go)
        {
            spriteR = go.GetComponent<SpriteRenderer>();
            image = go.GetComponent<_Image>();
        }
        public UniversalImage(SpriteRenderer _sprite) { spriteR = _sprite; }
        public UniversalImage(_Image _image) { image = _image; }

        public bool enabled
        {
            get
            {
                if (spriteR != null) return spriteR.enabled;
                else return image != null ? image.enabled : false;
            }
            set
            {
                if (spriteR != null) spriteR.enabled = value;
                else if (image != null) image.enabled = value;
            }
        }

        public SpriteDrawMode type
        {
            get { return default; }
            set
            {
                if (spriteR != null) spriteR.drawMode = value;
                if (image != null) image.type = (_Image.Type)value;
            }
        }

        public Sprite sprite
        {
            get
            {
                if (spriteR != null) return spriteR.sprite;
                else return image != null ? image.sprite : null;
            }
            set
            {
                if (spriteR != null) spriteR.sprite = value;
                else if (image != null) image.sprite = value;
            }
        }
    }
}
