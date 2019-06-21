using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.Event
{
    public class EditorEvent : MonoBehaviour
    {
        public Editeur editor;
        public enum ProgType { visual, textual }
        public ProgType type = ProgType.visual;

        public void ChangeType(int argument) { ChangeType((ProgType)argument); }
        public void ChangeType(ProgType newType)
        {
            //Save
            if (type == ProgType.visual)
            {
                VisualSave();
                foreach (Transform child in transform.GetChild(1).GetChild(1)) Destroy(child.gameObject);
            }
            else if (type == ProgType.textual) TextualSave();

            type = newType; //Set the new type

            //Load
            if (type == ProgType.visual) UnityThread.executeInUpdate(() => VisualInitialization());
            else if (type == ProgType.textual) TextualInitialization();
        }

        void OnEnable() { editor.bloqueSelect = true; ChangeType(type); }
        public void Exit()
        {
            if (type == ProgType.visual)
            {
                VisualSave();
                foreach (Transform child in transform.GetChild(1).GetChild(1)) Destroy(child.gameObject);
            }
            else if (type == ProgType.textual) TextualSave();

            foreach (int id in editor.SelectedBlock)
            {
                Transform obj = editor.transform.GetChild(1).Find("Objet n° " + id);
                if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
            }
            editor.SelectedBlock = new int[0];

            editor.bloqueSelect = false;
        }

        #region Visual
        System.Collections.Generic.Dictionary<string, EditorEventItem> visualPrefabs = new System.Collections.Generic.Dictionary<string, EditorEventItem>();

        void VisualInitialization()
        {
            Transform visual = transform.GetChild(1);
            Transform elements = visual.GetChild(0).GetComponent<ScrollRect>().content;

            string[] ids = new string[] {
                "collision", //trigger
                //"color", //action
                "if" //condition
            };
            foreach (string id in ids)
            {
                GameObject config = Resources.Load<GameObject>($"Events/{id}");
                if (config != null)
                {
                    if (!visualPrefabs.ContainsKey(id))
                    {
                        GameObject Slot = Instantiate(elements.GetChild(0).gameObject, elements);
                        EditorEventItem Item = Instantiate(config, Slot.transform).GetComponent<EditorEventItem>();

                        Slot.name = Item.id = id;
                        Slot.GetComponent<GridLayoutGroup>().cellSize =
                            Slot.GetComponent<RectTransform>().sizeDelta =
                            Item.referenceSize =
                            Item.GetComponent<RectTransform>().sizeDelta;
                        Slot.GetComponent<UImage_Reader>().baseID = Item.GetComponent<UImage_Reader>().baseID;
                        Slot.SetActive(true);

                        visualPrefabs.Add(id, Item);
                    }
                }
#if UNITY_EDITOR
                else Debug.LogWarning($"<b>{id}</b> has no prefab");
#endif
            }

            if(editor.SelectedBlock.Length == 1) VisualParse(editor.GetBlocStatus("Script", editor.SelectedBlock[0]));
        }

        public void VisualSave()
        {
            string script = $"// Auto-generated script from the visual programming panel\n\n{VisualToScript(transform.GetChild(1).GetChild(1))}";
            editor.ChangBlocStatus("Script", script, editor.SelectedBlock);
        }
        private string VisualToScript(Transform field, string prefix = "")
        {
            StringBuilder script = new StringBuilder(prefix) { appendEmptyStrings = false };
            foreach (Transform go in field)
            {
                if (script.Length > 0) script.AppendLine();

                EditorEventItem item = go.GetComponent<EditorEventItem>();
                if (item.type == EditorEventItem.Type.trigger)
                {
                    script.AppendLine($"void {item.id}()");
                    script.AppendLine("{");
                    foreach (EventField childField in item.fields)
                    {
                        if (childField.transform.childCount > 0) script.AppendLine(VisualToScript(childField.transform, "\t"));
                    }
                    script.AppendLine("}");
                }
                else if (item.type == EditorEventItem.Type.action) script.AppendLine($"{item.id}()");
                else if (item.type == EditorEventItem.Type.conditional)
                {
                    string condition = "";
                    StringBuilder actions = new StringBuilder() { appendEmptyStrings = false };
                    foreach (EventField childField in item.fields)
                    {
                        if (childField.accepted == EditorEventItem.Type.logicalOperator)
                        {
                            if (!string.IsNullOrEmpty(condition)) condition += ", ";
                            condition += VisualToScript(childField.transform);
                        }
                        else
                        {
                            if (childField.id != "then") actions.AppendLine(childField.id);
                            actions.AppendLine("{");
                            if (childField.transform.childCount > 0) actions.AppendLine(VisualToScript(childField.transform, "\t"));
                            actions.AppendLine("}");
                        }
                    }
                    script.AppendLine($"if ({condition})");
                    script.Merge(actions);
                }
                else if (item.type == EditorEventItem.Type.logicalOperator)
                    throw new System.NotImplementedException("Logical operators are not supported for the moment");
            }
            return script.ToString();
        }

        void VisualParse(string script)
        {
            string[] lines = script.Split("\n");
            Transform topParent = transform.GetChild(1).GetChild(1);
            Transform parent = topParent;

            Transform lastParent = null;
            Transform lastObj = null;
            string fieldID = null;
            foreach (string line in lines)
            {
                if (line.Contains("void ")) //Trigger
                    SpawnObj(line.Remove(line.LastIndexOf("(")).Remove(0, "void ".Length));
                else if (line.Contains("{")) parent = lastObj;
                else if (line.Contains("}"))
                {
                    EditorEventItem parentItem = parent.GetComponent<EditorEventItem>();
                    UnityThread.executeInUpdate(() => parentItem.UpdateSize());
                    lastParent = parent;
                    parent = parent.parent != topParent ? parent.parent.parent.parent : parent.parent;
                    fieldID = null;
                }
                else if (line.Contains("if (")) //Condition
                {
                    SpawnObj("if");
                    fieldID = "then";
                }
                else if (line.Contains("else"))
                { 
                    parent = lastParent; //Restore the IF
                    fieldID = "else";
                }
                else if (line.Contains("()")) //Action
                    SpawnObj(line.Remove(line.LastIndexOf("(")));
            }

            GameObject SpawnObj(string id)
            {
                if (visualPrefabs.ContainsKey(id))
                {
                    EditorEventItem prefab = visualPrefabs[id];
                    Transform objParent = parent;

                    if (parent != topParent)
                    {
                        EditorEventItem parentItem = parent.GetComponent<EditorEventItem>();
                        EventField field = parentItem.fields.Where(f => f.CanDrop(prefab.type) & (f.id == fieldID | fieldID == null)).FirstOrDefault();
                        if (field != null) objParent = field.transform;
                    }
                    GameObject go = Instantiate(prefab.gameObject, objParent);
                    go.SetActive(true);
                    lastObj = go.transform;
                    return go;
                }
                else return null;
            }

            Vector2 lastPos = new Vector2(25, -25);
            foreach (RectTransform transform in topParent)
            {
                transform.anchoredPosition = transform.sizeDelta * new Vector2(0.5F, -0.5F) + lastPos;
                lastPos.x += transform.sizeDelta.x + 25;
            }
        }
        #endregion

        #region Textual
        public void TextualSave()
        {
            string script = transform.GetChild(2).GetChild(0).GetComponent<InputField>().text;
            editor.ChangBlocStatus("Script", script, editor.SelectedBlock); 
        }

        void TextualInitialization()
        {
            string script = "";
            if (editor.SelectedBlock.Length == 1) script = editor.GetBlocStatus("Script", editor.SelectedBlock[0]);
            transform.GetChild(2).GetChild(0).GetComponent<InputField>().text = script;
        }
        #endregion
    }
}
