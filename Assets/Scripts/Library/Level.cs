using System;
using System.IO;
using System.Linq;
using FileFormat.XML;
using Tools;
using Tools.Dictionary;
using UnityEngine;

namespace Level
{
    [Serializable]
    public class Infos : IEquatable<Infos>
    {
        public string name;
        public string description;
        public string author;
        public string rpURL;
        public bool rpRequired;
        public Background background;
        public SongItem music;
        public Versioning version;
        public Player player;
        public VictoryConditions victoryConditions;

        public Block[] blocks;

        public override string ToString() { return Utils.ClassToXML(this, !ConfigAPI.GetBool("editor.beautify")); }
        public static Infos Parse(string xml) { return Utils.XMLtoClass<Infos>(xml); }

        public bool Equals(Infos other)
        {

            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (name == other.name
                & description == other.description
                & author == other.author
                & rpURL == other.rpURL
                & rpRequired == other.rpRequired
                & background == other.background
                & music == other.music
                & version == other.version
                & player == other.player
                & victoryConditions == other.victoryConditions
                & Block.Equals(blocks, other.blocks))
                return true;
            return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Infos); }
        public static bool operator ==(Infos left, Infos right)
        {
            if (left is null & right is null) return true;
            if (left is null | right is null) return false;
            return left.Equals(right);
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
            other.player = player;
            other.victoryConditions = victoryConditions;
            other.blocks = new Block[blocks.Length];
            for (var i = 0; i < blocks.Length; i++) blocks[i].CopyTo(out other.blocks[i]);
        }
    }

    [Serializable]
    public class Background
    {
        public string category = "native";
        public int id;
        public Color32 color;

        public bool Equals(Background other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (category == other.category
                & id == other.id
                & color.Equals(other.color))
                return true;
            return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Background); }
        public static bool operator ==(Background left, Background right)
        {
            if (left is null & right is null) return true;
            if (left is null | right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(Background left, Background right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    [Serializable]
    public class VictoryConditions
    {
        public int maxThrow; //Maximum number of throws

        public override bool Equals(object obj) { return Equals(obj as VictoryConditions); }
        public bool Equals(VictoryConditions other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            return maxThrow == other.maxThrow;
        }
        public static bool operator ==(VictoryConditions left, VictoryConditions right)
        {
            if (left is null & right is null) return true;
            if (left is null | right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(VictoryConditions left, VictoryConditions right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    [Serializable]
    public class Player
    {
        public int respawnMode; //Action to be taken in case of death
        public float distance = 5; //Maximum distance traveled by the player in a throw

        public override bool Equals(object obj) { return Equals(obj as Player); }
        public bool Equals(Player other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            return respawnMode == other.respawnMode & distance == other.distance;
        }
        public static bool operator ==(Player left, Player right)
        {
            if (left is null & right is null) return true;
            if (left is null | right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(Player left, Player right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    [Serializable]
    public class Block
    {
        public enum Type { Block, Event }
        public Type type;
        public string category = "native";
        public float id;
        public Vector3 position;
        public Serializable<string, string> parameter = new Serializable<string, string>();

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
            return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Block); }
        public static bool Equals(Block[] left, Block[] right)
        {
            var match = true;
            if (left is null & right is null) match = true;
            if (left is null | right is null) match = false;
            else if (left.Length != right.Length) match = false;
            else
            {
                for (var i = 0; i < left.Length & i < right.Length; i++)
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
            if (left is null | right is null) return false;
            return left.Equals(right);
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

    [Serializable]
    public class LevelItem
    {
        public string Name = "";
        public string Author = "";
        public string Description = "";
        public string Music = "";
        public string Data = "";

        public override string ToString() { return Utils.ClassToXML(this); }
        public static LevelItem Parse(string data) { return Utils.XMLtoClass<LevelItem>(data); }
        public static LevelItem Parse(Infos data)
        {
            if (data == null) return new LevelItem();
            return new LevelItem
            {
                Name = data.name,
                Author = data.author,
                Description = data.description,
                Music = data.music != null ? data.music.Artist + " - " + data.music.Name : "",
                Data = data.ToString()
            };
        }
    }

    [Serializable]
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

        public override string ToString() { return Utils.ClassToXML(this); }
        public static SongItem Parse(string data) { return Utils.XMLtoClass<SongItem>(data); }

        public override bool Equals(object obj) { return Equals(obj as SongItem); }
        public bool Equals(SongItem other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            return Name == other.Name & Artist == other.Artist;
        }
        public static bool operator ==(SongItem left, SongItem right)
        {
            if (left is null & right is null) return true;
            if (left is null | right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(SongItem left, SongItem right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    public static class LevelUpdater
    {
        #region UpdateLevel
        /// <summary>
        /// Update a level to the newest version
        /// Warning : The level will be incompatible with older versions
        /// </summary>
        /// <param name="path">Path to the file</param>
        public static void UpdateLevel(FileInfo path)
        {
            var fileLines = File.ReadAllText(path.FullName);
            File.WriteAllText(path.FullName, UpdateLevel(fileLines, path.FullName));
        }

        /// <summary>
        /// Update a level to the newest version
        /// Warning : The level will be incompatible with older versions
        /// </summary>
        /// <param name="fileLines">File content</param>
        public static string UpdateLevel(string fileLines, string path = "")
        {
            var updatedFile = fileLines;
            if (!Utils.IsValid(fileLines)) //0.2 - 0.2.2
            {
                var newFileLines = fileLines.Split(new[] { "\n" }, StringSplitOptions.None);
                var v = -1;
                for (var x = 0; x < newFileLines.Length; x++)
                {
                    if (newFileLines[x].Contains("version = ") & v == -1)
                    {
                        v = x;
                        x = newFileLines.Length;
                    }
                }
                var version = new Versioning(0.2F);
                if (v != -1) version = new Versioning(newFileLines[v].Replace("version = ", ""));
                else
                {
                    v = 0;
                    newFileLines = new[] { "version = 0.2" }.Union(newFileLines).ToArray();
                }

                //Upgrade to 0.2.1
                if (version.CompareTo(new Versioning("0.2"), Versioning.SortConditions.OlderOrEqual))
                {
                    var d = -1;
                    for (var x = 0; x < newFileLines.Length; x++)
                    {
                        if (newFileLines[x].Contains("Blocks {") & d == -1)
                        {
                            d = x + 1;
                            x = newFileLines.Length;
                        }
                    }
                    if (d != -1)
                    {
                        for (var i = d; i < newFileLines.Length; i++)
                        {
                            if (newFileLines[i] == "}") i = newFileLines.Length;
                            else
                            {
                                var parm = newFileLines[i].Split(new[] { "; " }, StringSplitOptions.None);
                                try
                                {
                                    if (float.Parse(parm[0]) >= 1) newFileLines[i] = parm[0] + "; " + parm[1] + "; {Rotate:" + parm[2] + "; Color:" + parm[3] + "; Behavior:" + parm[4] + "}";
                                    else newFileLines[i] = parm[0] + "; " + parm[1] + "; {}";
                                }
                                catch { }
                            }
                        }
                    }

                    //Set new version number
                    newFileLines[v] = "version = 0.2.1";
                    version = new Versioning("0.2.1");
                }
                if (version.CompareTo(new Versioning("0.3"), Versioning.SortConditions.Older)) //Upgrade from 0.2.1 - 0.2.2 to 0.3
                {
                    var Name = "";
                    if (!string.IsNullOrEmpty(path))
                    {
                        Name = Path.GetFileNameWithoutExtension(path);

                        //Drastic changes to the format, backup the level in case...
                        var backupFolder = Application.persistentDataPath + "/Levels/Edited Levels/Backups/";
                        if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);
                        File.WriteAllText(backupFolder + Name + ".level", fileLines);
                    }
                    var updated = new Infos();
                    updated.name = Name;
                    updated.player = new Player();
                    updated.blocks = new Block[0];

                    var blockArea = false;
                    foreach (var line in newFileLines)
                    {
                        if (line.Contains("description = ")) updated.description = line.Replace("description = ", "").Replace("\r", "");
                        else if (line.Contains("background = "))
                        {
                            var data = line.Replace("background = ", "").Replace("\r", "").Split(new[] { "; " }, StringSplitOptions.None);
                            if (data.Length >= 2)
                            {
                                var ID = 1; int.TryParse(data[0], out ID);
                                updated.background = new Background
                                {
                                    category = "native",
                                    id = ID,
                                    color = ColorExtensions.ParseHex(data[1])
                                };
                            }
                        }
                        else if (line.Contains("music = "))
                        {
                            var data = line.Replace("music = ", "").Replace("\r", "").Split(new[] { " - " }, StringSplitOptions.None);
                            if (data.Length >= 2) updated.music = new SongItem { Artist = data[0], Name = data[1] };
                        }
                        else if (line.Contains("author = ")) updated.author = line.Replace("author = ", "").Replace("\r", "");
                        else if (line.Contains("respawnMode = ")) int.TryParse(line.Replace("respawnMode = ", "").Replace("\r", ""), out updated.player.respawnMode);
                        else if (line.Contains("Blocks {")) blockArea = true;
                        else if (blockArea)
                        {
                            if (line == "}") blockArea = false;
                            else if (!string.IsNullOrEmpty(line))
                            {
                                var data = line.Replace("\r", "").Split(new[] { "; " }, StringSplitOptions.None);
                                if (data.Length >= 2)
                                {
                                    float ID = 1; float.TryParse(data[0], out ID);
                                    var type = Block.Type.Block;
                                    if (ID < 1) type = Block.Type.Event;

                                    var parameters = new Serializable<string, string>();
                                    try
                                    {
                                        var pData = line.Split(new[] { "; {" }, StringSplitOptions.None)[1].Split(new[] { "}" }, StringSplitOptions.None)[0].Split(new[] { "; " }, StringSplitOptions.None);
                                        foreach (var p in pData)
                                        {
                                            var param = p.Split(new[] { ":" }, StringSplitOptions.None);
                                            if (param.Length == 2)
                                            {
                                                if (param[0] == "Blocks")
                                                {
                                                    var pID = param[1].Split(',');
                                                    for (var i = 0; i < pID.Length; i++)
                                                    {
                                                        int.TryParse(pID[i], out var id);
                                                        pID[i] = (id - 8).ToString();
                                                    }
                                                    param[1] = string.Join(",", pID);
                                                }
                                                parameters.Add(param[0], param[1]);
                                            }
                                        }
                                    }
                                    catch { }

                                    var array = new[]{
                                        new Block
                                        {
                                            type = type,
                                            category = "native",
                                            id = ID,
                                            position = Vector3Extensions.Parse(data[1]),
                                            parameter = parameters
                                        }
                                    };
                                    updated.blocks = updated.blocks.Concat(array).ToArray();
                                }
                            }
                        }
                    }
                    updated.version = new Versioning("0.3");
                    updatedFile = updated.ToString();
                }
                else updatedFile = string.Join("\n", newFileLines);
            }

            { //XML based versions (0.3 - now)
                var updated = Infos.Parse(updatedFile);
                if (updated.version.CompareTo(new Versioning("0.4"), Versioning.SortConditions.Older)) //0.3 to 0.4
                {
                    var moves = updated.blocks.Where(b => b.id == 0.4F).ToList();
                    for (var i = 0; i < moves.Count; i++)
                    {
                        var blocks = moves[i].parameter["Blocks"];
                        if (!string.IsNullOrEmpty(blocks))
                        {
                            foreach (var block in blocks.Split(","))
                            {
                                if (int.TryParse(block, out var index))
                                {
                                    var Group = updated.blocks[index].parameter["Groups"];
                                    Group = Group + (string.IsNullOrEmpty(Group) ? "" : ", ") + i;
                                    updated.blocks[index].parameter["Groups"] = Group;
                                }
                            }
                            moves[i].parameter["Group"] = i.ToString();
                            moves[i].parameter.Remove("Blocks");
                        }
                    }
                }
                updatedFile = updated.ToString();
            }

            return updatedFile;
        }
        #endregion
    }
}
