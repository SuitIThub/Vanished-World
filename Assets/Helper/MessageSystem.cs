using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageSystem
{
    public class Message
    {
        public GameObject gameObject;
        public string parentId { get; }
        public float time;

        public string text { get; }

        public SpriteData leftIcon;
        public SpriteData rightIcon;

        public Image go;
        public TextMeshProUGUI textMesh => go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        public Image imgLeft => go.transform.GetChild(1).GetComponent<Image>();
        public Image imgRight => go.transform.GetChild(2).GetComponent<Image>();

        public Vector2 position;

        private Vector2 oldPos;

        public Message(GameObject parent, float time, string text, SpriteData left, SpriteData right, Vector2 pos)             
        {
            gameObject = parent;
            parentId = parent.name;
            this.time = time;
            this.text = text;
            leftIcon = left;
            rightIcon = right;
            go = null;
            position = pos;
            oldPos = gameObject.transform.position;
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

        public float getTransparency()
        {
            return textMesh.color.a;
        }

        public void setGoPosX(float x)
        {
            Vector3 pos = go.rectTransform.localPosition;
            pos.x = x;
            go.rectTransform.localPosition = pos;
        }

        public void setGoPosY(float y)
        {
            Vector3 pos = go.rectTransform.localPosition;
            pos.y = y;
            go.rectTransform.localPosition = pos;
        }

        public void setGoPos(float x, float y)
        {
            setGoPosX(x);
            setGoPosY(y);
        }

        public void setGoPos(Vector2 pos)
        {
            setGoPosX(pos.x);
            setGoPosY(pos.y);
        }

        public Vector2 getGoPos()
        {
            return go.rectTransform.localPosition;
        }

        public void updatePos()
        {
            Vector2 pos = (gameObject != null) ? gameObject.transform.position : oldPos;
            Vector3 diff = oldPos.toVec3().delta(pos) * 100;

            position += diff.toVec2();
            go.rectTransform.localPosition += diff;

            oldPos = pos;
        }
    }

    private const float ANIMATION_SPEED = 5f;
    private const float DEFAULT_TIME = 10f;
    private const float MESSAGE_HEIGHT = 50f;

    private Dictionary<string, List<Message>> pendingMessages;
    private Dictionary<string, List<Message>> activeMessages;

    private List<Message> addMessage;
    private List<Message> moveMessage;
    private List<Message> removeMessage;

    public MessageSystem()
    {
        pendingMessages = new Dictionary<string, List<Message>>();
        activeMessages = new Dictionary<string, List<Message>>();

        addMessage = new List<Message>();
        moveMessage = new List<Message>();
        removeMessage = new List<Message>();
    }

    public void Update()
    {
        checkPending();

        checkActive();

        checkAdding();
        checkMoving();
        checkRemoving();
    }


    public void destroyMessagesByID(string id)
    {
        pendingMessages.Remove(id);

        if (activeMessages.ContainsKey(id))
        {
            foreach(Message m in activeMessages[id])
            {
                m.time = float.MaxValue;

                m.position.y -= MESSAGE_HEIGHT * 1.1f;

                
                addMessage.Remove(m);
                moveMessage.Remove(m);
                removeMessage.Add(m);
            }

            activeMessages.Remove(id);
        }
    }

    public void sendMessage(GameObject parent, string text, float time = -1)
    {
        if (time < 0)
            time = DEFAULT_TIME;

        Vector2 pos = parent.transform.position * 100;
        pos.y += 130f;

        Message message = new Message(parent, time, text, null, null, pos);

        if (!pendingMessages.ContainsKey(parent.name))
            pendingMessages.Add(parent.name, new List<Message>());
        pendingMessages[parent.name].Add(message);
    }

    public void sendMessage(GameObject parent, string text, SpriteData icon, bool isLeft = false, float time = -1)
    {
        if (time < 0)
            time = DEFAULT_TIME;

        Vector2 pos = parent.transform.position * 100;
        pos.y += 130f;

        Message message = new Message(parent, time, text, (isLeft ? icon : null), (isLeft ? null : icon), pos);

        if (!pendingMessages.ContainsKey(parent.name))
            pendingMessages.Add(parent.name, new List<Message>());
        pendingMessages[parent.name].Add(message);
    }

    public void sendMessage(GameObject parent, string text, SpriteData left, SpriteData right, float time = -1)
    {
        if (time < 0)
            time = DEFAULT_TIME;

        Vector2 pos = parent.transform.position * 100;
        pos.y += 130f;

        Message message = new Message(parent, time, text, left, right, pos);

        if (!pendingMessages.ContainsKey(parent.name))
            pendingMessages.Add(parent.name, new List<Message>());
        pendingMessages[parent.name].Add(message);
    }

    private void checkActive()
    {
        List<string> keys = new List<string>(activeMessages.Keys);
        foreach(string key in keys)
        {
            for (int i = activeMessages[key].Count - 1; i >= 0; i--)
            {
                Message m = activeMessages[key][i];

                m.updatePos();

                m.time -= Time.deltaTime;
                if (m.time <= 0)
                {
                    for (int j = i + 1; j < activeMessages[key].Count; j++)
                    {
                        Message m2 = activeMessages[key][j];

                        m2.position.y -= MESSAGE_HEIGHT * 1.1f;
                        moveMessage.Add(m2);
                    }

                    m.time = float.MaxValue;

                    m.position.y -= MESSAGE_HEIGHT * 1.1f;

                    activeMessages[key].Remove(m);
                    if (activeMessages[key].Count == 0)
                        activeMessages.Remove(key);

                    moveMessage.Remove(m);
                    removeMessage.Add(m);
                }
            }

        }
    }

    private void checkRemoving()
    {
        for (int i = removeMessage.Count - 1; i >= 0; i--)
        {
            Message m = removeMessage[i];
            moveMessage.Remove(m);

            m.updatePos();

            Vector3 pos = m.go.transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, m.position.y, ANIMATION_SPEED * Time.deltaTime);
            m.go.transform.localPosition = pos;

            m.setTransparency(Mathf.Lerp(m.getTransparency(), 0, ANIMATION_SPEED * 2 * Time.deltaTime));

            if (pos.y.difference(m.position.y) < 0.01f)
            {
                addMessage.Remove(m);
                removeMessage.Remove(m);
                moveMessage.Remove(m);
                if (pendingMessages.ContainsKey(m.parentId))
                {
                    pendingMessages[m.parentId].Remove(m);

                    if (pendingMessages[m.parentId].Count == 0)
                        pendingMessages.Remove(m.parentId);
                }

                GameObject.Destroy(m.go.gameObject);
            }
        }
    }

    private void checkMoving()
    {
        for (int i = moveMessage.Count - 1; i >= 0; i--)
        {
            Message m = moveMessage[i];

            if (m.go == null)
            {
                moveMessage.RemoveAt(i);
                continue;
            }

            Vector3 pos = m.go.transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, m.position.y, ANIMATION_SPEED * Time.deltaTime);
            m.go.transform.localPosition = pos;

            if (pos.y.difference(m.position.y) < 0.01f)
                moveMessage.Remove(m);
        }
    }

    private void checkAdding()
    {
        for (int i = addMessage.Count - 1; i >= 0; i--)
        {
            Message m = addMessage[i];

            Vector3 pos = m.go.transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, m.position.y, ANIMATION_SPEED * Time.deltaTime);
            m.go.transform.localPosition = pos;

            m.setTransparency(Mathf.Lerp(m.getTransparency(), 1, ANIMATION_SPEED * Time.deltaTime));

            if (pos.y.difference(m.position.y) < 0.01f)
                addMessage.Remove(m);
        }
    }

    private void checkPending()
    {
        List<string> keys = new List<string>(pendingMessages.Keys);
        foreach (string key in keys)
        {
            if (activeMessages.ContainsKey(key) && activeMessages[key].Count >= 10)
                continue;

            Message m = pendingMessages[key].First();

            if (!activeMessages.ContainsKey(key))
                activeMessages.Add(key, new List<Message>());

            activeMessages[key].Add(m);

            loadMessageGO(m);
        }
    }

    private void loadMessageGO(Message m)
    {
        float amount = (float)activeMessages[m.parentId].Count;

        float offset = (amount - 1) * MESSAGE_HEIGHT * 1.1f;

        m.position.y += offset;


        //create GameObject at position above parent
        m.go = GameObject.Instantiate(
                CentreBrain.data.Prefabs["Message"],
                Vector3.zero,
                Quaternion.identity,
                CentreBrain.data.Folders.Message.transform).GetComponent<Image>();

        m.setTransparency(0);

        Vector3 v = m.textMesh.rectTransform.position;
        v.y = MESSAGE_HEIGHT;
        m.textMesh.rectTransform.position = v;

        m.textMesh.rectTransform.sizeDelta = new Vector2(10000, MESSAGE_HEIGHT);

        m.textMesh.SetText(m.text);
        m.textMesh.ForceMeshUpdate();


        float width = m.textMesh.textBounds.size.x + MESSAGE_HEIGHT / 2;
        float pos = 0;
        float leftWidth = 0;
        float rightWidth = 0;


        if (m.leftIcon != null)
        {
            leftWidth = MESSAGE_HEIGHT;
            pos += MESSAGE_HEIGHT / 2;

            m.imgLeft.sprite = m.leftIcon.sprite;
            m.imgLeft.rectTransform.sizeDelta = m.leftIcon.normalizeSize(MESSAGE_HEIGHT);
        }
        else
        {
            m.imgLeft.rectTransform.localPosition = new Vector2(0, 0);
            m.imgLeft.rectTransform.sizeDelta = new Vector2(0, 0);
            m.imgLeft.sprite = null;
        }

        if (m.rightIcon != null)
        {
            rightWidth = MESSAGE_HEIGHT;
            pos -= MESSAGE_HEIGHT / 2;

            m.imgRight.sprite = m.rightIcon.sprite;
            m.imgRight.rectTransform.sizeDelta = m.rightIcon.normalizeSize(MESSAGE_HEIGHT);
        }
        else
        {
            m.imgRight.rectTransform.sizeDelta = new Vector2(0, 0);
            m.imgRight.rectTransform.localPosition = new Vector2(0, 0);
            m.imgRight.sprite =  null;
        }

        if (m.leftIcon != null)
            m.imgLeft.rectTransform.localPosition = new Vector2(-MESSAGE_HEIGHT - 20, 0);
        if (m.rightIcon != null)
            m.imgRight.rectTransform.localPosition = new Vector2(MESSAGE_HEIGHT + 20, 0);


        m.textMesh.rectTransform.sizeDelta = new Vector2(width + leftWidth + rightWidth, MESSAGE_HEIGHT);
        m.textMesh.rectTransform.localPosition = new Vector2(pos, 0);

        m.go.rectTransform.sizeDelta = new Vector2(width + leftWidth + rightWidth, MESSAGE_HEIGHT);
        Vector2 rectPos = m.position;
        rectPos.y += MESSAGE_HEIGHT * 1.1f;

        m.go.rectTransform.localPosition = rectPos;

        pendingMessages[m.parentId].Remove(m);
        if (pendingMessages[m.parentId].Count == 0)
            pendingMessages.Remove(m.parentId);

        addMessage.Add(m);
    }
}
