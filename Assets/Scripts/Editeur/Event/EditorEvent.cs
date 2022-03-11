using System;
using System.Collections.Generic;
using System.Linq;
using AngryDash.Image.Reader;
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
                if (type == ProgType.visual) VisualSave();
                else if (type == ProgType.textual) TextualSave();
            }
            foreach (Transform child in transform.GetChild(1).GetChild(1)) Destroy(child.gameObject);

            type = newType; //Set the new type
            GetComponent<MenuManager>().Array((int)type); //Change the menu

            //Load
            if (type == ProgType.visual) UnityThread.executeInUpdate(() => VisualInitialization());
            else if (type == ProgType.textual) TextualInitialization();
        }

        private void OnEnable() { editor.canInteract = false; ChangeType(type, false); }
        public void Exit()
        {
            if (type == ProgType.visual)
            {
                VisualSave();
                foreach (Transform child in transform.GetChild(1).GetChild(1)) Destroy(child.gameObject);
            }
            else if (type == ProgType.textual) TextualSave();

            foreach (var id in editor.SelectedBlock)
            {
                var obj = editor.transform.GetChild(1).Find("Objet n° " + id);
                if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
            }
            editor.SelectedBlock = new int[0];

            editor.canInteract = true;
        }

        #region Visual

        private Dictionary<string, EditorEventItem> visualPrefabs = new Dictionary<string, EditorEventItem>();

        private readonly Dictionary<Type, string[]> ids = new Dictionary<Type, string[]>
        {
            { Type.trigger, new[] { "start", "collision", "respawn" } },
            { Type.action, new[] {
                "end", "lose", "bgColor", "bgChange", //game
                "checkpoint", "teleport", //player
                "changeTexture", //event
                "color", "active", //group
                "wait" //other
            } },
            //{ Type.conditional, new string[] { "if" } }
        };

        private void VisualInitialization()
        {
            var visual = transform.GetChild(1);
            Transform elements = visual.GetChild(0).GetChild(1).GetChild(0).GetComponent<ScrollRect>().content;

            foreach (var cat in ids)
            {
                foreach (var id in cat.Value)
                {
                    var config = Resources.Load<GameObject>($"Events/{cat.Key}/{id}");
                    if (config != null)
                    {
                        if (!visualPrefabs.ContainsKey(id))
                        {
                            var elementCat = elements.Find(cat.Key.ToString());
                            var Slot = Instantiate(elementCat.GetChild(0).gameObject, elementCat);
                            var Item = Instantiate(config, Slot.transform).GetComponent<EditorEventItem>();

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
                    else Debug.LogWarning($"<b>{id}</b> has no prefab at path <b>Events/{cat.Key}/{id}</b>");
#endif
                }
            }
            if (editor.SelectedBlock.Length == 1) VisualParse(editor.GetBlocStatus("Script", editor.SelectedBlock[0]));
        }

        private HashSet<string> triggerImplemented;
        public void VisualSave()
        {
            triggerImplemented = new HashSet<string>();
            var script = $"-- Auto-generated script from the visual programming panel\n\n{VisualToScript(transform.GetChild(1).GetChild(1))}";
            editor.ChangBlocStatus("Script", script, editor.SelectedBlock);
        }
        private string VisualToScript(Transform field, string prefix = "")
        {
            var script = new StringBuilder(prefix) { appendEmptyStrings = false };
            foreach (Transform go in field)
            {
                if (script.Length > 0) script.AppendLine();

                var item = go.GetComponent<EditorEventItem>();
                if (field.name != "Programme" | item.type == Type.trigger)
                {
                    if (item.type == Type.trigger)
                    {
                        script.AppendLine($"function {item.methodName}()");
                        foreach (var childField in item.fields)
                        {
                            if (childField.transform.childCount > 0) script.AppendLine(VisualToScript(childField.transform, "\t"));
                        }
                        script.AppendLine("end");
                        triggerImplemented.Add(item.id);
                    }
                    else if (item.type == Type.action)
                    {
                        var actions = new List<string>();
                        foreach (var parameter in item.parameters)
                        {
                            var actionPrefix = "";
                            var actionSufix = "";
                            if (parameter.Value.Key == EventParameter.Type.Number) actionPrefix = actionSufix = "";
                            else actionPrefix = actionSufix = "\"";
                            actions.Add(actionPrefix + parameter.Value.Value + actionSufix);
                        }
                        script.AppendLine($"{item.methodName}({string.Join(", ", actions)})");
                    }
                    else if (item.type == Type.conditional)
                    {
                        var condition = "";
                        var actions = new StringBuilder { appendEmptyStrings = false };
                        foreach (var childField in item.fields)
                        {
                            if (childField.accepted == Type.logicalOperator)
                            {
                                if (!string.IsNullOrEmpty(condition)) condition += ", ";
                                condition += VisualToScript(childField.transform);
                            }
                            else
                            {
                                if (childField.id != "then") actions.AppendLine(childField.id);
                                actions.AppendLine("then");
                                if (childField.transform.childCount > 0) actions.AppendLine(VisualToScript(childField.transform, "\t"));
                                actions.AppendLine("end");
                            }
                        }
                        script.AppendLine($"{item.methodName} ({condition})");
                        script.Merge(actions);
                    }
                    else if (item.type == Type.logicalOperator)
                        throw new NotImplementedException("Logical operators are not supported for the moment");
                }
            }
            return script.ToString();
        }

        private void VisualParse(string script)
        {
            var lines = script.Split("\n");
            var topParent = transform.GetChild(1).GetChild(1);
            var parent = topParent;

            Transform lastParent = null;
            Transform lastObj = null;
            string fieldID = null;
            foreach (var l in lines)
            {
                var line = l.Replace("\t", "");

                if (line.Contains("function ")) //Trigger
                    parent = SpawnObj(line.Remove(line.LastIndexOf("(")).Remove(0, "function ".Length)).transform;
                else if (line.Contains("end"))
                {
                    if (lastObj == parent)
                    {
                        parent = parent.parent != topParent ? parent.parent.parent.parent : parent.parent;
                        Destroy(lastObj.gameObject); //The parent is empty, so delete it
                    }
                    else
                    {
                        var parentItem = parent.GetComponent<EditorEventItem>();
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
                    var argIndex = line.LastIndexOf("(");
                    var go = SpawnObj(line.Remove(argIndex));
                    if (go != null)
                    {
                        var item = go.GetComponent<EditorEventItem>();
                        argIndex += 1;
                        var args = line.Substring(argIndex, line.LastIndexOf(")") - argIndex).Split(", ");
                        for (var i = 0; i < args.Length & i < item.parameters.Length; i++)
                        {
                            var actionPrefix = "";
                            var actionSufix = "";
                            if (item.parameters[i].Value.Key == EventParameter.Type.Number) actionPrefix = actionSufix = "";
                            else actionPrefix = actionSufix = "\"";
                            item.parameters[i].Value = new KeyValuePair<EventParameter.Type, string>(EventParameter.Type.Text, args[i].TrimStart(actionPrefix).TrimEnd(actionSufix));
                        }
                    }
                    else
                    {
                        ChangeType(ProgType.textual, false);
                        break;
                    }
                }
                else if (!line.Contains("--") & line != "")
                {
                    ChangeType(ProgType.textual, false);
                    break;
                }
            }

            GameObject SpawnObj(string method)
            {
                var id = "";
                if (visualPrefabs.ContainsKey(method)) id = method;
                else id = visualPrefabs.Values.FirstOrDefault(p => p.methodName == method)?.id;

                if (!string.IsNullOrEmpty(id))
                {
                    var prefab = visualPrefabs[id];
                    var objParent = parent;

                    if (parent != topParent)
                    {
                        var parentItem = parent.GetComponent<EditorEventItem>();
                        var field = parentItem.fields.Where(f => f.CanDrop(prefab.type) & (f.id == fieldID | fieldID == null)).FirstOrDefault();
                        if (field != null) objParent = field.transform;
                    }
                    var go = Instantiate(prefab.gameObject, objParent);
                    go.SetActive(true);
                    lastObj = go.transform;
                    return go;
                }

                return null;
            }

            UnityThread.executeInUpdate(() =>
            {
                var lastPos = new Vector2(25, -25);
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
            var script = transform.GetChild(2).GetChild(1).GetComponent<InputField>().text;
            editor.ChangBlocStatus("Script", script, editor.SelectedBlock);
        }

        private void TextualInitialization()
        {
            var script = "";
            if (editor.SelectedBlock.Length == 1) script = editor.GetBlocStatus("Script", editor.SelectedBlock[0]);
            transform.GetChild(2).GetChild(1).GetComponent<InputField>().text = script;
        }
        #endregion
    }
}
