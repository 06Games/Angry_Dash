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
            type = newType;

            if (type == ProgType.visual) VisualInitialization();
            else Debug.LogError("Textual programmation is unsupported for the moment");
        }

        void OnDisable() { VisualSave(); editor.bloqueSelect = false; }
        void OnEnable() { editor.bloqueSelect = true; ChangeType(type); }

        void VisualInitialization()
        {
            Transform visual = transform.GetChild(1);
            Transform elements = visual.GetChild(0).GetComponent<ScrollRect>().content;

            string[] ids = new string[] {
                "collision", //trigger
                //"color", //action
                "if", "else" //condition
            };
            foreach (string id in ids)
            {
                GameObject config = Resources.Load<GameObject>($"Events/{id}");
                if (config != null)
                {
                    if (elements.Find(id) == null)
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
                    }
                }
#if UNITY_EDITOR
                else Debug.LogWarning($"<b>{id}</b> has no prefab");
#endif
            }
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
                EditorEventItem item = go.GetComponent<EditorEventItem>();
                if (item.type == EditorEventItem.Type.trigger)
                {
                    script.AppendLine($"void {item.id}()");
                    script.AppendLine("{");
                    foreach (EventField childField in item.fields)
                    {
                        if (childField.transform.childCount > 0) script.AppendLine(VisualToScript(childField.transform, $"{prefix}\t"));
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
                            if (childField.transform.childCount > 0) actions.AppendLine(VisualToScript(childField.transform, $"\t"));
                            actions.AppendLine("}");
                        }
                    }
                    script.AppendLine($"if({condition})");
                    script.Merge(actions);
                }
                else if (item.type == EditorEventItem.Type.logicalOperator)
                    throw new System.NotImplementedException("Logical operators are not supported for the moment");
            }
            return script.ToString();
        }
    }
}
