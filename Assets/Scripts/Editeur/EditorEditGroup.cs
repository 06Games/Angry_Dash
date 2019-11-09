using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class EditorEditGroup : MonoBehaviour
{
    public Editeur editor;
    int[] SB;
    HashSet<int>[] groupsPerBlock;

    void Update()
    {
        if (editor.SelectedBlock == null) { transform.GetComponentInParent<Edit>().EnterToEdit(); return; }
        else if (editor.SelectedBlock.Length == 0) { transform.GetComponentInParent<Edit>().EnterToEdit(); return; }

        if (SB != editor.SelectedBlock)
        {
            SB = editor.SelectedBlock;
            groupsPerBlock = new HashSet<int>[SB.Length];

            for (int i = 0; i < SB.Length; i++)
            {
                HashSet<int> blockGroups = new HashSet<int>();
                foreach (string group in editor.GetBlocStatus("Groups", SB[i]).Split(", "))
                {
                    if (int.TryParse(group, out int g)) blockGroups.Add(g);
                }
                groupsPerBlock[i] = blockGroups;
            }
            Actualise();
        }
    }

    void Actualise()
    {
        Dictionary<int, bool> Groups = new Dictionary<int, bool>();
        Transform groups = transform.GetChild(2).GetComponent<ScrollRect>().content;
        for (int i = 1; i < groups.childCount; i++) Destroy(groups.GetChild(i).gameObject);

        for (int i = 0; i < groupsPerBlock.Length; i++)
        {
            List<int> generalGroups = Groups.Keys.ToList();
            foreach (int diff in groupsPerBlock[i].Except(generalGroups)) Groups.Add(diff, i == 0);
            if (i > 0) foreach (int diff in generalGroups.Except(groupsPerBlock[i])) Groups[diff] = false;
        }
        foreach (var Group in Groups)
        {
            Transform go = Instantiate(groups.GetChild(0).gameObject, groups).transform;
            go.GetComponent<AngryDash.Image.Reader.UImage_Reader>().SetID("native/GUI/editor/edit/blocks/groups/selected" + (Group.Value ? "Common" : "")).Load();
            go.GetComponent<Button>().onClick.AddListener(() => Remove(Group.Key));
            go.GetChild(0).GetComponent<Text>().text = Group.Key.ToString();
            go.gameObject.SetActive(true);
        }
    }

    void Save()
    {
        for (int i = 0; i < SB.Length; i++) editor.ChangBlocStatus("Groups", string.Join(", ", groupsPerBlock[i]), new int[] { SB[i] });
    }

    public void Add(InputField field)
    {
        foreach (HashSet<int> list in groupsPerBlock)
        {
            if (int.TryParse(field.text, out int group)) list.Add(group);
        }
        Save();
        Actualise();
    }

    public void Remove(int i)
    {
        foreach (HashSet<int> list in groupsPerBlock) list.Remove(i);
        Save();
        Actualise();
    }
}
