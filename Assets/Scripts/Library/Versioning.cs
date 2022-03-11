using System;
using System.Text.RegularExpressions;
using Tools;
using UnityEngine;

[Serializable]
public class Versioning
{
    //For serialization
    public string number = ""; //Store serialized version number
    public Versioning() { version = number.Split("."); } //Set the real var

    public static Versioning Actual => new Versioning(Application.version);

    public enum Sort { Newer, Equal, Older, Error }
    public enum SortConditions { Newer, NewerOrEqual, Equal, OlderOrEqual, Older }

    private string[] version;
    public Versioning(float _version) { number = _version.ToString(); version = _version.ToString().Split("."); }
    public Versioning(string _version)
    {
        number = _version;
        version = Regex.Replace(_version, "[^0-9.]", "").Split(".");
    }

    public override string ToString() { return string.Join(".", version); }
    public bool CompareTo(Versioning compared, SortConditions conditions)
    {
        var sort = CompareTo(compared);
        var lastest = conditions == SortConditions.Newer || conditions == SortConditions.NewerOrEqual;
        var equal = conditions == SortConditions.NewerOrEqual || conditions == SortConditions.Equal || conditions == SortConditions.OlderOrEqual;
        var oldest = conditions == SortConditions.OlderOrEqual || conditions == SortConditions.Older;

        if (lastest & sort == Sort.Newer) return true;
        if (equal & sort == Sort.Equal) return true;
        if (oldest & sort == Sort.Older) return true;
        return false;
    }
    public Sort CompareTo(Versioning compared)
    {
        for (var i = 0; (i < version.Length | i < compared.version.Length); i++)
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
