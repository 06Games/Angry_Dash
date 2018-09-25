using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Editor_MoveTrigger : MonoBehaviour
{

    public Editeur editor;
    int[] SB;
    int array;

    public string[] Blocks;
    public Vector2 Range;
    public float Speed;
    public int Type;

    private void Start()
    {
        if (string.IsNullOrEmpty(editor.file)) gameObject.SetActive(false);
    }

    void Update()
    {
        if (editor.SelectedBlock.Length == 0) { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }

        if (SB != editor.SelectedBlock)
        {
            if (float.Parse(editor.GetBlocStatus("ID", editor.SelectedBlock[0])) < 1)
            {
                SB = editor.SelectedBlock;
                Blocks = editor.GetBlocStatus("Blocks", SB[0]).Split(new string[] { "," }, System.StringSplitOptions.None);
                if (string.IsNullOrEmpty(Blocks[0]) | Blocks[0] == "Null") Blocks = new string[0];
                try { Range = getVector2(editor.GetBlocStatus("Range", SB[0])); } catch { }
                try { Speed = float.Parse(editor.GetBlocStatus("Speed", SB[0])); } catch { }
                try { Type = int.Parse(editor.GetBlocStatus("Type", SB[0])); } catch { }
                Actualise();
            }
            else { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }
        }


        if (GetComponent<CreatorManager>().array == 1)
        {
            editor.bloqueSelect = true;

            if (array != GetComponent<CreatorManager>().array)
            {
                for (int i = 0; i < Blocks.Length; i++)
                {
                    Transform obj = editor.transform.GetChild(1).Find("Objet n° " + Blocks[i]);
                    if (obj != null) obj.transform.GetChild(1).gameObject.SetActive(true);
                }
            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.y > Screen.height / 4)
                {
                    Vector2 pos = editor.GetClicPos();
                    int Selected = editor.GetBloc((int)pos.x, (int)pos.y);
                    if (Selected != -1) Blocks = Blocks.Union(new string[] { Selected.ToString() }).ToArray();

                    for (int i = 0; i < Blocks.Length; i++)
                    {
                        Transform obj = editor.transform.GetChild(1).Find("Objet n° " + Blocks[i]);
                        if (obj != null) obj.transform.GetChild(1).gameObject.SetActive(true);
                    }
                }
            }


#if UNITY_STANDALONE || UNITY_EDITOR
            if (Input.GetKey(KeyCode.Mouse1))
            {
#else
        SimpleGesture.OnLongTap(() => {
#endif
                if (Input.mousePosition.y > Screen.height / 4)
                {
                    Vector2 pos = editor.GetClicPos();
                    int Selected = editor.GetBloc((int)pos.x, (int)pos.y);
                    if (Selected != -1)
                    {
                        List<string> list = new List<string>(Blocks);
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (int.Parse(Blocks[i]) == Selected)
                            {
                                list.RemoveAt(i);
                                Transform obj = editor.transform.GetChild(1).Find("Objet n° " + Blocks[i]);
                                Blocks = list.ToArray();
                                if (obj != null) obj.transform.GetChild(1).gameObject.SetActive(false);
                            }
                        }
                        Blocks = list.ToArray();
                    }
                }
            }
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        });
#endif
        }
        else
        {
            if (array == 1)
            {
                for (int i = 0; i < editor.transform.GetChild(1).childCount; i++)
                {
                    Transform obj = editor.transform.GetChild(1).GetChild(i);
                    if (obj.childCount > 1) obj.transform.GetChild(1).gameObject.SetActive(false);
                }
            }

            editor.bloqueSelect = false;
        }

        array = GetComponent<CreatorManager>().array;



        string blocks = "Null";
        if(Blocks.Length > 0) blocks = Blocks[0];
        for (int i = 1; i < Blocks.Length; i++)
            blocks = blocks + "," + Blocks[i];

        editor.ChangBlocStatus("Blocks", blocks, SB);
        editor.ChangBlocStatus("Range", Range.ToString(), SB);
        editor.ChangBlocStatus("Speed", Speed.ToString(), SB);
        editor.ChangBlocStatus("Type", Type.ToString(), SB);
    }
    
    void Actualise()
    {
        transform.GetChild(2).GetChild(0).GetComponent<Slider>().value = Range.x * 10F;
        transform.GetChild(2).GetChild(1).GetComponent<Slider>().value = Range.y * 10F;
        transform.GetChild(2).GetChild(2).GetComponent<Slider>().value = Speed * 10F;
        transform.GetChild(3).GetChild(0).GetComponent<Dropdown>().value = Type;
    }


    public void RangeValueChanged(int ScrollID)
    {
        Slider slider = transform.GetChild(2).GetChild(ScrollID).GetComponent<Slider>();
        int max = (slider.value / 10F).ToString().Length;
        if (max > 5) max = 5;
        slider.transform.GetChild(3).GetComponent<InputField>().text = (slider.value/10F).ToString().Substring(0, max);

        if(ScrollID == 0) Range.x = slider.value / 10F;
        else if (ScrollID == 1) Range.y = slider.value / 10F;
        else if (ScrollID == 2) Speed = slider.value / 10F;
    }
    public void TextRangeValueChanged(int ScrollID)
    {
        Slider slider = transform.GetChild(2).GetChild(ScrollID).GetComponent<Slider>();
        InputField inputField = slider.transform.GetChild(3).GetComponent<InputField>();
        if (inputField.text.Length > 4 & !inputField.text.Contains(".")) inputField.text = "9999";
        try
        {
            if (float.Parse(inputField.text) <= 10)
                slider.value = float.Parse(inputField.text) * 10F;
            else slider.value = 100;
        }
        catch { }
    }

    public void TypeValueChanged(Dropdown dropdown)
    {
        Type = dropdown.value;
    }


    public Vector2 getVector2(string rString)
    {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = System.Convert.ToSingle(temp[0]);
        float y = System.Convert.ToSingle(temp[1]);
        Vector2 rValue = new Vector2(x, y);
        return rValue;
    }

}
