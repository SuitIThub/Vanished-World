using UnityEngine;

namespace DisplayScreen
{
    public abstract class Module
    {
        public readonly string id;
        public string name;

        private protected Window parent;

        private protected int rowPos;
        private protected int colPos;
        private protected int rowAmount;
        private protected int colAmount;

        private protected Vector2 pos;
        private protected Vector2 size;

        private protected RectTransform rect;

        private protected Vector4 border;

        public Module(Window parent, string name)
        {
            id = IdUtilities.id;

            this.name = name;

            this.parent = parent;

            rowPos = 0;
            colPos = 0;
            rowAmount = 1;
            colAmount = 1;

            border = Vector4.zero;

            loadGO();
        }

        public Module SetRowPos(int rowPos)
        {
            this.rowPos = rowPos;
            return this;
        }

        public Module SetColPos(int colPos)
        {
            this.colPos = colPos;
            return this;
        }

        public Module SetRowAmount(int amount)
        {
            this.rowAmount = amount;
            return this;
        }

        public Module SetColAmount(int amount)
        {
            this.colAmount = amount;
            return this;
        }

        public Module SetBorder(float x = 0, float y = 0, float z = 0, float w = 0)
        {
            this.border = new Vector4(x, y, z, w);
            return this;
        }

        public Module SetBorder(Vector4 border)
        {
            this.border = border;
            return this;
        }

        abstract protected internal void loadGO();

        abstract internal void UpdateSize();
        internal void UpdatePos()
        {
            float posX = 0;
            for (int i = 0; i < colPos; i++)
                posX += parent.GetColumnWidth(i);
            posX += ((size.x + border.x + border.z) / 2) + parent.moduleOffset.x + (border.x / 2) - (border.z / 2);

            float posY = 0;
            for (int i = 0; i < rowPos; i++)
                posY -= parent.GetRowHeight(i);
            posY -= ((size.y + border.y + border.w) / 2) + parent.moduleOffset.y + (border.y / 2) - (border.w / 2);

            rect.localPosition = new Vector2(posX, posY);
        }
        internal void UpdateSize2()
        {
            float height = 0;
            for (int i = rowPos; i < rowPos + rowAmount; i++)
            {
                if (parent.rowHeight.Count > i)
                    height += parent.rowHeight[i];
                else
                    parent.rowHeight.Add(0);
            }

            if (height < size.y)
                parent.rowHeight[rowPos + rowAmount - 1] += size.y.difference(height);
        }

        private protected float GetWidth()
        {
            float f = 0;

            for (int i = colPos; i < colPos + colAmount; i++)
                f += parent.GetColumnWidth(i);

            return f;
        }
    }
}