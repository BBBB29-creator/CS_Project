using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public class Inventory
    {
        // 💡 최대 소지 개수를 4개로 고정!
        private const int MAX_SLOTS = 4;

        // 실제 아이템들을 담아둘 리스트
        public List<ItemData> Items { get; private set; } = new List<ItemData>();

        // 가방이 꽉 찼는지 확인하는 프로퍼티 (상점 등에서 체크용)
        public bool IsFull => Items.Count >= MAX_SLOTS;

        // 현재 가방에 들어있는 아이템 개수
        public int CurrentCount => Items.Count;

        // 아이템 획득 (상점에서 사거나 던전에서 얻을 때)
        public bool AddItem(ItemData item)
        {
            if (IsFull)
            {
                Program.AddLog("🎒 인벤토리가 가득 차서 아이템을 더 넣을 수 없습니다! (최대 4개)");
                return false; // 추가 실패
            }

            Items.Add(item);
            Program.AddLog($"📥 [{item.Name}]을(를) 가방에 넣었습니다. ({Items.Count}/{MAX_SLOTS})");
            return true; // 추가 성공
        }

        // 아이템 사용 또는 제거 (전투 중 사용하거나 상점에 팔 때)
        public bool RemoveItem(ItemData item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
                return true;
            }
            return false;
        }
    }
}