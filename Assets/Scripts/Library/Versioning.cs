using Tools;
using UnityEngine;

[System.Serializable]
public class Versioning
{
    //For serialization
    public string number = ""; //Store serialized version number
    public Versioning() { version = number.Split("."); } //Set the real var

    public static Versioning Actual { get { return new Versioning(Application.version); } }
    public enum Sort { Newer, Equal, Older, Error }
    public enum SortConditions { Newer, NewerOrEqual, Equal, OlderOrEqual, Older }

    string[] version;
    public Versioning(float _version) { number = _version.ToString(); version = _version.ToString().Split("."); }
    public Versioning(string _version)
    {
        number = _version;
        version = System.Text.RegularExpressions.Regex.Replace(_version, "[^0-9.]", "").Split(".");
    }

    public override string ToString() { return string.Join(".", version); }
    public bool CompareTo(Versioning compared, SortConditions conditions)
    {
        Sort sort = CompareTo(compared);
        bool lastest = conditions == SortConditions.Newer || conditions == SortConditions.NewerOrEqual;
        bool equal = conditions == SortConditions.NewerOrEqual || conditions == SortConditions.Equal || conditions == SortConditions.OlderOrEqual;
        bool oldest = conditions == SortConditions.OlderOrEqual || conditions == SortConditions.Older;

        if (lastest & sort == Sort.Newer) return true;
        else if (equal & sort == Sort.Equal) return true;
        else if (oldest & sort == Sort.Older) return true;
        else return false;
    }
    public Sort CompareTo(Versioning compared)
    {
        for (int i = 0; (i < version.Length | i < compared.version.Length); i++)
        {
            float versionNumber = 0;
            if (version.Length > i) float.TryParse(version[i], out versionNumber);
            float comparedVersion = 0;
            if (compared.version.Length > i) float.TryParse(compared.version[i], out comparedVersion);

            if (versionNumber > comparedVersion) return Sort.Newer;
            if (versionNumber == comparedVersion & (i >= version.Length - 1 & i >= compared.version.Length - 1)) return Sort.Equal;
            if (versionNumber < comparedVersion) return Sort.Older;
        }
        Debug.LogError("Can't compare versions !");
        return Sort.Error;
    }
}
