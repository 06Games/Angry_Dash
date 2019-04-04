namespace Level
{
    [System.Serializable]
    public class Infos : System.IEquatable<Infos>
    {
        public string name;
        public string description;
        public string author;
        public Background background;
        public SongItem music;
        public Versioning version;
        public int respawnMode;

        public Block[] blocks;

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this, !ConfigAPI.GetBool("editor.beautify")); }
        public static Infos Parse(string xml) { return FileFormat.XML.Utils.XMLtoClass<Infos>(xml); }

        public bool Equals(Infos other)
        {

            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (name == other.name
                & description == other.description
                & author == other.author
                & background == other.background
                & music == other.music
                & version == other.version
                & respawnMode == other.respawnMode
                & Block.Equals(blocks, other.blocks))
                return true;
            else return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Infos); }
        public static bool operator ==(Infos left, Infos right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Infos left, Infos right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public void CopyTo(out Infos other)
        {
            other = new Infos();
            other.name = name;
            other.description = description;
            other.author = author;
            other.background = background;
            other.music = music;
            other.version = version;
            other.respawnMode = respawnMode;
            other.blocks = new Block[blocks.Length];
            for (int i = 0; i < blocks.Length; i++) blocks[i].CopyTo(out other.blocks[i]);
        }
    }

    [System.Serializable]
    public class Background
    {
        public string category = "native";
        public int id;
        public UnityEngine.Color32 color;

        public bool Equals(Background other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (category == other.category
                & id == other.id
                & color.Equals(other.color))
                return true;
            else return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Background); }
        public static bool operator ==(Background left, Background right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Background left, Background right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    [System.Serializable]
    public class Block
    {
        public enum Type { Block, Event }
        public Type type;
        public string category = "native";
        public float id;
        public UnityEngine.Vector3 position;
        public Tools.Dictionary.Serializable<string, string> parameter = new Tools.Dictionary.Serializable<string, string>();

        public bool Equals(Block other)
        {
            if (other is null) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (type == other.type
                & category == other.category
                & id == other.id
                & position == other.position
                & parameter == other.parameter)
                return true;
            else return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Block); }
        public static bool Equals(Block[] left, Block[] right)
        {
            bool match = true;
            if (left is null & right is null) match = true;
            if (left is null | right is null) match = false;
            else if (left.Length != right.Length) match = false;
            else
            {
                for (int i = 0; i < left.Length & i < right.Length; i++)
                {
                    if (!(left[i] is null & right[i] is null))
                    {
                        if (left[i] is null | right[i] is null) match = false;
                        else if (!left[i].Equals(right[i])) match = false;
                    }
                }
            }
            return match;
        }
        public static bool operator ==(Block left, Block right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Block left, Block right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public void CopyTo(out Block other)
        {
            other = new Block();
            other.type = type;
            other.category = category;
            other.id = id;
            other.position = position;
            parameter.CopyTo(out other.parameter);
        }
    }

    [System.Serializable]
    public class LevelItem
    {
        public string Name = "";
        public string Author = "";
        public string Description = "";
        public string Music = "";
        public string Data = "";

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this); }
        public static LevelItem Parse(string data) { return FileFormat.XML.Utils.XMLtoClass<LevelItem>(data); }
    }

    [System.Serializable]
    public class SongItem
    {
        public string Name = "";
        public string Artist = "";
        public string Licence = "";
        public string URL = "";
        public SongItem() { }
        public SongItem(string name, string artist = "", string licence = "", string url = "")
        {
            Name = name;
            Artist = artist;
            Licence = licence;
            URL = url;
        }

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this); }
        public static SongItem Parse(string data) { return FileFormat.XML.Utils.XMLtoClass<SongItem>(data); }
    }
}
