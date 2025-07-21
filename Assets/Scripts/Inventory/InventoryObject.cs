using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory", order = 0)]
    public class InventoryObject : ScriptableObject{
        public List<InventorySlot> listItens = new ();
        public void AddItem(DefaultObject item, int amount = 1){
            var hasItem = false;
            foreach (var t in listItens.Where(t => t.item == item))
            {
                t.AddAmount(amount);
                hasItem = true;
                break;
            }
            if(!hasItem){
                listItens.Add(new InventorySlot(item, amount));
            }
            //string json = JsonUtility.ToJson(Inventory);
            //Debug.Log(json);
        }
        public void RemoveItem(DefaultObject item, int amount = 1){
            var hasItem = false;
            var indexItem = -1;
            for (var i = 0; i < listItens.Count; i++)
            {
                if (listItens[i].item != item) continue;
                indexItem = i;
                hasItem = listItens[i].RemoveAmount(amount);
            }
            if(!hasItem){
                listItens.RemoveAt(indexItem);
            }
        }
        public bool CheckItem(DefaultObject item)
        {
            return listItens.Any(t => t.item == item);
        }
    }
    [System.Serializable]
    public class InventorySlot{
        public DefaultObject item;
        public int amount;
        public InventorySlot(DefaultObject currentItem, int currentAmount = 1){
            item = currentItem;
            amount = currentAmount;
        }
        public void AddAmount(int value = 1){
            amount += value;
        }
        public bool RemoveAmount(int value = 1){
            var hasItem = true;
            if(amount > 0){
                amount -= value;
            }
            if(amount <= 0){
                hasItem = false;
            }
            return hasItem;
        }
    }
}