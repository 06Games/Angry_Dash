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
    int affected;

    public int AffectationType = 0;
    public string[] Blocks;
    public Vector2 Translation;
    public bool[] TranslationFromPlayer = new bool[2];
    public int Type;
    public float Speed;
    public bool MultiUsage;
    public Vector3 Rotation;
    public bool[] Reset = new bool[2];

    private void Start()
    {
        if (string.IsNullOrEmpty(editor.file)) gameObject.SetActive(false);
    }

    void Update()
    {
        if (editor.SelectedBlock.Length == 0) { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }

        if (SB != editor.SelectedBlock)
        {
            if (float.Parse(editor.GetBlocStatus("ID", editor.SelectedBlock[0])) == 0.4F)
            {
                SB = editor.SelectedBlock;

                try { AffectationType = int.Parse(editor.GetBlocStatus("AffectationType", SB[0])); } catch { }
                Blocks = editor.GetBlocStatus("Blocks", SB[0]).Split(new string[] { "," }, System.StringSplitOptions.None);
                if (string.IsNullOrEmpty(Blocks[0]) | Blocks[0] == "Null") Blocks = new string[0];

                try { Translation = getVector2(editor.GetBlocStatus("Translation", SB[0])); } catch { }
                string translationFrom = editor.GetBlocStatus("TranslationFrom", SB[0]);
                try
                {
                    string[] translationFromArray = translationFrom.Substring(1, translationFrom.Length - 2).Split(',');
                    for (int i = 0; i < translationFromArray.Length; i++)
                        TranslationFromPlayer[i] = bool.Parse(translationFromArray[i]);
                }
                catch { }
                string reset = editor.GetBlocStatus("Reset", SB[0]);
                try
                {
                    string[] resetArray = reset.Substring(1, reset.Length - 2).Split(',');
                    for (int i = 0; i < resetArray.Length; i++)
                        Reset[i] = bool.Parse(resetArray[i]);
                }
                catch { }

                try { Type = int.Parse(editor.GetBlocStatus("Type", SB[0])); } catch { }
                try { Speed = float.Parse(editor.GetBlocStatus("Speed", SB[0])); } catch { }
                try { MultiUsage = bool.Parse(editor.GetBlocStatus("MultiUsage", SB[0])); } catch { }

                try { Rotation = getVector3(editor.GetBlocStatus("Rotation", SB[0])); } catch { }

                Actualise();
            }
            else { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }
        }

        #region BlockSelection
        if (GetComponent<CreatorManager>().array == 1 & AffectationType == 0)
        {
            editor.bloqueSelect = true;

            if (array != GetComponent<CreatorManager>().array | AffectationType != affected)
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
                    Vector2 pos = editor.GetWorldPosition(Input.mousePosition);
                    int Selected = editor.GetBloc((int)pos.x, (int)pos.y);
                    if (Selected != -1)
                    {
                        Blocks = Blocks.Union(new string[] { Selected.ToString() }).ToArray();

                        string blocks = "Null";
                        if (Blocks.Length > 0) blocks = Blocks[0];
                        for (int i = 1; i < Blocks.Length; i++)
                            blocks = blocks + "," + Blocks[i];
                        editor.ChangBlocStatus("Blocks", blocks, SB);
                    }

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
                    Vector2 pos = editor.GetWorldPosition(Input.mousePosition);
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

                        string blocks = "Null";
                        if (Blocks.Length > 0) blocks = Blocks[0];
                        for (int i = 1; i < Blocks.Length; i++)
                            blocks = blocks + "," + Blocks[i];
                        editor.ChangBlocStatus("Blocks", blocks, SB);
                    }
                }
            }
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        );
#endif
        }
        else
        {
            if (array == 1 & affected == 0)
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
        affected = AffectationType;
        #endregion
    }

    void Actualise()
    {
        //Blocks
        ChangAffectationType(AffectationType);

        //Translation
        transform.GetChild(2).GetChild(1).GetChild(3).GetComponent<InputField>().text = Translation.x.ToString();
        transform.GetChild(2).GetChild(1).GetChild(5).GetComponent<Toggle>().isOn = TranslationFromPlayer[0];
        transform.GetChild(2).GetChild(2).GetChild(3).GetComponent<InputField>().text = Translation.y.ToString();
        transform.GetChild(2).GetChild(2).GetChild(5).GetComponent<Toggle>().isOn = TranslationFromPlayer[1];
        transform.GetChild(2).GetChild(3).GetComponent<Toggle>().isOn = Reset[0];

        //Settings
        transform.GetChild(3).GetChild(0).GetComponent<Dropdown>().value = Type;
        if (Speed <= 0) Speed = 1;
        transform.GetChild(3).GetChild(1).GetChild(3).GetComponent<InputField>().text = Speed.ToString();
        transform.GetChild(3).GetChild(2).GetComponent<Toggle>().isOn = MultiUsage;

        //Rotation
        transform.GetChild(4).GetChild(1).GetChild(3).GetComponent<InputField>().text = Rotation.x.ToString();
        transform.GetChild(4).GetChild(2).GetChild(3).GetComponent<InputField>().text = Rotation.y.ToString();
        transform.GetChild(4).GetChild(3).GetChild(3).GetComponent<InputField>().text = Rotation.z.ToString();
        transform.GetChild(4).GetChild(4).GetComponent<Toggle>().isOn = Reset[1];
    }


    public void ChangAffectationType(int a)
    {
        AffectationType = a;
        editor.ChangBlocStatus("AffectationType", AffectationType.ToString(), SB);

        for (int i = 0; i < transform.GetChild(1).GetChild(0).childCount; i++)
            transform.GetChild(1).GetChild(0).GetChild(i).gameObject.SetActive(i == a);
        for (int i = 0; i < transform.GetChild(1).GetChild(1).childCount; i++)
            transform.GetChild(1).GetChild(1).GetChild(i).GetComponent<Button>().interactable = !(i == a);
    }

    public void RangeValueChanged(float ScrollID)
    {
        int Cat = (int)ScrollID;
        int Child = Mathf.RoundToInt((ScrollID - (int)ScrollID) * 10F);
        Slider slider = transform.GetChild(Cat).GetChild(Child).GetComponent<Slider>();

        float value = (slider.value - (slider.maxValue / 2)) / 10F;
        float multiplier = 0.05F;
        int max = value.ToString().Length;
        if (max > 5) max = 5;

        float inputFieldValue = -1;
        try { inputFieldValue = float.Parse(slider.transform.GetChild(3).GetComponent<InputField>().text); } catch { }
        if (inputFieldValue <= (slider.maxValue * multiplier) | (int)value < (int)(slider.maxValue * multiplier))
            slider.transform.GetChild(3).GetComponent<InputField>().text = value.ToString().Substring(0, max);
    }
    public void TextRangeValueChanged(float ScrollID)
    {
        int Cat = (int)ScrollID;
        int Child = Mathf.RoundToInt((ScrollID - (int)ScrollID) * 10F);
        Slider slider = transform.GetChild(Cat).GetChild(Child).GetComponent<Slider>();
        InputField inputField = slider.transform.GetChild(3).GetComponent<InputField>();
        if (inputField.text.Length > 4 & !(inputField.text.Contains(".") | inputField.text.Contains("-"))) inputField.text = "9999";
        try
        {
            float value = float.Parse(inputField.text);

            if (ScrollID == 2.1F) Translation.x = value;
            else if (ScrollID == 2.2F) Translation.y = value;
            else if (ScrollID == 4.1F) Rotation.x = value;
            else if (ScrollID == 4.2F) Rotation.y = value;
            else if (ScrollID == 4.3F) Rotation.z = value;

            value = ((float.Parse(inputField.text) * 10F) + (slider.maxValue / 2));
            slider.value = value;
        }
        catch { }

        if (Cat == 2) editor.ChangBlocStatus("Translation", Translation.ToString(), SB);
        else if (Cat == 4) editor.ChangBlocStatus("Rotation", Rotation.ToString(), SB);
    }
    public void ToggleRangeValueChanged(float ScrollID)
    {
        int Cat = (int)ScrollID;
        int Child = Mathf.RoundToInt((ScrollID - (int)ScrollID) * 10F);
        Toggle toggle = transform.GetChild(Cat).GetChild(Child).GetChild(5).GetComponent<Toggle>();

        if (Cat == 2)
        {
            TranslationFromPlayer[Child - 1] = toggle.isOn;

            string param = "(" + TranslationFromPlayer[0].ToString();
            for (int i = 1; i < TranslationFromPlayer.Length; i++)
                param = param + "," + TranslationFromPlayer[i].ToString();
            param = param + ")";
            editor.ChangBlocStatus("TranslationFrom", param, SB);
        }
    }
    public void ToggleResetRangeValueChanged(int ScrollID)
    {
        Transform go = transform.GetChild(ScrollID);
        Toggle reset = go.GetChild(go.childCount - 1).GetComponent<Toggle>();
        for (int i = 1; i < go.childCount - 1; i++)
            go.GetChild(i).gameObject.SetActive(!reset.isOn);

        Reset[(ScrollID / 2) - 1] = reset.isOn;

        string param = "(" + Reset[0].ToString();
        for (int i = 1; i < Reset.Length; i++)
            param = param + "," + Reset[i].ToString();
        param = param + ")";
        editor.ChangBlocStatus("Reset", param, SB);
    }

    public void TypeValueChanged(Dropdown dropdown)
    {
        Type = dropdown.value;
        editor.ChangBlocStatus("Type", Type.ToString(), SB);
    }

    public void SpeedValueChanged()
    {
        Slider slider = transform.GetChild(3).GetChild(1).GetComponent<Slider>();

        float value = slider.value;
        float multiplier = 1;
        int max = value.ToString().Length;
        if (max > 5) max = 5;

        float inputFieldValue = -1;
        try { inputFieldValue = float.Parse(slider.transform.GetChild(3).GetComponent<InputField>().text); } catch { }
        if (inputFieldValue <= (slider.maxValue * multiplier) | (int)value < (int)(slider.maxValue * multiplier))
            slider.transform.GetChild(3).GetComponent<InputField>().text = value.ToString().Substring(0, max);

        editor.ChangBlocStatus("Speed", Speed.ToString(), SB);
    }
    public void TextSpeedValueChanged()
    {
        Slider slider = transform.GetChild(3).GetChild(1).GetComponent<Slider>();
        InputField inputField = slider.transform.GetChild(3).GetComponent<InputField>();
        if (inputField.text.Length > 4 & !(inputField.text.Contains(".") | inputField.text.Contains("-"))) inputField.text = "9999";
        try
        {
            float value = float.Parse(inputField.text);
            Speed = value;
            slider.value = value;
        }
        catch { }

        editor.ChangBlocStatus("MultiUsage", MultiUsage.ToString(), SB);
    }

    public void MultiUsageChanged(Toggle toggle)
    {
        MultiUsage = toggle.isOn;
    }


    public static Vector2 getVector2(string rString)
    {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = System.Convert.ToSingle(temp[0]);
        float y = System.Convert.ToSingle(temp[1]);
        Vector2 rValue = new Vector2(x, y);
        return rValue;
    }
    public static Vector3 getVector3(string rString)
    {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = System.Convert.ToSingle(temp[0]);
        float y = System.Convert.ToSingle(temp[1]);
        float z = System.Convert.ToSingle(temp[2]);
        Vector3 rValue = new Vector3(x, y, z);
        return rValue;
    }
}