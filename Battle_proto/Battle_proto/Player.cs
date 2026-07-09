using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public class Player : Character
    {
        public Inventory MyInventory { get; private set; } = new Inventory();

        // 보유 전리품 수치
        public int Trophy { get; set; } = 15;

        // 💡 [추가] 현재 장착 중인 무기 데이터를 보관하는 변수 (기본값은 null = 맨손/기본검 상태)
        public ItemData EquippedWeapon { get; private set; } = null;

        // 💡 [스탯 동적 연산 프로퍼티 기본 오버라이딩]
        // 무기를 장착하면 기본 공격력(20)에 무기 고유 대미지가 더해집니다!
        public new int Damage
        {
            get
            {
                int baseDamage = 20; // 모험가 기본 공격력
                if (EquippedWeapon != null) baseDamage += EquippedWeapon.Damage; // 무기 대미지 합산
                return baseDamage;
            }
        }

        // 💡 [스탯 동적 연산 프로퍼티 기본 오버라이딩]
        // 기획서 사양 반영: 장비의 MoveCost가 높을수록(대검=8, 장검=6) 행동이 느려집니다!
        // 여기서는 기본 속도(50)에서 무기의 MoveCost만큼 속도를 차감하여 게이지가 더 느리게 차도록 설계합니다.
        public new int Speed
        {
            get
            {
                int baseSpeed = 50; // 모험가 기본 속도
                if (EquippedWeapon != null)
                {
                    // 무기 행동비용(칸수)이 높을수록 속도가 무거워집니다. (예: 대검 장착 시 50 - (8 * 3) = 26 속도로 둔중해짐)
                    baseSpeed -= (EquippedWeapon.MoveCost * 3);
                }
                return Math.Max(10, baseSpeed); // 최소 속도 보장 안전장치
            }
        }

        public Player(string name, int hp, int speed, double crit, double dodge)
            : base(name, hp, speed, true, crit, dodge, 20)
        {
        }

        public void ExecuteAttack(Character target, Action<string> addLog)
        {
            // BattleCore에 장착된 무기의 동적 Damage 스탯이 실시간 배달됩니다!
            string log = BattleCore.ProcessAttack(this, target);
            addLog(log);
        }

        // 💡 [무기 장착 시스템 완벽 반영 버전 UseItem]
        public bool UseItem(int slotIndex, List<Character> allCharacters, Action<string> addLog, out int moveCost)
        {
            moveCost = 0;

            if (slotIndex < 0 || slotIndex >= MyInventory.Items.Count)
            {
                addLog("❌ 해당 슬롯에 아이템이 없습니다.");
                return false;
            }

            ItemData item = MyInventory.Items[slotIndex];
            moveCost = item.MoveCost;

            // 1. 게이지 부족 검사 (무기 장착은 행동 비용을 소모하지 않으므로 Consumable 일 때만 체크)
            if (item.Type == ItemType.Consumable)
            {
                double requiredAtb = item.MoveCost * 10.0;
                if (this.AtbGauge < requiredAtb)
                {
                    addLog($"⚠️ 게이지 부족! [{item.Name}] 사용에는 최소 {item.MoveCost}칸({requiredAtb}%)이 필요합니다. (현재: {Math.Round(this.AtbGauge)}%)");
                    return false;
                }
            }

            // --- [효과 적용 분기 처리] ---

            // 💡 [핵심 구현] A. 장비 아이템(무기)일 때 -> 장착 프로세스 가동!
            if (item.Type == ItemType.Equipment)
            {
                // 이미 무기를 장착하고 있었다면, 기존 무기를 다시 가방으로 돌려보냅니다 (탈착)
                if (EquippedWeapon != null)
                {
                    MyInventory.AddItem(EquippedWeapon);
                    addLog($"🔄 기존에 장착 중이던 [{EquippedWeapon.Name}]을(를) 해제하여 가방에 넣었습니다.");
                }

                // 새로운 무기 장착 및 가방에서 제거
                EquippedWeapon = item;
                MyInventory.RemoveItem(item);

                // 💡 장착은 턴 비용을 쓰지 않으므로 moveCost를 0으로 리셋하여 게이지가 깎이지 않게 방어합니다!
                moveCost = 0;

                addLog($"⚔️ [{item.Name}]을(를) 성공적으로 장착했습니다! (공격력: {this.Damage} / 속도: {this.Speed})");
                return true;
            }

            // B. 소모품 아이템일 때 (기존 연동 구조 완벽 유지)
            if (item.Type == ItemType.Consumable)
            {
                // 공격형 투척/폭탄 소모품
                if (item.HpType == HpEffectType.None && item.Damage > 0)
                {
                    var aliveMonsters = allCharacters.FindAll(c => !c.IsPlayer && c.Hp > 0);
                    if (aliveMonsters.Count == 0)
                    {
                        addLog("⚠️ 공격할 수 있는 살아있는 적이 없습니다.");
                        return false;
                    }

                    if (item.Id == 1008 || item.Id == 1009) // 폭탄
                    {
                        addLog($"💣 [{this.Name}]이(가) 범위 투척 무기 [{item.Name}]을(를) 던졌습니다!");
                        foreach (var monster in aliveMonsters)
                        {
                            monster.Hp = Math.Max(0, monster.Hp - item.Damage);
                            addLog($"💥 {monster.Name}에게 {item.Damage}의 광역 범위 피해! (HP: {monster.Hp}/{monster.MaxHp})");
                            if (monster.Hp <= 0) addLog($"💀 [{monster.Name}]이(가) 폭사에 휘말려 쓰러졌습니다!");
                        }
                    }
                    else if (item.Id == 1006 || item.Id == 1007) // 투척검
                    {
                        Character singleTarget = aliveMonsters[0];
                        singleTarget.Hp = Math.Max(0, singleTarget.Hp - item.Damage);
                        addLog($"🎯 [{this.Name}]의 [{item.Name}] 신속 투척! -> {singleTarget.Name}에게 {item.Damage}의 암기 피해! (HP: {singleTarget.Hp}/{singleTarget.MaxHp})");
                        if (singleTarget.Hp <= 0) addLog($"💀 [{singleTarget.Name}]이(가) 쓰러졌습니다!");
                    }

                    MyInventory.RemoveItem(item);
                    return true;
                }

                // 회복 및 버프초 계열
                switch (item.HpType)
                {
                    case HpEffectType.FixedValue:
                        this.Hp = Math.Min(this.MaxHp, this.Hp + item.HpValue);
                        addLog($"🧪 [{item.Name}] 복용! HP를 {item.HpValue} 회복했습니다. (현재 HP: {this.Hp}/{this.MaxHp})");
                        break;

                    case HpEffectType.FullRecovery:
                        this.Hp = this.MaxHp;
                        addLog($"🧪 [{item.Name}] 복용! 모험가의 체력이 전부(100%) 회복되었습니다! (현재 HP: {this.Hp}/{this.MaxHp})");
                        break;

                    case HpEffectType.MaxHpIncrease:
                        this.MaxHp += item.HpValue;
                        this.Hp += item.HpValue;
                        addLog($"🌱 [{item.Name}] 복용! 영구적으로 최대 체력이 {item.HpValue} 상승했습니다! (최대 HP: {this.MaxHp})");
                        break;
                }

                MyInventory.RemoveItem(item);
                return true;
            }

            return false;
        }
    }
}
