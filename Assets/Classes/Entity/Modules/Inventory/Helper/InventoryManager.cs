using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ElementDisplay<Entity.Item>;

namespace Entity.Inventory
{
    public class InventoryManager
    {
        public bool isActive { get; private set; }

        private Core inventory;

        private ElementDisplay<Item> display;

        private Item displayedItem = null;

        private DisplayScreen.Window window;

        public InventoryManager(Core inventory)
        {
            isActive = false;
            display = new ElementDisplay<Item>()
                .setAlignment(Enums.Alignment.Horizontal.left)
                .setAmount((int)(Screen.height / 100))
                .setCenter(new Vector2(float.PositiveInfinity, 0))
                .setSelect(selectType.single)
                .setHeight(50)
                .setTextAlignment(Enums.Alignment.Horizontal.right)
                .setClickable(true)
                .setStatusChanged(Update)
                .setCancelOnConfirm(false);

            this.inventory = inventory;
        }

        public void pingUpdate()
        {
            updateWindowData();
            display.updateAllText();
        }

        public void Update(Item data, params ReturnCode[] code)
        {

            if (Array.Exists(code, returnCode => returnCode == 405))
                display.addElement(display.convertToElement(data, null));
            if (Array.Exists(code, returnCode => returnCode == 406))
                display.removeElement(data.id);

            if (data.id == display.currentSelected)
            {
                displayedItem = data;
                updateWindowData();
            }

            window.setVisibility(display.elementAmount != 0);
        }

        private void updateWindowData()
        {
            window.SetImage("Icon", displayedItem.icon);
            window.SetText("Title", displayedItem.displayName);
            window.SetText("SubTitle", "Subtitle");
            window.SetText("Description", displayedItem.Info().description);
            window.SetText("UID", displayedItem.UniqueID.ToString());

            if (displayedItem.Interact(out Interaction.Core interact))
            {
                List<InteractionExecuter> executers = new List<InteractionExecuter>();

                if (inventory.parent.Interact(out Interaction.Core playerInt))
                    executers.AddRange(playerInt.interactions);
                executers.AddRange(interact.interactions);

                window.SetInteractions("Interactions", executers, displayedItem);
            }

            window.SetOffset(new Vector2(-60 - display.elementWidth, 0));

            window.Update();
        }

        public void Start(bool autoUpdate = true)
        {
            if (isActive)
                return;

            isActive = true;

            window = new DisplayScreen.Window()
                .SetCenter(new Vector2(float.PositiveInfinity, float.PositiveInfinity))
                .SetWindowWidth(500)
                .SetHorizontalAlignment(Enums.Alignment.Horizontal.left)
                .SetVerticalAlignment(Enums.Alignment.Vertical.down)
                .SetColWidth(0, 0.3f)
                .SetColWidth(1, 0.7f);

            DisplayScreen.Image icon = (DisplayScreen.Image)
                new DisplayScreen.Image(window, "Icon")
                    .SetRowPos(0)
                    .SetColPos(0)
                    .SetBorder(10, 10, 0, 5)
                    .SetRowAmount(2)
                    .SetColAmount(1);
            DisplayScreen.Text title = (DisplayScreen.Text)
                new DisplayScreen.Text(window, "Title")
                    .SetFontSize(40)
                    .SetRowPos(0)
                    .SetColPos(1)
                    .SetBorder(5, 10, 10, 5)
                    .SetRowAmount(1)
                    .SetColAmount(1);
            DisplayScreen.Text subTitle = (DisplayScreen.Text)
                new DisplayScreen.Text(window, "SubTitle")
                    .SetFontSize(35)
                    .SetRowPos(1)
                    .SetColPos(1)
                    .SetBorder(5, 10, 10, 5)
                    .SetRowAmount(1)
                    .SetColAmount(1);
            DisplayScreen.Text description = (DisplayScreen.Text)
                new DisplayScreen.Text(window, "Description")
                    .SetFontSize(28)
                    .SetWrapping(true)
                    .SetRowPos(2)
                    .SetColPos(1)
                    .SetBorder(10, 5, 10, 10)
                    .SetRowAmount(1)
                    .SetColAmount(1);
            DisplayScreen.Text UID = (DisplayScreen.Text)
                new DisplayScreen.Text(window, "UID")
                    .SetFontSize(28)
                    .SetWrapping(true)
                    .SetRowPos(3)
                    .SetColPos(1)
                    .SetBorder(10, 5, 10, 10)
                    .SetRowAmount(1)
                    .SetColAmount(1);
            DisplayScreen.InteractionList interactTags = (DisplayScreen.InteractionList)
                new DisplayScreen.InteractionList(window, "Interactions")
                    .SetFontSize(28)
                    .SetRowPos(2)
                    .SetColPos(0)
                    .SetBorder(10, 5, 10, 10)
                    .SetRowAmount(2)
                    .SetColAmount(1);

            window.addModule(icon);
            window.addModule(title);
            window.addModule(subTitle);
            window.addModule(description);
            window.addModule(UID);
            window.addModule(interactTags);

            window.Start();
            display.Start();

            registerKeys();

            inventory.statusChanged += Update;
            display.setElements(display.convertToElements(inventory.items, null));
        }

        private void registerKeys()
        {
            CentreBrain.keyManager.Add("Inventory_MoveUp",
                new KeyManager.Scroll(KeyManager.KeyBase.KeyDir.down, null, display.scrollUp)
            );
            CentreBrain.keyManager.Add("Inventory_MoveDown",
                new KeyManager.Scroll(KeyManager.KeyBase.KeyDir.up, null, display.scrollDown)
            );
            CentreBrain.keyManager.Add("Inventory_Confirm",
                new KeyManager.Mouse(0, KeyManager.KeyBase.KeyDir.down, null, Confirm)
            );
            CentreBrain.keyManager.Add("Inventory_Cancel",
                new KeyManager.Key(KeyCode.Escape, KeyManager.KeyBase.KeyDir.down, null, Cancel)
            );
        }

        public void Confirm(KVStorage _)
        {
            display.Confirm();
        }

        public void Cancel(KVStorage _)
        {
            display.Cancel();
            window.Cancel();

            isActive = false;

            CentreBrain.keyManager.Remove("Inventory_MoveUp");
            CentreBrain.keyManager.Remove("Inventory_MoveDown");
            CentreBrain.keyManager.Remove("Inventory_Confirm");
            CentreBrain.keyManager.Remove("Inventory_Cancel");

            CentreBrain.data.updateMethod.Remove("InventoryManager");

            inventory.statusChanged -= Update;
        }

        public void test(Item data)
        {
            Debug.Log(data.UniqueID);
        }
    }
}
