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
            // -------------------------------------------------------------
            // 분기 1. [인벤토리 창이 열려있을 때]
            // -------------------------------------------------------------
            if (CurrentState == GameState.Inventory)
            {
                // 💡 [오류 해결]: 이제 인벤토리 안에서 '2'번은 대형 물약 사용에 양보하고, 
                // 오직 'Q' 키로만 가방을 닫을 수 있게 수정하여 단축키 충돌을 해결했습니다!
                if (key == 'q')
                {
                    CurrentState = GameState.Battle;
                    addLog("⚔️ 인벤토리를 닫았습니다. 시간이 다시 흐릅니다.");
                    return;
                }

                // 가방 안의 아이템 선택 (1~4번은 이제 온전히 아이템만 사용합니다)
                if (key >= '1' && key <= '4')
                {
                    int slotIndex = key - '1';
                    if (_player.UseItem(slotIndex, allCharacters, addLog, out int moveCost))
                    {
                        // 기획서 반영: 행동비용 차감
                        _player.AtbGauge -= (moveCost * 10);
                        if (_player.AtbGauge < 0) _player.AtbGauge = 0;

                        addLog($"⏳ 행동 비용 {moveCost} 지불! (남은 게이지: {Math.Round(_player.AtbGauge)}%)");
                        CurrentState = GameState.Battle; // 사용 성공 시 전투 복귀
                    }
                }
                return;
            }

            // -------------------------------------------------------------
            // 분기 2. [전투 승리 후 선택 대기 창일 때]
            // -------------------------------------------------------------
            if (CurrentState == GameState.SelectNext)
            {
                if (key == '1')
                {
                    CurrentState = GameState.Shop;
                    addLog("🏪 상점에 입장했습니다. (나가기: Q)");
                }
                else if (key == '2')
                {
                    WaveManager.SpawnRandomMonsters(allCharacters);
                    _player.AtbGauge = 50;
                    CurrentState = GameState.Battle;
                    addLog("⚔️ 새로운 적들이 전장에 난입했습니다!");
                }
                return;
            }

            // -------------------------------------------------------------
            // 분기 3. [상점 창이 열려있을 때]
            // -------------------------------------------------------------
            if (CurrentState == GameState.Shop)
            {
                if (key == 'q')
                {
                    CurrentState = GameState.SelectNext;
                    addLog("↩️ 상점에서 나왔습니다.");
                }
                return;
            }

            // -------------------------------------------------------------
            // 분기 4. [일반 전투 상태일 때]
            // -------------------------------------------------------------
            if (_player.Hp <= 0) return;

            // 4-A. 타겟 조준 모드일 때
            if (IsTargetingMode)
            {
                var monsters = allCharacters.FindAll(c => !c.IsPlayer);

                if (key >= '1' && key <= '9')
                {
                    int monsterIndex = key - '1';

                    if (monsterIndex >= 0 && monsterIndex < monsters.Count && monsters[monsterIndex].Hp > 0)
                    {
                        Character target = monsters[monsterIndex];

                        _player.ExecuteAttack(target, addLog);
                        _player.AtbGauge -= 40.0; // 4칸 소모

                        addLog($"⚔️ [{target.Name}] 조준 격파! 비용 4칸 소모! (남은 게이지: {Math.Round(_player.AtbGauge)}%)");
                        IsTargetingMode = false;
                    }
                    else
                    {
                        addLog("❌ 사망했거나 올바르지 않은 적 번호입니다.");
                    }
                }
                else if (key == 'q')
                {
                    IsTargetingMode = false;
                    addLog("↩️ 공격을 취소했습니다.");
                }
                return;
            }

            // 4-B. 일반 평화 상태일 때
            if (key == '2') // 2번 누르면 가방 오픈
            {
                CurrentState = GameState.Inventory;
                addLog("🎒 인벤토리를 열었습니다. (시간 일시정지)");
                return;
            }

            if (key == '1') // 1번 누르면 조준 모드 진입
            {
                if (_player.AtbGauge < 40.0)
                {
                    addLog($"⚠️ 게이지 부족! 최소 4칸(40%)이 필요합니다. (현재: {Math.Round(_player.AtbGauge)}%)");
                    return;
                }

                IsTargetingMode = true;
                addLog("🎯 공격할 적의 번호([1번], [2번]...)를 입력하세요! (취소: Q)");
            }
        }
    }
}
