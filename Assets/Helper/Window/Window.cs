using Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DisplayScreen
{
    public class Window
    {
        public readonly string id;

        private Dictionary<string, Module> modulesName;
        private Dictionary<string, Module> modulesID;

        internal List<float> rowHeight { get; private set; }
        internal List<float> columnWidth { get; private set; }

        internal float windowWidth;
        internal float absoluteWidths = 0;

        internal UnityEngine.UI.Image panel;

        internal Vector2 pos;
        internal Enums.Alignment.Horizontal horizontalAlignment = 0;
        internal Enums.Alignment.Vertical verticalAlignment = 0;
        internal Vector2 offset;

        internal Vector2 moduleOffset;

        public Window()
        {
            id = IdUtilities.id;

            modulesName = new Dictionary<string, Module>();
            modulesID = new Dictionary<string, Module>();

            rowHeight = new List<float>();
            columnWidth = new List<float>();

            windowWidth = 0f;

            loadGO();
        }

        #region config
        public Window SetHorizontalAlignment(Enums.Alignment.Horizontal alignment)
        {
            this.horizontalAlignment = alignment;
            return this;
        }

        public Window SetVerticalAlignment(Enums.Alignment.Vertical alignment)
        {
            this.verticalAlignment = alignment;
            return this;
        }

        public Window SetCenter(Vector2 center)
        {
            pos = center;
            return this;
        }

        public Window SetOffset(Vector2 offset)
        {
            this.offset = offset;
            return this;
        }

        public Window addModule(Module module)
        {
            if (modulesName.ContainsKey(module.name) ||
                modulesID.ContainsKey(module.id))
                return this;

            modulesName.Add(module.name, module);
            modulesID.Add(module.id, module);

            return this;
        }

        public Window SetColWidth(int column, float width)
        {
            while (columnWidth.Count <= column)
                columnWidth.Add(0);
            columnWidth[column] = width;

            absoluteWidths = 0;
            foreach(float f in columnWidth)
            {
                if (f > 1)
                    absoluteWidths += f;
            }

            return this;
        }

        public Window SetWindowWidth(float width)
        {
            windowWidth = width;
            return this;
        }

        public Text GetText(string moduleName)
        {
            if (!modulesName.ContainsKey(moduleName) || !(modulesName[moduleName] is Text))
                return null;

            return modulesName[moduleName] as Text;
        }

        public bool SetText(string moduleName, string text)
        {
            if (!modulesName.ContainsKey(moduleName) || !(modulesName[moduleName] is Text))
                return false;
            (modulesName[moduleName] as Text).SetText(text);
            return true;
        }

        public Image GetImage(string moduleName)
        {
            if (!modulesName.ContainsKey(moduleName) || !(modulesName[moduleName] is Image))
                return null;

            return modulesName[moduleName] as Image;
        }

        public bool SetImage(string moduleName, SpriteData icon)
        {
            if (!modulesName.ContainsKey(moduleName) || !(modulesName[moduleName] is Image))
                return false;
            (modulesName[moduleName] as Image).SetImage(icon);
            return true;
        }

        public bool SetInteractions(string moduleName, IEnumerable<Entity.InteractionExecuter> interactions, Item item)
        {
            if (!modulesName.ContainsKey(moduleName) || !(modulesName[moduleName] is InteractionList))
                return false;
            (modulesName[moduleName] as InteractionList).SetInteractionList(interactions, item);
            return true;
        }

        public void setVisibility(bool isVisible)
        {
            panel.gameObject.SetActive(isVisible);
        }

        #endregion

        #region management
        public void Update()
        {
            for (int i = 0; i < rowHeight.Count; i++)
                rowHeight[i] = 0;
            foreach (Module m in modulesID.Values)
                m.UpdateSize();
            foreach (Module m in modulesID.Values)
                m.UpdateSize2();
            UpdateWindow();
            foreach (Module m in modulesID.Values)
                m.UpdatePos();
        }

        public void Start(bool autoUpdate = true)
        {
            panel.gameObject.SetActive(true);
            if (autoUpdate)
                CentreBrain.data.updateMethod.Add(id + "_Window", Update);
        }

        public void Cancel()
        {
            CentreBrain.data.updateMethod.Remove(id + "_Window");
            GameObject.Destroy(panel.gameObject);
        }
        #endregion

        #region internal
        internal void SetRowHeight(int row, float height)
        {
            while (rowHeight.Count <= row)
                rowHeight.Add(0);
            rowHeight[row] = height;
        }

        internal float GetColumnWidth(int column)
        {
            if (columnWidth.Count <= column)
                return 0;

            if (columnWidth[column] > 1)
                return columnWidth[column];

            float width = windowWidth - absoluteWidths;
            return width * columnWidth[column];
        }

        internal float GetRowHeight(int row)
        {
            if (rowHeight.Count <= row)
                return 0;

            return rowHeight[row];
        }
        #endregion

        #region private
        private void UpdateWindow()
        {
            float windowHeight = rowHeight.Sum();

            Vector2 pos = this.pos;
            pos = pos.getScreenAddon();

            pos.x += (float)(int)horizontalAlignment * (windowWidth / 2);
            pos.y += (float)(int)verticalAlignment * (windowHeight / 2);
            pos += offset;

            panel.rectTransform.localPosition = pos;

            panel.rectTransform.sizeDelta = new Vector2(windowWidth, windowHeight);

            moduleOffset = new Vector2(-windowWidth / 2, -windowHeight / 2);
        }

        private void loadGO()
        {
            panel = GameObject.Instantiate(
                CentreBrain.data.Prefabs["Window"],
                CentreBrain.data.Folders.DisplayScreen.transform
            ).GetComponent<UnityEngine.UI.Image>();
            panel.name = id;
            panel.gameObject.SetActive(false);
        }
        #endregion
    }
}