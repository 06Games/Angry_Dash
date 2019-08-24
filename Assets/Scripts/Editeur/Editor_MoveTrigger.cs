﻿using UnityEngine;
using UnityEngine.UI;

public class Editor_MoveTrigger : MonoBehaviour
{

    public Editeur editor;
    int[] SB;
    int array;
    int affected;

    public int AffectationType = 0;
    public int Group;
    public Vector2 Translation;
    public bool[] TranslationFromPlayer = new bool[2];
    public int Type;
    public float Speed;
    public bool MultiUsage;
    public Vector3 Rotation;
    public bool[] Reset = new bool[2];
    public bool GlobalRotation = false;

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
                int.TryParse(editor.GetBlocStatus("Group", SB[0]), out Group);

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
                try { GlobalRotation = bool.Parse(editor.GetBlocStatus("GlobalRotation", SB[0])); } catch { }

                try { Type = int.Parse(editor.GetBlocStatus("Type", SB[0])); } catch { }
                try { Speed = float.Parse(editor.GetBlocStatus("Speed", SB[0])); } catch { }
                try { MultiUsage = bool.Parse(editor.GetBlocStatus("MultiUsage", SB[0])); } catch { }

                try { Rotation = getVector3(editor.GetBlocStatus("Rotation", SB[0])); } catch { }

                Actualise();
            }
            else { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }
        }
    }

    void Actualise()
    {
        //Blocks
        ChangAffectationType(AffectationType);
        transform.GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetComponent<InputField>().text = Group.ToString();

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
        transform.GetChild(4).GetChild(5).GetComponent<Toggle>().isOn = GlobalRotation;
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
    public void GroupeChange(InputField field) {
        if (!string.IsNullOrEmpty(field.text))
        {
            int.TryParse(field.text, out int groupe);
            if (groupe < 0) groupe = 0;
            Group = groupe;
            editor.ChangBlocStatus("Group", Group.ToString(), SB);
        }
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
        Toggle reset = null;
        if (ScrollID == 2) reset = go.GetChild(go.childCount - 1).GetComponent<Toggle>();
        else if (ScrollID == 4) reset = go.GetChild(go.childCount - 2).GetComponent<Toggle>();

        for (int i = 1; i < go.childCount; i++)
            if (go.GetChild(i) != reset.transform)
                go.GetChild(i).gameObject.SetActive(!reset.isOn);

        Reset[(ScrollID / 2) - 1] = reset.isOn;

        string param = "(" + Reset[0].ToString();
        for (int i = 1; i < Reset.Length; i++)
            param = param + "," + Reset[i].ToString();
        param = param + ")";
        editor.ChangBlocStatus("Reset", param, SB);
    }
    public void ToggleGlobalRotationRangeValueChanged()
    {
        Transform go = transform.GetChild(4);

        GlobalRotation = go.GetChild(go.childCount - 1).GetComponent<Toggle>();

        editor.ChangBlocStatus("GlobalRotation", GlobalRotation.ToString(), SB);
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