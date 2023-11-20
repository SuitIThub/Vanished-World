using UnityEngine;
using ImageComp = UnityEngine.UI.Image;

namespace DisplayScreen
{
    public class Image : Module
    {
        private SpriteData icon;

        private ImageComp image;

        public Image(Window parent, string name) : base(parent, name)
        {

        }

        public Image SetImage(SpriteData sprite)
        {
            icon = sprite;
            image.sprite = icon.sprite;
            return this;
        }

        protected internal override void loadGO()
        {
            rect = GameObject.Instantiate(
                CentreBrain.data.Prefabs["WindowModuleImage"],
                parent.panel.transform).GetComponent<RectTransform>();

            rect.name = name;
            image = rect.GetComponent<ImageComp>();
        }

        internal override void UpdateSize()
        {
            float width = GetWidth() - border.x - border.z;

            if (icon != null)
                size = icon.normalizeSize(width, -1);
            else
                size = new Vector2(width, 0);

            float h = 0;
            if (parent.rowHeight.Count > rowPos)
                h = parent.rowHeight[rowPos];

            if (rowAmount == 1 && h < size.y)
                parent.SetRowHeight(rowPos, size.y + border.y + border.w);

            image.rectTransform.sizeDelta = size;
        }
    }
}