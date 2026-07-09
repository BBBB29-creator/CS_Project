using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public class InputManager
    {
        public GameState CurrentState { get; set; } = GameState.Battle;
        private Player _player;

        // UI와 Program에서 실시간 동기화할 퍼블릭 변수
        public bool IsTargetingMode { get; set; } = false;

        public InputManager(Player player)
        {
            _player = player;
        }

        public void HandleGlobalInput(char key, List<Character> allCharacters, Action<string> addLog)
        {
            char lowerKey = char.ToLower(key);

            // [분기 1. 인벤토리 창 처리]
            if (CurrentState == GameState.Inventory)
            {
                if (lowerKey == 'q')
                {
                    CurrentState = GameState.Battle;
                    addLog("⚔️ 인벤토리를 닫았습니다. 시간이 다시 흐릅니다.");
                    return;
                }

                if (lowerKey >= '1' && lowerKey <= '4')
                {
                    int slotIndex = lowerKey - '1';
                    if (_player.UseItem(slotIndex, allCharacters, addLog, out int moveCost))
                    {
                        _player.AtbGauge -= (moveCost * 10);
                        if (_player.AtbGauge < 0) _player.AtbGauge = 0;

                        addLog($"⏳ 행동 비용 {moveCost} 지불! (남은 게이지: {Math.Round(_player.AtbGauge)}%)");
                        CurrentState = GameState.Battle;
                    }
                }
                return;
            }

            // [분기 2. 선택 대기 창 처리]
            if (CurrentState == GameState.SelectNext)
            {
                if (lowerKey == '1')
                {
                    CurrentState = GameState.Shop;
                    addLog("🏪 상점에 입장했습니다. 아이템 알파벳(A~L)을 눌러 구매하세요! (나가기: Q)");
                }
                else if (lowerKey == '2')
                {
                    WaveManager.SpawnRandomMonsters(allCharacters);
                    _player.AtbGauge = 50;
                    CurrentState = GameState.Battle;
                    addLog("⚔️ 새로운 적들이 전장에 난입했습니다! 난타전 시작!");
                }
                return;
            }

            // [분기 3. 상점 창 처리]
            if (CurrentState == GameState.Shop)
            {
                if (lowerKey == 'q')
                {
                    CurrentState = GameState.SelectNext;
                    addLog("↩️ 상점에서 나왔습니다. 다음 행동을 선택하세요.");
                    return;
                }

                if (lowerKey >= 'a' && lowerKey <= 'l')
                {
                    int shopIndex = lowerKey - 'a';
                    Shop.BuyItem(shopIndex, _player, addLog);
                }
                return;
            }

            // [분기 4. 일반 전투 상태 처리]
            if (_player.Hp <= 0) return;

            // 💡 [핵심 연동]: 현재 무기에 따른 동적 ATB 소모 비용 계산!
            // 기본 무기(맨손/기본검)의 비용은 기획서 사양인 4칸(40%)입니다.
            double attackCost = 40.0;

            // 만약 대검이나 단검을 장착했다면, 그 무기가 요구하는 MoveCost * 10 만큼 비용이 무거워지거나 가벼워집니다!
            if (_player.EquippedWeapon != null)
            {
                attackCost = _player.EquippedWeapon.MoveCost * 10.0; // 대검은 8칸이므로 80% 소모! 단검은 2칸이므로 20% 소모!
            }

            // 4-A. 타겟 조준 모드일 때
            if (IsTargetingMode)
            {
                var monsters = allCharacters.FindAll(c => !c.IsPlayer);

                if (lowerKey >= '1' && lowerKey <= '9')
                {
                    int monsterIndex = lowerKey - '1';

                    if (monsterIndex >= 0 && monsterIndex < monsters.Count && monsters[monsterIndex].Hp > 0)
                    {
                        Character target = monsters[monsterIndex];

                        // 공격 실행
                        _player.ExecuteAttack(target, addLog);

                        // 💡 [버그 해결]: 고정 40%를 깎는 게 아니라, 장비한 무기 고유의 attackCost만큼 깎습니다!
                        _player.AtbGauge -= attackCost;
                        if (_player.AtbGauge < 0) _player.AtbGauge = 0;

                        addLog($"⚔️ [{target.Name}] 저격 완료! 무기 행동 비용 {attackCost / 10:F0}칸 소모! (남은 게이지: {Math.Round(_player.AtbGauge)}%)");
                        IsTargetingMode = false;
                    }
                    else
                    {
                        addLog("❌ 사망했거나 올바르지 않은 적 번호입니다.");
                    }
                }
                else if (lowerKey == 'q')
                {
                    IsTargetingMode = false;
                    addLog("↩️ 공격을 취소했습니다.");
                }
                return;
            }

            // 4-B. 일반 평화 상태일 때
            if (lowerKey == '2')
            {
                CurrentState = GameState.Inventory;
                addLog("🎒 인벤토리를 열었습니다. (시간 일시정지)");
                return;
            }

            if (lowerKey == '1')
            {
                // 💡 [버그 해결]: 공격 선언 단계에서도 고정 40%가 아닌, 무기 고유의 attackCost만큼 차 있는지 검사합니다!
                if (_player.AtbGauge < attackCost)
                {
                    addLog($"⚠️ 게이지 부족! 현재 무기 공격에는 최소 {attackCost / 10:F0}칸({attackCost}%)이 필요합니다. (현재: {Math.Round(_player.AtbGauge)}%)");
                    return;
                }

                IsTargetingMode = true;
                addLog("🎯 공격할 적의 번호([1번], [2번]...)를 입력하세요! (취소: Q)");
            }
        }
    }
}
