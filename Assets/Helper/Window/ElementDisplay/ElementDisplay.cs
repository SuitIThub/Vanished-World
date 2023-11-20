using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using Entity;
using static KeyManager;

public class ElementDisplay<T> where T : class
{
    public class ElementComparer : IComparer<string>
    {
        ElementDisplay<T> display;

        public ElementComparer(ElementDisplay<T> display)
        {
            this.display = display;
        }

        public int Compare(string a, string b)
        {
            if (display.activeElements.ContainsKey(a) && display.activeElements.ContainsKey(b))
            {
                Element first = display.activeElements[a];
                Element second = display.activeElements[b];
                // We can compare both properties.

                int length = first.comparables.Length;
                if (second.comparables.Length > length)
                    length = second.comparables.Length;

                for (int i = 0; i < length; i++)
                {
                    if (first.comparables.Length <= i)
                        return -1;
                    else if (second.comparables.Length <= i)
                        return 1;
                    int compare = first.comparables[i].CompareTo(second.comparables[i]);
                    if (compare != 0)
                        return compare;
                }

                return 0;
            }

            if (!display.activeElements.ContainsKey(a))
            {
                if (!display.activeElements.ContainsKey(b))
                    return 0;
                return -1;
            }

            // Only the second instance is not null, so prefer that.
            return 1;
        }
    }

    public class Element
    {
        public string id { get; }
        public string text;
        public SpriteData icon;
        public Vector2 size;
        public Vector2 pos;
        public Color color;
        public T data;
        public ElementDisplay<T> parent;
        public executeMethod OnExecute;

        public float buffer;

        public Image go;
        public TextMeshProUGUI textMesh => go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        public Image imgLeft => go.transform.GetChild(1).GetComponent<Image>();
        public Image imgRight => go.transform.GetChild(2).GetComponent<Image>();

        internal string[] comparables;

        public Element(ElementDisplay<T> parent, string text, SpriteData icon = null)
        {
            this.parent = parent;
            id = IdUtilities.id;
            this.text = text;
            this.icon = icon;
            size = Vector2.zero;
            pos = Vector2.zero;
            color = Color.white;
            data = default(T);
            OnExecute = null;
            buffer = 0;
            go = null;
            comparables = new string[] { text, id };
        }

        public Element(ElementDisplay<T> parent, T data)
        {
            this.parent = parent;

            if (typeof(T) == typeof(Entity.Core) || typeof(T).IsSubclassOf(typeof(Entity.Core)))
            {
                Entity.Core entity = data as Entity.Core;
                id = entity.id;

                if (data.GetType() == typeof(Entity.Item))
                {
                    Entity.Item item = data as Entity.Item;

                    text = item.displayName;

                    int amount = -1;
                    item.Info().getElement("property.amount", ref amount);

                    if (amount > 0)
                    {
                        if (amount > 1)
                            text = item.displayNamePlural;
                        text += $" (x{amount})";
                    }
                }
                else
                    text = entity.displayName;
                icon = entity.icon;
                this.size = Vector2.zero;
                pos = Vector2.zero;
                color = Color.white;
                this.data = data;
                OnExecute = null;
                buffer = 0;
                go = null;
                comparables = new string[entity.Info().comparables.Length];

                if (entity.Info().comparables.Length != 0)
                    entity.Info().comparables.CopyTo(comparables, 0);
            }
            else
            {
                id = IdUtilities.id;
                this.text = "EMPTY";
                this.icon = null;
                size = Vector2.zero;
                pos = Vector2.zero;
                color = Color.white;
                data = default(T);
                OnExecute = null;
                buffer = 0;
                go = null;
                comparables = new string[] { id };
            }
        }

        #region configuration

        public Element setSize(float x, float y)
        {
            size = new Vector2(x, y);
            return this;
        }

        public Element setSize(Vector2 size)
        {
            this.size = size;
            return this;
        }

        public Element setPos(float x, float y)
        {
            this.pos = new Vector2(x, y);
            return this;
        }

        public Element setPos(Vector2 pos)
        {
            this.pos = pos;
            return this;
        }

        public Element setColor(Color c)
        {
            this.color = c;
            return this;
        }

        public Element setAlpha(float a)
        {
            Color c = color;
            c.a = a;
            color = c;
            return this;
        }

        public Element setData(T data)
        {
            this.data = data;
            return this;
        }

        public Element setMethod(executeMethod method)
        {
            OnExecute = method;
            return this;
        }

        public Element addMethod(executeMethod method)
        {
            OnExecute += method;
            return this;
        }

        public Element setComparables(params string[] comparables)
        {
            this.comparables = comparables;
            return this;
        }

        public void setTransparency(float t)
        {
            Color c1 = go.color;
            c1.a = t / 2f;
            go.color = c1;

            Color c2 = textMesh.color;
            c2.a = t;
            textMesh.color = c2;

            Color c3 = imgLeft.color;
            c3.a = t;
            imgLeft.color = c3;

            Color c4 = imgRight.color;
            c4.a = t;
            imgRight.color = c4;
        }

        #endregion

        #region methods
        internal void OnPointerClick()
        {
            if (parent.clickable)
                jumpTo();
        }

        public void jumpTo()
        {
            parent.scrollToId(id);
        }


        public float getTransparency()
        {
            return textMesh.color.a;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Element))
                return false;

            Element e = obj as Element;
            return e.id == id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum selectType
    {
        all,
        multiple,
        single
    }

    public delegate void executeMethod(T data);
    public delegate void statusMethod(T data, params ReturnCode[] status);

    public string id { get; }

    public selectType type { get; private set; }
    private Alignment.Horizontal alignment;
    private Vector2 _center;
    public Vector2 center
    {
        get => _center.getScreenAddon();
    }
    private int visibleAmount;

    private List<string> chosenElements;
    private List<string> elementOrder;

    private protected Dictionary<string, Element> activeElements;
    private Dictionary<string, Element> movingElements;
    private Dictionary<string, Element> removingElements;

    public int elementAmount { get => elementOrder.Count; }

    public float elementWidth { get; private set; }

    private int cursor;
    public string currentSelected { get; private set; }

    private Color unselectedColor;
    private Color selectedColor;
    private Color markedColor;

    private float ANIMATION_SPEED = 20f;
    private float MESSAGE_HEIGHT   = 20f;
    private float ALIGNMENT_OFFSET = 50f;
    private float MESSAGE_GAP      =  2f;

    public bool clickable { get; private set; }

    private statusMethod OnStatusChanged;

    private bool cancelOnConfirm;

    private Alignment.Horizontal textAlignment = 0;

    public ElementDisplay()
    {
        id = IdUtilities.id;
        alignment = Alignment.Horizontal.center;
        _center = Vector2.zero;

        activeElements = new Dictionary<string, Element>();
        movingElements = new Dictionary<string, Element>();
        removingElements = new Dictionary<string, Element>();

        chosenElements = new List<string>();
        elementOrder = new List<string>();

        unselectedColor = Color.gray;
        selectedColor = Color.white;
        markedColor = Color.yellow;
        cancelOnConfirm = true;

        clickable = false;
    }

    #region configuration

    public ElementDisplay<T> setClickable(bool isClickable)
    {
        clickable = isClickable;
        return this;
    }

    public ElementDisplay<T> setTextAlignment(Alignment.Horizontal type)
    {
        textAlignment = type;
        return this;
    }

    public ElementDisplay<T> setAlignment(Alignment.Horizontal type)
    {
        alignment = type;
        return this;
    }

    public ElementDisplay<T> setOffset(float offset)
    {
        ALIGNMENT_OFFSET = offset;
        return this;
    }

    public ElementDisplay<T> setSelect(selectType type)
    {
        this.type = type;
        return this;
    }

    public ElementDisplay<T> setCenter(Vector2 pos)
    {
        _center = pos;
        return this;
    }

    public ElementDisplay<T> setAmount(int amount)
    {
        visibleAmount = amount;
        return this;
    }

    public ElementDisplay<T> setSelectColor(Color c)
    {
        selectedColor = c;
        return this;
    }

    public ElementDisplay<T> setUnselectColor(Color c)
    {
        unselectedColor = c;
        return this;
    }

    public ElementDisplay<T> setMarkedColor(Color c)
    {
        markedColor = c;
        return this;
    }

    public ElementDisplay<T> setStatusChanged(statusMethod method)
    {
        OnStatusChanged += method;
        return this;
    }

    public ElementDisplay<T> setCancelOnConfirm(bool cancel = true)
    {
        cancelOnConfirm = cancel;
        return this;
    }

    public ElementDisplay<T> setHeight(float height)
    {
        if (height <= 1 && height > 0)
            MESSAGE_HEIGHT = Screen.height * height;
        else
            MESSAGE_HEIGHT = height;
        return this;
    }

    public ElementDisplay<T> Start(bool autoUpdate = true)
    {
        if (autoUpdate)
            CentreBrain.data.updateMethod.Add(id, updateDisplay);

        return this;
    }

    public void Cancel(KVStorage _ = null)
    {
        CentreBrain.data.updateMethod.Remove(id);

        chosenElements.Clear();
        elementOrder.Clear();
        movingElements.Clear();
        removingElements.Clear();

        foreach (Element e in activeElements.Values)
        {
            OnStatusChanged?.Invoke(e.data);

            e.go.gameObject.destroy(true);
        }

        activeElements.Clear();
    }

    public List<T> Confirm(KVStorage _ = null)
    {
        List<T> output = new List<T>();

        if (type == selectType.all)
            chosenElements.AddRange(activeElements.Keys);
        else if (type == selectType.single && activeElements.Count != 0)
            chosenElements.Add(currentSelected);
        else if (type == selectType.multiple && chosenElements.Count == 0 && activeElements.Count == 1)
            chosenElements.Add(currentSelected);

        foreach(string key in chosenElements)
        {
            if (removingElements.ContainsKey(key) || !activeElements.ContainsKey(key))
                continue;
            Element e = activeElements[key];
            e.OnExecute?.Invoke(e.data);
            output.Add(e.data);
        }

        if (cancelOnConfirm)
            Cancel(null);

        return output;
    }

    public T getCurrentSelected()
    {
        if (activeElements.Count == 0)
            return default(T);

        Element e = activeElements[currentSelected];
        return e.data;
    }

    public List<T> getSelectedElements()
    {
        List<T> output = new List<T>();

        if (type == selectType.all)
            chosenElements.AddRange(activeElements.Keys);
        else if (type == selectType.multiple)
        {
            int count = 0;

            foreach (string key in chosenElements)
            {
                if (removingElements.ContainsKey(key) || !activeElements.ContainsKey(key))
                    continue;
                count++;
                Element e = activeElements[key];
                e.OnExecute?.Invoke(e.data);
                output.Add(e.data);
            }

            if (count == 0 && activeElements.ContainsKey(currentSelected))
            {
                Element e = activeElements[currentSelected];
                e.OnExecute?.Invoke(e.data);
                output.Add(e.data);
            }
        }
        else if (activeElements.Count != 0 && !removingElements.ContainsKey(currentSelected))
        {
            Element e = activeElements[currentSelected];
            e.OnExecute?.Invoke(e.data);
            output.Add(e.data);
        }        

        return output;
    }

#endregion

    #region runtime

    public void updateDisplay()
    {
        checkRemoving();
        checkMoving();
    }

    public Element convertToElement<U>(U entity, executeMethod method) where U : IIDBase, T
    {
        Element e;
        if (activeElements.ContainsKey(entity.id))
            e = activeElements[entity.id];
        else
            e = new Element(this, (T)(object)entity);

        e.setMethod(method);

        return e;
    }

    public List<Element> convertToElements<U>(IEnumerable<U> entities, executeMethod method) where U : IIDBase, T
    {
        List<Element> output = new List<Element>();
        foreach (U entity in entities)
            output.Add(convertToElement(entity, method));
        return output;
    }

    public void setElements(IEnumerable<Element> newElements)
    {
        (List<Element>, List<Element>, List<Element>) tupel = activeElements.Values.getCrossSection(newElements);
        List<Element> existing = tupel.Item1;
        List<Element> missing = tupel.Item2;
        List<Element> added = tupel.Item3;

        float width = 0;

        foreach (Element e in existing)
        {
            if (removingElements.ContainsKey(e.id))
            {
                e.setAlpha(1);
                removingElements.Remove(e.id);
                elementOrder.Add(e.id);
                movingElements.Set(e.id, e);
            }

            TextMeshProUGUI tmp = e.go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (tmp.textBounds.size.x > width)
                width = tmp.textBounds.size.x;
        }

        foreach (Element e in added)
        {
            createGameObject(e, out float w);
            if (w > width)
                width = w;

            activeElements.Set(e.id, e);
            movingElements.Set(e.id, e);

            elementOrder.Add(e.id);

            if ((chosenElements.Contains(e.id) || type == selectType.all))
                OnStatusChanged?.Invoke(e.data, ReturnCode.Code(501)); //selected
        }

        if (elementOrder.Count == 1)
        {
            if (chosenElements.Contains(elementOrder[0]) || type == selectType.all)
                OnStatusChanged?.Invoke(activeElements[elementOrder[0]].data, ReturnCode.Code(502), ReturnCode.Code(501)); //marked, selected
            else
                OnStatusChanged?.Invoke(activeElements[elementOrder[0]].data, ReturnCode.Code(502)); //marked
        }

        ElementComparer ec = new ElementComparer(this);
        elementOrder.Sort(ec);

        foreach (Element e in missing)
        {
            e.pos.x = 0;
            e.setAlpha(0);

            chosenElements.Remove(e.id);
            removingElements.Set(e.id, e);
            movingElements.Remove(e.id);
            elementOrder.Remove(e.id);
        }

        recalculateWidth(width);
        updateCursorPos();
        calculatePositions();

        if (elementOrder.Count != 0)
            OnStatusChanged?.Invoke(activeElements[currentSelected].data, ReturnCode.Code(502)); //marked
    }

    public void addElement(Element e)
    {
        if (activeElements.ContainsKey(e.id))
            setElements(activeElements.Values);
        else
        {
            List<Element> elements = new List<Element>(activeElements.Values);
            elements.Add(e);
            setElements(elements);
        }
    }

    public void removeElement(string id)
    {
        if (!activeElements.ContainsKey(id))
            return;

        List<Element> elements = new List<Element>(activeElements.Values);
        elements.Remove(activeElements[id]);
        setElements(elements);
    }

    public void scrollUp(KVStorage _)
    {
        if (elementOrder.Count == 0)
            return;

        cursor--;
        if (cursor < 0)
            cursor = 0;

        if (currentSelected != elementOrder[cursor] && OnStatusChanged != null)
        {
            Element e1 = activeElements[elementOrder[cursor]]; //new Selected
            Element e2 = activeElements[currentSelected]; //old Selected

            ReturnCode[] status1 = (chosenElements.Contains(elementOrder[cursor])) ?
                new ReturnCode[] { ReturnCode.Code(502), ReturnCode.Code(501) } :
                new ReturnCode[] { ReturnCode.Code(502) };
            ReturnCode[] status2 = (chosenElements.Contains(currentSelected)) ?
                new ReturnCode[] { ReturnCode.Code(501) } :
                new ReturnCode[0];

            currentSelected = elementOrder[cursor];
            calculatePositions();

            OnStatusChanged(e1.data, status1);
            OnStatusChanged(e2.data, status2);
            return;
        }

        currentSelected = elementOrder[cursor];
        calculatePositions();
    }

    public void scrollDown(KVStorage _)
    {
        if (elementOrder.Count == 0)
            return;

        cursor++;
        if (cursor >= elementOrder.Count)
            cursor = elementOrder.Count - 1;

        if (currentSelected != elementOrder[cursor] && OnStatusChanged != null)
        {
            Element e1 = activeElements[elementOrder[cursor]];
            Element e2 = activeElements[currentSelected];

            ReturnCode[] status1 = (chosenElements.Contains(elementOrder[cursor])) ?
                new ReturnCode[] { ReturnCode.Code(502), ReturnCode.Code(501) } :
                new ReturnCode[] { ReturnCode.Code(502) };
            ReturnCode[] status2 = (chosenElements.Contains(currentSelected)) ?
                new ReturnCode[] { ReturnCode.Code(501) } :
                new ReturnCode[0];

            currentSelected = elementOrder[cursor];
            calculatePositions();

            OnStatusChanged(e1.data, status1);
            OnStatusChanged(e2.data, status2);
            return;
        }

        currentSelected = elementOrder[cursor];
        calculatePositions();
    }

    public void scrollToId(string id)
    {
        if (!activeElements.ContainsKey(id))
            return;

        if (currentSelected != id && OnStatusChanged != null)
        {
            Element e1 = activeElements[id];
            Element e2 = activeElements[currentSelected];

            ReturnCode[] status1 = (chosenElements.Contains(id)) ?
                new ReturnCode[] { ReturnCode.Code(502), ReturnCode.Code(501) } :
                new ReturnCode[] { ReturnCode.Code(502) };
            ReturnCode[] status2 = (chosenElements.Contains(currentSelected)) ?
                new ReturnCode[] { ReturnCode.Code(501) } :
                new ReturnCode[0];

            currentSelected = id;
            updateCursorPos();
            calculatePositions();

            OnStatusChanged(e1.data, status1);
            OnStatusChanged(e2.data, status2);
            return;
        }

        currentSelected = id;
        updateCursorPos();
        calculatePositions();
    }

    public void selectElement(KVStorage _)
    {
        if (type == selectType.all)
            return;

        Element e = activeElements[currentSelected];

        if (type == selectType.multiple)
        {
            if (chosenElements.Contains(e.id))
            {
                OnStatusChanged?.Invoke(e.data, ReturnCode.Code(502));

                chosenElements.Remove(e.id);
                e.color = new Color(unselectedColor.r, unselectedColor.g, unselectedColor.b);
                movingElements.Set(e.id, e);
            }
            else
            {
                OnStatusChanged?.Invoke(e.data, ReturnCode.Code(502), ReturnCode.Code(501));

                chosenElements.Add(e.id);
                e.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
                movingElements.Set(e.id, e);
            }
        }
        else
        {
            if (chosenElements.Contains(e.id))
                return;

            foreach(string s in chosenElements)
            {
                if (!activeElements.ContainsKey(s))
                    continue;

                Element el = activeElements[s];

                OnStatusChanged?.Invoke(el.data);

                el.color = new Color(unselectedColor.g, unselectedColor.b, unselectedColor.a);
                movingElements.Set(el.id, el);
            }

            OnStatusChanged?.Invoke(e.data, ReturnCode.Code(502), ReturnCode.Code(501));

            chosenElements.Clear();
            chosenElements.Add(e.id);
            e.color = new Color(selectedColor.g, selectedColor.b, selectedColor.a);
            movingElements.Set(e.id, e);
        }
    }

    #endregion

    #region background

    private void checkMoving()
    {
        List<string> keys = movingElements.Keys.ToList();
        foreach(string key in keys)
        {
            Element e = movingElements[key];

            e.go.transform.localPosition = new Vector2(
                Mathf.Lerp(e.go.transform.localPosition.x, e.pos.x, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.transform.localPosition.y, e.pos.y, ANIMATION_SPEED * Time.deltaTime));

            e.go.rectTransform.sizeDelta = new Vector2(
                Mathf.Lerp(e.go.rectTransform.sizeDelta.x, e.size.x, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.rectTransform.sizeDelta.y, e.size.y, ANIMATION_SPEED * Time.deltaTime));

            Color destColor = e.color;

            if (currentSelected == key)
                destColor = markedColor;

            e.go.color = new Color(
                Mathf.Lerp(e.go.color.r, destColor.r, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.color.g, destColor.g, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.color.b, destColor.b, ANIMATION_SPEED * Time.deltaTime));

            e.setTransparency(Mathf.Lerp(e.getTransparency(), e.color.a, ANIMATION_SPEED * Time.deltaTime));

            if (Vector2.Distance(e.go.transform.localPosition, e.pos) < 0.1f &&
                Vector2.Distance(e.go.rectTransform.sizeDelta, e.size) < 0.1f &&
                e.go.color.r.difference(destColor.r) < 0.01f &&
                e.go.color.g.difference(destColor.g) < 0.01f &&
                e.go.color.b.difference(destColor.b) < 0.01f &&
                e.go.color.a.difference(e.color.a) < 0.01f)
            {
                movingElements.Remove(key);
            }
        }
    }

    private void checkRemoving()
    {
        List<string> keys = removingElements.Keys.ToList();
        foreach (string key in keys)
        {
            Element e = removingElements[key];

            e.go.transform.localPosition = new Vector2(
                Mathf.Lerp(e.go.transform.localPosition.x, e.pos.x, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.transform.localPosition.y, e.pos.y, ANIMATION_SPEED * Time.deltaTime));

            e.go.rectTransform.sizeDelta = new Vector2(
                Mathf.Lerp(e.go.rectTransform.sizeDelta.x, e.size.x, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.rectTransform.sizeDelta.y, e.size.y, ANIMATION_SPEED * Time.deltaTime));

            e.go.color = new Color(
                Mathf.Lerp(e.go.color.r, e.color.r, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.color.g, e.color.g, ANIMATION_SPEED * Time.deltaTime),
                Mathf.Lerp(e.go.color.b, e.color.b, ANIMATION_SPEED * Time.deltaTime));

            e.setTransparency(Mathf.Lerp(e.getTransparency(), e.color.a, ANIMATION_SPEED * Time.deltaTime));

            float posDist = Vector2.Distance(e.go.transform.localPosition, e.pos);
            float sizeDist = Vector2.Distance(e.go.rectTransform.sizeDelta, e.size);

            if (posDist < 0.1f &&
                sizeDist < 0.1f &&
                e.go.color.r.difference(e.color.r) < 0.01f &&
                e.go.color.g.difference(e.color.g) < 0.01f &&
                e.go.color.b.difference(e.color.b) < 0.01f &&
                e.go.color.a.difference(e.color.a) < 0.01f)
            {
                removingElements.Remove(key);
                elementOrder.Remove(key);
                activeElements.Remove(key);
                recalculateWidth();
                calculatePositions();

                OnStatusChanged?.Invoke(e.data);

                e.go.gameObject.destroy(true);
            }
        }
    }

    private void createGameObject(Element e, out float width)
    {
        e.go = GameObject.Instantiate(
            CentreBrain.data.Prefabs["Message"],
            Vector2.zero,
            Quaternion.identity,
            CentreBrain.data.Folders.ElementDisplay.transform
        ).GetComponent<Image>();

        e.go.name = e.id;

        ElementDisplayButton edb = e.go.GetComponent<ElementDisplayButton>();
        edb.method = e.OnPointerClick;

        e.textMesh.rectTransform.sizeDelta = new Vector2(10000, MESSAGE_HEIGHT);

        e.textMesh.SetText(e.text);
        e.textMesh.ForceMeshUpdate();

        e.textMesh.alignment = (textAlignment == Enums.Alignment.Horizontal.center) ? TextAlignmentOptions.Center : 
                               (textAlignment == Enums.Alignment.Horizontal.right) ? TextAlignmentOptions.Right : 
                               TextAlignmentOptions.Left;
        

        e.imgLeft.rectTransform.sizeDelta = Vector2.zero;
        float imageSize = 0;
        if (e.icon != null)
        {
            e.imgLeft.rectTransform.sizeDelta = e.icon.normalizeSize(MESSAGE_HEIGHT);
            e.imgLeft.sprite = e.icon.sprite;
            imageSize = MESSAGE_HEIGHT;
        }

        e.textMesh.rectTransform.sizeDelta = new Vector2(e.textMesh.textBounds.size.x, MESSAGE_HEIGHT);

        width = e.textMesh.textBounds.size.x + imageSize + MESSAGE_HEIGHT / 2;

        if (chosenElements.Contains(e.id) || type == selectType.all)
            e.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, 0);
        else
            e.color = new Color(unselectedColor.r, unselectedColor.g, unselectedColor.b, 0);

        e.setSize(width, MESSAGE_HEIGHT);
        e.setPos(Vector2.negativeInfinity);

        e.setTransparency(0);

        e.go.rectTransform.sizeDelta = new Vector2(width, MESSAGE_HEIGHT);
        e.imgLeft.rectTransform.localPosition = new Vector2(-width / 2 + imageSize / 2, 0);
        e.textMesh.rectTransform.localPosition = new Vector2(imageSize / 2, 0);
    }

    public void updateAllText()
    {
        foreach(Element e in activeElements.Values)
            updateText(e);
        recalculateWidth();
    }

    private void updateText(Element e)
    {
        if (e.data.GetType() == typeof(Entity.Item))
        {
            Entity.Item item = e.data as Entity.Item;

            string text = item.displayName;

            int amount = -1;
            item.Info().getElement("property.amount", ref amount);

            if (amount > 0)
            {
                if (amount > 1)
                    text = item.displayNamePlural;
                text += $" (x{amount})";
            }

            e.textMesh.SetText(text);
        }
    }

    private void calculatePositions()
    {
        for (int i = 0; i < elementOrder.Count; i++)
        {
            int ePos = i - cursor;
            string key = elementOrder[i];
            Element e = activeElements[key];

            if (e.go.rectTransform.sizeDelta.x.difference(elementWidth) >= 0.1f)
            {
                e.setSize(elementWidth, e.go.rectTransform.sizeDelta.y);
                movingElements.Set(key, e);
            }

            Vector2 pos = new Vector2(
                (int)alignment * (ALIGNMENT_OFFSET + elementWidth / 2),
                ePos * (MESSAGE_HEIGHT + MESSAGE_GAP)) + center;
            if (Vector2.Distance(pos, e.go.rectTransform.localPosition) >= 0.1f)
            {
                if (e.pos.x == float.NegativeInfinity)
                    e.go.rectTransform.localPosition = new Vector2(0, pos.y) + center;

                e.setPos(pos);
                movingElements.Set(key, e);
            }

            float a = 1;
            if (ePos == visibleAmount || ePos == -visibleAmount)
                a = 0.5f;

            if (ePos > visibleAmount || ePos < -visibleAmount)
                a = 0;

            if (e.color.a != a)
            {
                e.color.a = a;
                movingElements.Set(key, e);
            }
        }
    }

    private void recalculateWidth(float maxWidth = 0)
    {
        foreach(Element e in activeElements.Values)
        {
            if (removingElements.ContainsKey(e.id))
                continue;

            float width = e.textMesh.textBounds.size.x + MESSAGE_HEIGHT / 2;

            if (e.icon != null)
                width += MESSAGE_HEIGHT;

            if (width > maxWidth)
                maxWidth = width;
        }

        elementWidth = maxWidth;

        foreach(Element e in activeElements.Values)
        {
            if (e.go.rectTransform.sizeDelta.x.difference(elementWidth) >= 0.1f)
            {
                e.setSize(elementWidth, e.go.rectTransform.sizeDelta.y);
                movingElements.Set(e.id, e);
            }
        }
    }

    private void updateCursorPos()
    {
        if (elementOrder.Contains(currentSelected))
            cursor = elementOrder.IndexOf(currentSelected);
        else if (elementOrder.Count > 0)
        {
            if (cursor >= elementOrder.Count)
                cursor = elementOrder.Count - 1;


            if (currentSelected != elementOrder[cursor] && OnStatusChanged != null)
            {
                Element e1 = activeElements[elementOrder[cursor]];
                ReturnCode[] status1 = (chosenElements.Contains(elementOrder[cursor])) ?
                    new ReturnCode[] { ReturnCode.Code(502), ReturnCode.Code(501) } :
                    new ReturnCode[] { ReturnCode.Code(502) };
                OnStatusChanged(e1.data, status1);

                if (currentSelected != null && activeElements.ContainsKey(currentSelected))
                {
                    Element e2 = activeElements[currentSelected];
                    ReturnCode[] status2 = (chosenElements.Contains(currentSelected)) ?
                        new ReturnCode[] { ReturnCode.Code(501) } :
                        new ReturnCode[0];
                    OnStatusChanged(e2.data, status2);
                }
            }

            currentSelected = elementOrder[cursor];
        }
    }

    #endregion
}