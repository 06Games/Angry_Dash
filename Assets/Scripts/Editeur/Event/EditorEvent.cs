using AngryDash.Image.Reader;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Editor.Event
{
    public class EditorEvent : MonoBehaviour
    {
        public Editeur editor;
        public enum ProgType { visual, textual }
        public ProgType type = ProgType.visual;

        public void ChangeType(int argument) { ChangeType((ProgType)argument); }
        public void ChangeType(ProgType newType, bool save = true)
        {
            if (save)
            {
                //Save
                if (type == ProgType.visual)
                {
                    VisualSave();
                    foreach (Transform child in transform.GetChild(1).GetChild(1)) Destroy(child.gameObject);
                }
                else if (type == ProgType.textual) TextualSave();
            }

            type = newType; //Set the new type
            GetComponent<MenuManager>().Array((int)type); //Change the menu

            //Load
            if (type == ProgType.visual) UnityThread.executeInUpdate(() => VisualInitialization());
            else if (type == ProgType.textual) TextualInitialization();
        }

        void OnEnable() { editor.bloqueSelect = true; ChangeType(type, false); }
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
        Dictionary<string, EditorEventItem> visualPrefabs = new Dictionary<string, EditorEventItem>();
        readonly Dictionary<Type, string[]> ids = new Dictionary<Type, string[]>() {
            { Type.trigger, new string[] { "start", "collision" } },
            { Type.action, new string[] {
                "end", "lose", "checkpoint", "teleport", //player
                "changeTexture", //event
                "color" //group
            } },
            //{ Type.conditional, new string[] { "if" } }
        };

        void VisualInitialization()
        {
            Transform visual = transform.GetChild(1);
            Transform elements = visual.GetChild(0).GetChild(0).GetComponent<ScrollRect>().content;

            List<string> idList = new List<string>();
            foreach (var keyPair in ids) idList.AddRange(keyPair.Value);
            foreach (string id in idList)
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

            if (editor.SelectedBlock.Length == 1) VisualParse(editor.GetBlocStatus("Script", editor.SelectedBlock[0]));
        }

        HashSet<string> triggerImplemented;
        public void VisualSave()
        {
            triggerImplemented = new HashSet<string>();
            string script = $"// Auto-generated script from the visual programming panel\n\n{VisualToScript(transform.GetChild(1).GetChild(1))}";
            foreach (string id in ids[Type.trigger])
            {
                if (!triggerImplemented.Contains(id)) script += $"\npublic void {visualPrefabs[id].methodName}()\n{{\n}}";
            }
            editor.ChangBlocStatus("Script", script, editor.SelectedBlock);
        }
        private string VisualToScript(Transform field, string prefix = "")
        {
            StringBuilder script = new StringBuilder(prefix) { appendEmptyStrings = false };
            foreach (Transform go in field)
            {
                if (script.Length > 0) script.AppendLine();

                EditorEventItem item = go.GetComponent<EditorEventItem>();
                if (field.name != "Programme" | item.type == Type.trigger)
                {
                    if (item.type == Type.trigger)
                    {
                        script.AppendLine($"public void {item.methodName}()");
                        script.AppendLine("{");
                        foreach (EventField childField in item.fields)
                        {
                            if (childField.transform.childCount > 0) script.AppendLine(VisualToScript(childField.transform, "\t"));
                        }
                        script.AppendLine("}");
                        triggerImplemented.Add(item.id);
                    }
                    else if (item.type == Type.action)
                    {
                        List<string> actions = new List<string>();
                        foreach (EventParameter parameter in item.parameters)
                        {
                            string actionPrefix = "";
                            string actionSufix = "";
                            if (parameter.value.contentType == InputField.ContentType.DecimalNumber) actionSufix = "F";
                            else if (parameter.value.contentType.In(InputField.ContentType.IntegerNumber, InputField.ContentType.Pin)) actionPrefix = actionSufix = "";
                            else actionPrefix = actionSufix = "\"";
                            actions.Add(actionPrefix + parameter.value.text + actionSufix);
                        }
                        script.AppendLine($"{item.methodName}({string.Join(", ", actions)});");
                    }
                    else if (item.type == Type.conditional)
                    {
                        string condition = "";
                        StringBuilder actions = new StringBuilder() { appendEmptyStrings = false };
                        foreach (EventField childField in item.fields)
                        {
                            if (childField.accepted == Type.logicalOperator)
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
                        script.AppendLine($"{item.methodName} ({condition})");
                        script.Merge(actions);
                    }
                    else if (item.type == Type.logicalOperator)
                        throw new System.NotImplementedException("Logical operators are not supported for the moment");
                }
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
            foreach (string l in lines)
            {
                string line = l.Replace("\t", "");

                if (line.Contains("public void ")) //Trigger
                    SpawnObj(line.Remove(line.LastIndexOf("(")).Remove(0, "public void ".Length));
                else if (line.Contains("{")) parent = lastObj;
                else if (line.Contains("}"))
                {
                    if (lastObj == parent)
                    {
                        parent = parent.parent != topParent ? parent.parent.parent.parent : parent.parent;
                        Destroy(lastObj.gameObject); //The parent is empty, so delete it
                    }
                    else
                    {
                        EditorEventItem parentItem = parent.GetComponent<EditorEventItem>();
                        UnityThread.executeInUpdate(() => parentItem.UpdateSize());
                        lastParent = parent;
                        parent = parent.parent != topParent ? parent.parent.parent.parent : parent.parent;
                    }
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
                else if (line.Contains("(") & line.Contains(")")) //Action
                {
                    int argIndex = line.LastIndexOf("(");
                    var go = SpawnObj(line.Remove(argIndex));
                    if (go != null)
                    {
                        EditorEventItem item = go.GetComponent<EditorEventItem>();
                        argIndex += 1;
                        string[] args = line.Substring(argIndex, line.LastIndexOf(")") - argIndex).Split(", ");
                        for (int i = 0; i < args.Length & i < item.parameters.Length; i++)
                        {
                            string actionPrefix = "";
                            string actionSufix = "";
                            if (item.parameters[i].value.contentType == InputField.ContentType.DecimalNumber) actionSufix = "F";
                            else if (item.parameters[i].value.contentType.In(InputField.ContentType.IntegerNumber, InputField.ContentType.Pin)) actionPrefix = actionSufix = "";
                            else actionPrefix = actionSufix = "\"";
                            item.parameters[i].value.text = args[i].TrimStart(actionPrefix).TrimEnd(actionSufix);
                        }
                    }
                    else
                    {
                        ChangeType(ProgType.textual, false);
                        break;
                    }
                }
                else if (!line.Contains("//", "/*") & line != "")
                {
                    ChangeType(ProgType.textual, false);
                    break;
                }
            }

            GameObject SpawnObj(string method)
            {
                string id = "";
                if (visualPrefabs.ContainsKey(method)) id = method;
                else
                {
                    foreach (EditorEventItem prefab in visualPrefabs.Values)
                    {
                        if (prefab.methodName == method) { id = prefab.id; break; }
                    }
                }

                if(!string.IsNullOrEmpty(id))
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

            UnityThread.executeInUpdate(() =>
            {
                Vector2 lastPos = new Vector2(25, -25);
                foreach (RectTransform transform in topParent)
                {
                    transform.anchoredPosition = transform.rect.size * new Vector2(0.5F, -0.5F) + lastPos;
                    lastPos.x += transform.rect.size.x + 25;
                }
            });
        }
        #endregion

        #region Textual
        public void TextualSave()
        {
            string script = transform.GetChild(2).GetChild(1).GetComponent<InputField>().text;
            editor.ChangBlocStatus("Script", script, editor.SelectedBlock); 
        }

        void TextualInitialization()
        {
            string script = "";
            if (editor.SelectedBlock.Length == 1) script = editor.GetBlocStatus("Script", editor.SelectedBlock[0]);
            transform.GetChild(2).GetChild(1).GetComponent<InputField>().text = script;
        }
        #endregion
    }
}
