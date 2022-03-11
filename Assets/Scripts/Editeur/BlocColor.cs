using Tools;
using UnityEngine;
using UnityEngine.UI;

public class BlocColor : MonoBehaviour
{

    public Editeur editeur;
    public int[] Bloc;
    private ColorPicker CP;
    public ColorPicker cpExpend;

    private void Start()
    {
        CP = transform.GetChild(0).GetComponent<ColorPicker>();
        if (string.IsNullOrEmpty(editeur.file)) gameObject.SetActive(false);
    }

    private void Update()
    {
        if (editeur.SelectedBlock.Length == 0) { transform.parent.parent.GetComponent<Edit>().EnterToEdit(); return; }

        if (Bloc != editeur.SelectedBlock)
        {
            Bloc = editeur.SelectedBlock;
            Expend(false);
        }
        else
        {
            if (transform.GetChild(0).gameObject.activeInHierarchy)
                editeur.ChangBlocStatus("Color", CP.CurrentColor.ToHex(), Bloc);
            else editeur.ChangBlocStatus("Color", cpExpend.CurrentColor.ToHex(), Bloc);
        }
    }

    public void Expend(bool expend)
    {
        if (gameObject.activeInHierarchy)
        {
            transform.GetChild(0).gameObject.SetActive(!expend);
            cpExpend.transform.parent.gameObject.SetActive(expend);
            CP.transform.GetChild(0).GetChild(1).GetComponent<HexColorField>().displayAlpha = true; //Don't include alpha in hex codes
            CP.transform.GetChild(4).GetChild(3).gameObject.SetActive(true); //Sets to RGBA

            if (Bloc.Length == 0) { transform.parent.parent.GetComponent<Edit>().EnterToEdit(); return; }
            CP.CurrentColor = ColorExtensions.ParseHex(editeur.GetBlocStatus("Color", Bloc[0]));
            cpExpend.CurrentColor = ColorExtensions.ParseHex(editeur.GetBlocStatus("Color", Bloc[0]));
            editeur.canInteract = !expend;
            editeur.SelectedBlock = Bloc;

            var BG = cpExpend.transform.GetChild(0).GetChild(0).gameObject;
            BG.SetActive(true); //Actives preview
            try
            {
                //Sets the image to the preview
                BG.GetComponent<Image>().sprite = GameObject.Find("Objet n° " + Bloc[0]).GetComponent<SpriteRenderer>().sprite;
            }
            catch { BG.SetActive(false); }
        }
        else
        {
            cpExpend.transform.parent.gameObject.SetActive(expend);
            editeur.canInteract = true;
        }
    }
}
