using System.Collections.Generic;
using System.Linq;
using AngryDash.Image.Reader;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class EditorEditGroup : MonoBehaviour
{
    public Editeur editor;
    private int[] SB;
    private HashSet<int>[] groupsPerBlock;

    private void Update()
    {
        if (editor.SelectedBlock == null) { transform.GetComponentInParent<Edit>().EnterToEdit(); return; }

        if (editor.SelectedBlock.Length == 0) { transform.GetComponentInParent<Edit>().EnterToEdit(); return; }

        if (SB != editor.SelectedBlock)
        {
            SB = editor.SelectedBlock;
            groupsPerBlock = new HashSet<int>[SB.Length];

            for (var i = 0; i < SB.Length; i++)
            {
                var blockGroups = new HashSet<int>();
                foreach (var group in editor.GetBlocStatus("Groups", SB[i]).Split(", "))
                {
                    if (int.TryParse(group, out var g)) blockGroups.Add(g);
                }
                groupsPerBlock[i] = blockGroups;
            }
            Actualise();
        }
    }

    private void Actualise()
    {
        var Groups = new Dictionary<int, bool>();
        Transform groups = transform.GetChild(2).GetComponent<ScrollRect>().content;
        for (var i = 1; i < groups.childCount; i++) Destroy(groups.GetChild(i).gameObject);

        for (var i = 0; i < groupsPerBlock.Length; i++)
        {
            var generalGroups = Groups.Keys.ToList();
            foreach (var diff in groupsPerBlock[i].Except(generalGroups)) Groups.Add(diff, i == 0);
            if (i > 0) foreach (var diff in generalGroups.Except(groupsPerBlock[i])) Groups[diff] = false;
        }
        foreach (var Group in Groups)
        {
            var go = Instantiate(groups.GetChild(0).gameObject, groups).transform;
            go.GetComponent<UImage_Reader>().SetID("native/GUI/editor/edit/blocks/groups/selected" + (Group.Value ? "Common" : "")).LoadAsync();
            go.GetComponent<Button>().onClick.AddListener(() => Remove(Group.Key));
            go.GetChild(0).GetComponent<Text>().text = Group.Key.ToString();
            go.gameObject.SetActive(true);
        }
    }

    private void Save()
    {
        for (var i = 0; i < SB.Length; i++) editor.ChangBlocStatus("Groups", string.Join(", ", groupsPerBlock[i]), new[] { SB[i] });
    }

    public void Add(InputField field)
    {
        foreach (var list in groupsPerBlock)
        {
            if (int.TryParse(field.text, out var group)) list.Add(group);
        }
        Save();
        Actualise();
    }

    public void Remove(int i)
    {
        foreach (var list in groupsPerBlock) list.Remove(i);
        Save();
        Actualise();
    }
}
