using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static ViewScanner;

namespace Entity
{
    public class InteractionExecuterViewScan : InteractionExecuter
    {
        public static class Options
        {
            public readonly static Dictionary<string, RuntimeMethod> options = new()
            {
                {"isITEM",      isITEM},
                {"isNPC",       isNPC},
                {"isOBJ",       isOBJ},
                {"notITEM",     notITEM},
                {"notNPC",      notNPC},
                {"notOBJ",      notOBJ},
                {"isStackable", isStackable},
                {"isFunnel",    isFunnel }
            };

            public static bool isITEM(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2, List<Core> _3, Core focus, Vector2 _4)
            {
                bool isItem = (focus != null) ? focus.objectType == "ITEM" : false;
                executer.voteColor((isItem) ? 1 : 0);
                return isItem;
            }

            public static bool isNPC(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2, List<Core> _3, Core focus, Vector2 _4)
            {
                bool isNpc = (focus != null) ? focus.objectType == "NPC" : false;
                executer.voteColor((isNpc) ? 1 : 0);
                return isNpc;
            }

            public static bool isOBJ(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2, List<Core> _3, Core focus, Vector2 _4)
            {
                bool isOBJ = (focus != null) ? focus.objectType == "OBJ" : false;
                executer.voteColor((isOBJ) ? 1 : 0);
                return isOBJ;
            }

            public static bool notITEM(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2, List<Core> _3, Core focus, Vector2 _4)
            {
                bool notItem = (focus != null) ? focus.objectType != "ITEM" : true;
                executer.voteColor((notItem) ? 1 : 0);
                return notItem;
            }

            public static bool notNPC(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2, List<Core> _3, Core focus, Vector2 _4)
            {
                bool notNpc = (focus != null) ? focus.objectType != "NPC" : true;
                executer.voteColor((notNpc) ? 1 : 0);
                return notNpc;
            }

            public static bool notOBJ(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2, List<Core> _3, Core focus, Vector2 _4)
            {
                bool notObj = (focus != null) ? focus.objectType != "OBJ" : true;
                executer.voteColor((notObj) ? 1 : 0);
                return notObj;
            }

            public static bool isStackable(InteractionExecuterViewScan executer, ViewScanner scanner, List<Core> filteredEntities,
                                           List<Core> unfilteredEntities, Core focus, Vector2 _1)
            {
                if (filteredEntities.Count == 0)
                {
                    if (unfilteredEntities.Count != 0)
                    {
                        executer.voteColor(0);
                        return false;
                    }
                    else
                    {
                        executer.voteColor(1);
                        return true;
                    }
                }
                else
                {
                    if (focus == null)
                    {
                        executer.voteColor(0);
                        return false;
                    }

                    Entity.Core entity = focus;

                    Item itemGround = null;
                    if (!scanner.data.getElement("item", ref itemGround) || entity.GetType() != typeof(Item))
                    {
                        executer.voteColor(0);
                        return false;
                    }

                    Item item = entity as Item;

                    ReturnCode mergeCode = item.canBeMergedWith(itemGround);

                    if (mergeCode == 401)
                    {
                        executer.voteColor(0);
                        return false;
                    }
                    else if (mergeCode == 402)
                    {
                        executer.voteColor(2);
                        return true;
                    }
                    else
                    {
                        executer.voteColor(1);
                        return true;
                    }
                }
            }

            public static bool isFunnel(InteractionExecuterViewScan executer, ViewScanner _1, List<Core> _2,
                                        List<Core> unfilteredEntities, Core focus, Vector2 _3)
            {
                if (focus != null)
                {
                    bool isFunnel = true;
                    focus.Info().getElement("isFunnel", ref isFunnel);

                    if (!focus.Inventory(out _))
                        isFunnel = false;

                    if (isFunnel)
                    {
                        executer.voteColor(2);
                        return true;
                    }
                    else
                    {
                        executer.voteColor(0);
                        return false;
                    }
                }
                else if (unfilteredEntities.Count != 0)
                {
                    executer.voteColor(0);
                    return false;
                }
                else
                {
                    executer.voteColor(1);
                    return true;
                }
            }
        }

        public ViewScanner scanner;
        public MeshGraph backgroundGraph;
        public MeshGraph foregroundGraph;
        public ElementDisplay<Core> display;

        public Core focus;

        private GameObject focusCenter;

        private Dictionary<int, List<string>> options = new Dictionary<int, List<string>>();

        private int newColor = 0;

        public delegate bool RuntimeMethod(
            InteractionExecuterViewScan executer,
            ViewScanner scanner,
            List<Core> filteredEntities,
            List<Core> unfileredEntities,
            Core focus,
            Vector2 pos);

        public InteractionExecuterViewScan(KVStorage json, KVStorage data, KVStorage replaceData,
                                           string separator = "{}") :
            base(json, data, replaceData, separator)
        {
            data.getElement("center", ref focusCenter);

            KVStorage scan = null;
            if (json.getElement("data", ref scan, replaceData, separator))
                loadData(scan, replaceData, separator);

            return;
        }

        public void setScanRadius(float radius)
        {
            if (foregroundGraph.GetType() == typeof(MeshGraph.Circle))
                foregroundGraph.setRadius(radius);
            scanner.setScanRadius(radius);
        }

        public override void Trigger(KVStorage data, Core origin, Core sender = null, KVStorage replaceData = null)
        {
            if (sender == null)
                sender = origin;

            interactionData = new InteractionData<Core>(origin, sender, interactionKey, data, this);

            if (display == null)
            {
                executeInteractions(interactionData);
                return;
            }

            scanner.data = data;

            scanner.executer = this;

            scanner.setIgnore(focusCenter.name)
                .Start();

            if (backgroundGraph != null)
                backgroundGraph.Draw(false);
            if (foregroundGraph != null)
                foregroundGraph.Draw(false);
            display.Start(false);

            registerKeys();
        }

        #region Runtime

        private protected override void updateInteraction(ViewScanner scanner, List<Core> filteredEntities,
                                                          List<Core> unfilteredEntities, Vector2 pos)
        {
            Vector2 center = focusCenter.transform.position;

            backgroundGraph.setCenter(center);
            if (scanner.GetType() == typeof(PointScanner))
                foregroundGraph.setCenter(pos);
            else
                foregroundGraph.setCenter(center);
            scanner.setCenter(center);

            float degree = center.toDegree(pos);

            if (foregroundGraph != null)
            {
                foregroundGraph.setAngle(degree);
                foregroundGraph.updateGraph();
            }
            if (backgroundGraph != null)
            {
                backgroundGraph.setAngle(degree);
                backgroundGraph.updateGraph();
            }

            if (degree > 90 && degree < 270)
                display.setAlignment(Enums.Alignment.Horizontal.right);
            else
                display.setAlignment(Enums.Alignment.Horizontal.left);

            display.setElements(display.convertToElements(filteredEntities, null));
            display.updateDisplay();

            focus = display.getCurrentSelected();

            updateMethod?.Invoke();

            executePossible = true;

            newColor = 0;

            foreach (List<string> optionsList in options.Values)
            {
                bool result = false;
                foreach (string option in optionsList)
                {
                    if (Options.options[option](this, scanner, filteredEntities, unfilteredEntities, focus, pos))
                    {
                        result = true;
                        break;
                    }
                }

                if (!result)
                {
                    executePossible = false;
                    break;
                }
            }

            confirmColor();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color">
        ///     <list type="table">
        ///         <item>
        ///             <term>0</term>
        ///             <description>red</description>
        ///         </item>
        ///         <item>
        ///             <term>1</term>
        ///             <description>green</description>
        ///         </item>
        ///         <item>
        ///             <term>2</term>
        ///             <description>yellow</description>
        ///         </item>
        ///     </list>
        /// </param>
        public void voteColor(int color)
        {
            if (color > newColor)
                newColor = color;
        }

        public void confirmColor()
        {
            Color color = newColor switch
            {
                1 => new Color(0, 1, 0, 0.5f), //green
                2 => new Color(1, 0.92f, 0.016f, 0.5f), //yellow
                _ => new Color(1, 0, 0, 0.5f) // red
            };

            if (foregroundGraph != null)
                foregroundGraph.setColor(color);
        }

        private protected override void cancelInteraction(KVStorage _ = null)
        {
            scanner.cancelScan();
            display.Cancel();
            foregroundGraph.Stop();
            backgroundGraph.Stop();

            base.cancelInteraction(_);
        }

        private protected override void confirmInteraction(ViewScanner _, List<Core> _1, List<Core> _2, Vector2 pos)
        {
            if (!executePossible)
                return;

            interactionData.data.Set("position", pos);
            interactionData.data.Set("entities", display.getSelectedElements());
            interactionData.data.Set("focus", display.getCurrentSelected() as Core);

            if (scanner.stopAfterConfirm)
                cancelInteraction();

            base.confirmInteraction(_, _1, _2, pos);
        }


        private protected override void registerKeys()
        {
            CentreBrain.keyManager.Add(id + "_Interaction_MoveUp",
                new KeyManager.Scroll(KeyManager.KeyBase.KeyDir.down, null, display.scrollUp)
            );
            CentreBrain.keyManager.Add(id + "_Interaction_MoveDown",
                new KeyManager.Scroll(KeyManager.KeyBase.KeyDir.up, null, display.scrollDown)
            );
            if (display.type == ElementDisplay<Core>.selectType.multiple)
            {
                CentreBrain.keyManager.Add(id + "_Interaction_Select",
                    new KeyManager.Key(KeyCode.Space, KeyManager.KeyBase.KeyDir.down, null, display.selectElement)
                );
            }
            CentreBrain.keyManager.Add(id + "_Interaction_Confirm",
                new KeyManager.Mouse(0, KeyManager.KeyBase.KeyDir.down, null, scanner.confirmScan)
            );
            CentreBrain.keyManager.Add(id + "_Interaction_Cancel",
                new KeyManager.Key(KeyCode.Escape, KeyManager.KeyBase.KeyDir.down, null, cancelInteraction)
            );
        }

        private protected override void loadData(KVStorage data, KVStorage replaceData = null, string separator = "{}")
        {
            string areaType = "";
            if (!data.getElement("areaType", ref areaType, replaceData, separator))
                return;

            data = data.replaceData(replaceData, separator);

            if (areaType == "cone")
                loadConeData(data, replaceData, separator);
            else if (areaType == "point")
                loadPointData(data, replaceData, separator);
            else if (areaType == "line")
                loadLineData(data, replaceData, separator);
            else if (areaType == "circle")
                loadCircleData(data, replaceData, separator);

            scanner.setRuntimeMethod(updateInteraction)
                .setConfirmMethod(confirmInteraction);

            display = new ElementDisplay<Core>()
                .setCenter(Vector2.zero)
                .setHeight(30)
                .setAmount(4)
                .setAlignment(Enums.Alignment.Horizontal.left)
                .setTextAlignment(Enums.Alignment.Horizontal.right)
                .setStatusChanged(updateOutline);

            ElementDisplay<Core>.selectType selectType = ElementDisplay<Core>.selectType.all;
            data.getEnum("scanType", ref selectType, replaceData, separator);
            display.setSelect(selectType);



            bool oneTimeUse = false;
            if (data.getElement("oneTimeUse", ref oneTimeUse, replaceData, separator))
                scanner.setCancelOnConfirm(oneTimeUse);

            backgroundGraph.setColor(new Color(0.5f, 0.5f, 0.5f, 0.2f));
            backgroundGraph.setEdges(180);
            if (foregroundGraph != null)
            {
                foregroundGraph.setColor(new Color(0f, 0f, 1f, 0.2f));
            }

            List<object> optionsList = null;
            if (data.getElement("options", ref optionsList, replaceData, separator))
            {
                options = new Dictionary<int, List<string>>();
                Dictionary<int, List<RuntimeMethod>> scanOptions = new Dictionary<int, List<RuntimeMethod>>();

                foreach (string s in optionsList.OfType<string>())
                {
                    string[] sSplit = s.Split(':');
                    if (sSplit.Length != 2)
                        continue;
                    if (sSplit[0].ToInt(out int group))
                    {
                        if (!options.ContainsKey(group))
                            options.Add(group, new List<string>());
                        options[group].Add(sSplit[1]);

                        if (Options.options.ContainsKey(sSplit[1]))
                        {
                            RuntimeMethod option = Options.options[sSplit[1]];

                            if (!scanOptions.ContainsKey(group))
                                scanOptions.Add(group, new List<RuntimeMethod>());
                            scanOptions[group].Add(option);
                        }
                    }
                }

                scanner.setOptions(scanOptions);
            }
        }

        #region loadData-Subclasses

        private void loadConeData(KVStorage scan, KVStorage replaceData, string separator)
        {
            float degree = 45;
            scan.getElement("degree", ref degree, replaceData, separator);

            scanner = new ConeScanner(degree);

            backgroundGraph = new MeshGraph.Circle();

            foregroundGraph = new MeshGraph.Cone(degree);
            foregroundGraph.setEdges((int)degree / 2);

            float radius = 0;
            if (scan.getElement("scanDistance", ref radius, replaceData, separator))
            {
                scanner.setRadius(radius);
                backgroundGraph.setRadius(radius);
                foregroundGraph.setRadius(radius);
            }
        }

        private void loadPointData(KVStorage scan, KVStorage replaceData, string separator)
        {
            float pointRadius = 1;
            scan.getElement("radius", ref pointRadius, replaceData, separator);

            scanner = new PointScanner(pointRadius);
            backgroundGraph = new MeshGraph.Circle();

            foregroundGraph = new MeshGraph.Circle();
            foregroundGraph.setRadius(pointRadius);
            foregroundGraph.setEdges((int)pointRadius * 20);

            float radius = 0;
            if (scan.getElement("scanDistance", ref radius, replaceData, separator))
            {
                scanner.setRadius(radius);
                backgroundGraph.setRadius(radius);
            }
        }

        private void loadLineData(KVStorage scan, KVStorage replaceData, string separator)
        {
            scanner = new LineScanner();

            backgroundGraph = new MeshGraph.Circle();

            foregroundGraph = new MeshGraph.Cone(2f);
            foregroundGraph.setEdges(2);

            float radius = 0;
            if (scan.getElement("scanDistance", ref radius, replaceData, separator))
            {
                scanner.setRadius(radius);
                backgroundGraph.setRadius(radius);
                foregroundGraph.setRadius(radius);
            }
        }

        private void loadCircleData(KVStorage scan, KVStorage replaceData, string separator)
        {
            scanner = new CircleScanner();

            backgroundGraph = new MeshGraph.Circle();

            foregroundGraph = null;

            float radius = 0;
            if (scan.getElement("scanDistance", ref radius, replaceData, separator))
            {
                scanner.setRadius(radius);
                backgroundGraph.setRadius(radius);
            }
        }

        #endregion

        public void updateOutline(Core data, params ReturnCode[] statuses)
        {
            if (data is Entity.Core)
            {
                Entity.Core entity = data as Entity.Core;

                if (entity.isDestroyed || entity.gameObject == null)
                    return;

                if (statuses.Length == 0)
                    entity.setOutline();
                else
                {

                    if (Array.Exists(statuses, x => x == 501))
                        entity.setOutline(2);
                    if (Array.Exists(statuses, x => x == 502))
                        entity.setOutline(0);
                }
            }
        }

        #endregion
    }
}