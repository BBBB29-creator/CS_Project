using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public static class Shop
    {
        // 상점에서 판매하는 아이템 목록 (기획서 기반)
        public static List<ItemData> ShopItems { get; private set; } = new List<ItemData>();

        // 상점 초기화: 1001번 검을 제외한 12종의 아이템을 진열대에 올립니다.
        public static void InitializeShop()
        {
            ShopItems.Clear();

            // --- [1. 장비류 아이템 (1002 ~ 1005)] ---
            // 생성자 규격 규칙: ID, 타입, 이름, 설명, HP효과타입, HP효과량, 행동비용(MoveCost), 대미지(Damage), 가격(Price/Trophy), 비고
            ShopItems.Add(new ItemData(1002, ItemType.Equipment, "단검", "짧지만 다루기 쉽고 행동이 빨라진다.", HpEffectType.None, 0, 2, 25, 3));
            ShopItems.Add(new ItemData(1003, ItemType.Equipment, "장검", "행동이 조금 느려지지만 더 강한 검.", HpEffectType.None, 0, 6, 50, 4));
            ShopItems.Add(new ItemData(1004, ItemType.Equipment, "대검", "행동이 매우 느리지만 매우 강력한 검.", HpEffectType.None, 0, 8, 90, 10));
            ShopItems.Add(new ItemData(1005, ItemType.Equipment, "다용도 칼", "전투용이 아닌 칼. 스킬, 아이템 효과 상승.", HpEffectType.None, 0, 2, 15, 5, "공격 계열 소모품 피해량 1.1~1.5배"));

            // --- [2. 공격형 소모품 (1006 ~ 1009)] ---
            ShopItems.Add(new ItemData(1006, ItemType.Consumable, "투척검", "짧은 행동으로 피해를 준다.", HpEffectType.None, 0, 1, 20, 1));
            ShopItems.Add(new ItemData(1007, ItemType.Consumable, "투척검2", "기존 투척검 보다 강해진 무기", HpEffectType.None, 0, 1, 30, 2));
            ShopItems.Add(new ItemData(1008, ItemType.Consumable, "폭탄", "다중 피해를 입히는 투척 무기", HpEffectType.None, 0, 3, 30, 4));
            ShopItems.Add(new ItemData(1009, ItemType.Consumable, "개량폭탄", "화력이 강해진 폭탄", HpEffectType.None, 0, 4, 50, 6));

            // --- [3. 회복 및 버프형 소모품 (1010 ~ 1013)] ---
            ShopItems.Add(new ItemData(1010, ItemType.Consumable, "물약", "체력 30을 회복하는 물약", HpEffectType.FixedValue, 30, 1, 0, 1));
            ShopItems.Add(new ItemData(1011, ItemType.Consumable, "중형 물약", "체력 50을 회복하는 물약", HpEffectType.FixedValue, 50, 1, 0, 2));
            ShopItems.Add(new ItemData(1012, ItemType.Consumable, "대형 물약", "체력을 전부 회복하는 물약", HpEffectType.FullRecovery, 0, 8, 0, 6));
            ShopItems.Add(new ItemData(1013, ItemType.Consumable, "신성초", "체력 최대치를 늘려주는 풀", HpEffectType.MaxHpIncrease, 10, 0, 0, 5));
        }

        // 아이템 구매 핵심 로직 (모험가 Player의 Trophy를 기반으로 동작)
        public static void BuyItem(int shopIndex, Player player, Action<string> addLog)
        {
            // 1. 상품 번호 유효성 검사
            if (shopIndex < 0 || shopIndex >= ShopItems.Count)
            {
                addLog("❌ 상점에 존재하지 않는 상품 번호입니다.");
                return;
            }

            ItemData selectedItem = ShopItems[shopIndex];

            // 2. 인벤토리 가방 칸수 제한 체크 (기획: 최대 4칸)
            if (player.MyInventory.IsFull)
            {
                addLog("🎒 가방이 가득 찼습니다! 더 이상 구매할 수 없습니다. (최대 4개 소지)");
                return;
            }

            // 3. 보유 전리품(Trophy) 검사
            if (player.Trophy < selectedItem.Price)
            {
                addLog($" 전리품이 부족합니다! [{selectedItem.Name}] 구매에는 {selectedItem.Price}개의 전리품이 필요합니다. (보유: {player.Trophy}개)");
                return;
            }

            // 4. 구매 성공: 모험가의 Trophy 차감 및 인벤토리 아이템 획득
            player.Trophy -= selectedItem.Price;
            player.MyInventory.AddItem(selectedItem);

            addLog($"🏪 구매 완료! [{selectedItem.Name}]을(를) 획득했습니다. (-{selectedItem.Price} 전리품 / 남은 전리품: {player.Trophy}개)");
        }
    }
}
