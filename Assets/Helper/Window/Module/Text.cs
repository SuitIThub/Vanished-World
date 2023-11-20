using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace DisplayScreen
{
    public class Text : Module
    {
        private string text = "";

        TextMeshProUGUI textMesh;

        public Text(Window parent, string name) : base(parent, name)
        {

        }

        public Text SetText(string text)
        {
            this.text = text;
            textMesh.text = text;
            return this;
        }

        public Text SetFontSize(float size)
        {
            textMesh.fontSize = size;
            return this;
        }

        public Text SetWrapping(bool isWrappingEnabled)
        {
            textMesh.enableWordWrapping = isWrappingEnabled;
            return this;
        }

        public Text setOverflow(TextOverflowModes overflowMode, Text link = null)
        {
            textMesh.overflowMode = overflowMode;
            if (overflowMode == TextOverflowModes.Linked)
                textMesh.linkedTextComponent = link.textMesh;
            return this;
        }

        protected internal override void loadGO()
        {
            rect = GameObject.Instantiate(
                CentreBrain.data.Prefabs["WindowModuleText"],
                parent.panel.transform).GetComponent<RectTransform>();

            rect.name = name;
            textMesh = rect.GetComponent<TextMeshProUGUI>();
        }

        internal override void UpdateSize()
        {
            float width = GetWidth();

            textMesh.ForceMeshUpdate();

            float height = textMesh.textBounds.size.y;
            size = new Vector2(width - border.x - border.z, height);

            float h = 0;
            if (parent.rowHeight.Count > rowPos)
                h = parent.rowHeight[rowPos];

            if (rowAmount == 1 && h < size.y)
                parent.SetRowHeight(rowPos, size.y + border.y + border.w);

            textMesh.rectTransform.sizeDelta = size;
        }
    }
}