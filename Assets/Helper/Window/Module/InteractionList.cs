using Entity;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DisplayScreen
{
    public class InteractionList : Module
    {
        public class Element
        {
            public string text;
            public RectTransform tag;
            public TextMeshProUGUI textMesh;
            public Button button;
            public float fontSize;
            public float textLength;
            public InteractionExecuter executer;
        }

        private GridLayoutGroup grid;
        private Dictionary<string, Element> elements;

        private float fontSize = 18;

        private Item item;

        public InteractionList(Window parent, string name) : base(parent, name)
        {
            elements = new Dictionary<string, Element>();
        }

        public InteractionList SetInteractionList(IEnumerable<InteractionExecuter> interactions, Item item)
        {
            this.item = item;

            List<string> keys = interactions.Select(interact => interact.name).ToList();

            foreach (InteractionExecuter interact in interactions.Where(i => !elements.ContainsKey(i.name)))
                addTag(interact);

            List<string> oldKeys = elements.Keys.Except(keys).ToList();
            foreach (string s in oldKeys)
                removeTag(s);

            return this;
        }

        public InteractionList SetFontSize(float size)
        {
            this.fontSize = size;
            return this;
        }

        protected internal override void loadGO()
        {
            rect = GameObject.Instantiate(
                CentreBrain.data.Prefabs["WindowModuleInteraction"],
                parent.panel.transform).GetComponent<RectTransform>();

            rect.name = name;
            grid = rect.GetComponent<GridLayoutGroup>();
        }

        private void addTag(InteractionExecuter interaction)
        {
            if (elements.ContainsKey(interaction.name))
                return;

            Element e = new Element();

            e.executer = interaction;

            e.tag = GameObject.Instantiate(
                CentreBrain.data.Prefabs["WindowModuleInteractionTag"],
                rect.transform).GetComponent<RectTransform>();

            e.textMesh = e.tag.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            e.text = interaction.name;
            e.textMesh.text = interaction.name;
            e.fontSize = 0;
            e.textLength = 0;

            e.button = e.tag.GetComponent<Button>();
            e.button.onClick.AddListener(() => OnClick(interaction));

            elements.Add(interaction.name, e);

            updateGrid();
        }

        private void OnClick(InteractionExecuter interaction)
        {
            if (CentreBrain.data.player.Interact(out Entity.Interaction.Core interact))
            {
                Entity.Interaction.Player player = interact as Entity.Interaction.Player;

                if (!InteractionExecuter.isRunning)
                {
                    interaction.cancelMethod = () => { InteractionExecuter.isRunning = false; };
                    KVStorage data = new KVStorage("item", item);

                    if (interaction.GetType() == typeof(InteractionExecuterViewScan))
                    {
                        InteractionExecuterViewScan ievs = interaction as InteractionExecuterViewScan;
                        Vector2 scale = Vector2.one;
                        item.Info().getElement("materialProperty.scale", ref scale);

                        ievs.setScanRadius(scale.x * 0.75f);
                    }
                    interaction.Trigger(data, item, player.parent, item.Info().data);
                    InteractionExecuter.isRunning = true;
                }
            }
        }

        private void removeTag(string s)
        {
            if (!elements.ContainsKey(s))
                return;

            Element e = elements[s];
            GameObject.Destroy(e.tag.gameObject);
            elements.Remove(s);

            updateGrid();
        }

        private void updateGrid()
        {
            float maxSize = -1;
            float maxHeight = -1;
            foreach(string s in elements.Keys)
            {
                Element e = elements[s];
                if (e.fontSize != fontSize)
                {
                    e.textMesh.fontSize = fontSize;
                    e.fontSize = fontSize;
                    e.textMesh.ForceMeshUpdate();
                }
                e.textLength = e.textMesh.textBounds.size.x + 10;
                if (maxSize < e.textLength)
                    maxSize = e.textLength;
                if (maxHeight < e.textMesh.textBounds.size.y)
                    maxHeight = e.textMesh.textBounds.size.y;
            }

            if (maxSize != -1 && maxHeight != -1)
                grid.cellSize = new Vector2(maxSize, maxHeight);
        }

        internal override void UpdateSize()
        {
            float width = GetWidth() - border.x - border.z;

            //todo: calculate Height
            updateGrid();

            int cellAmount = (int)(parent.windowWidth / grid.cellSize.x);

            int rows = elements.Count / cellAmount + 1;

            size = new Vector2(width, (float)rows * grid.cellSize.y);

            float h = 0;
            if (parent.rowHeight.Count > rowPos)
                h = parent.rowHeight[rowPos];

            if (rowAmount == 1 && h < size.y)
                parent.SetRowHeight(rowPos, size.y + border.y + border.w);

            rect.sizeDelta = size;
            
            foreach(string s in elements.Keys)
            {
                Element e = elements[s];
                e.textMesh.rectTransform.sizeDelta = grid.cellSize;
            }
        }
    }
}