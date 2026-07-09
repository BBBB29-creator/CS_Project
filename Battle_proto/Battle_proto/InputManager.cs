using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public class InputManager
    {
        // 현재 게임의 상태를 기억 (기본값은 전투)
        public GameState CurrentState { get; set; } = GameState.Battle;

        private Player _player;

        // 💡 플레이어가 공격 버튼(1)을 눌러 적 번호를 입력 대기 중인지 기억하는 변수
        private bool _isTargetingMode = false;

        public InputManager(Player player)
        {
            _player = player;
        }

        // 메인 루프에서 키 입력을 받으면 이 메서드를 호출합니다.
        public void HandleGlobalInput(char key, List<Character> allCharacters, Action<string> addLog)
        {
            // -------------------------------------------------------------
            // 분기 1. [인벤토리 창이 열려있을 때] 키 입력 처리 (우선순위 1등)
            // -------------------------------------------------------------
            if (CurrentState == GameState.Inventory)
            {
                if (key == '2' || key == 'q')
                {
                    CurrentState = GameState.Battle;
                    addLog("⚔️ 인벤토리를 닫았습니다. 시간이 다시 흐릅니다.");
                    return;
                }

                if (key >= '1' && key <= '4')
                {
                    int slotIndex = key - '1';
                    if (_player.UseItem(slotIndex, allCharacters, addLog, out int moveCost))
                    {
                        _player.AtbGauge -= (moveCost * 10);
                        if (_player.AtbGauge < 0) _player.AtbGauge = 0;

                        addLog($"⏳ 행동 비용 {moveCost} 지불! (남은 ATB 게이지: {Math.Round(_player.AtbGauge)}%)");
                        CurrentState = GameState.Battle;
                    }
                }
                return;
            }

            // -------------------------------------------------------------
            // 분기 2. 💡 [전투 승리 후 선택 대기 창일 때] 키 입력 처리
            // -------------------------------------------------------------
            if (CurrentState == GameState.SelectNext)
            {
                if (key == '1') // 상점 입장
                {
                    CurrentState = GameState.Shop;
                    addLog("🏪 상점에 입장했습니다. 아이템 번호(1~3)를 눌러 구매하세요. (나가기: Q)");
                }
                else if (key == '2') // 다음 전투 시작
                {
                    WaveManager.SpawnRandomMonsters(allCharacters); // 무작위 2마리 스폰
                    _player.AtbGauge = 50; // 긴장감을 위해 용사 게이지는 절반만 주고 시작
                    CurrentState = GameState.Battle;
                    addLog("⚔️ 새로운 적들이 전장에 난입했습니다! 난타전 시작!");
                }
                return;
            }

            // -------------------------------------------------------------
            // 분기 3. 💡 [상점 창이 열려있을 때] 키 입력 처리
            // -------------------------------------------------------------
            if (CurrentState == GameState.Shop)
            {
                if (key == 'q') // 상점 탈출 -> 다시 선택 대기 창으로 복귀
                {
                    CurrentState = GameState.SelectNext;
                    addLog("↩️ 상점에서 나왔습니다. 다음 행동을 선택하세요.");
                }

                // TODO: 상점에서 1, 2, 3번 키로 물건 사는 구체적인 로직은 이곳에 작성될 예정입니다.
                return;
            }

            // -------------------------------------------------------------
            // 분기 4. [일반 전투 상태일 때] 키 입력 처리
            // -------------------------------------------------------------
            if (_player.Hp <= 0) return;

            // 💡 4-A. 타겟팅 공격 모드일 때 (1번을 이미 누르고 적 번호를 고르는 중)
            if (_isTargetingMode)
            {
                var monsters = allCharacters.FindAll(c => !c.IsPlayer);

                if (key >= '1' && key <= '9')
                {
                    int monsterIndex = key - '1';

                    // 살아있고 유효한 몬스터 번호를 선택했는지 확인
                    if (monsterIndex >= 0 && monsterIndex < monsters.Count && monsters[monsterIndex].Hp > 0)
                    {
                        Character target = monsters[monsterIndex];

                        // 공격 실행 및 행동비용 4칸(40%) 차감
                        _player.ExecuteAttack(target, addLog);
                        _player.AtbGauge -= 40.0;

                        addLog($"⚔️ [{target.Name}] 집중 저격! 행동 비용 4칸 소모! (남은 게이지: {Math.Round(_player.AtbGauge)}%)");
                        _isTargetingMode = false; // 공격 성공 후 타겟 모드 종료
                    }
                    else
                    {
                        addLog("❌ 올바르지 않거나 이미 사망한 적의 번호입니다. 다시 선택하세요.");
                    }
                }
                else if (key == 'q') // Q 누르면 공격 선언 취소
                {
                    _isTargetingMode = false;
                    addLog("↩️ 공격을 취소했습니다.");
                }
                return;
            }

            // 💡 4-B. 일반 평화로운(?) 실시간 전투 조작 대기 상태
            if (key == '2') // 2번 누르면 인벤토리 토글 오픈
            {
                CurrentState = GameState.Inventory;
                addLog("🎒 인벤토리를 열었습니다. (시간 일시정지)");
                return;
            }

            if (key == '1') // 1번 누르면 공격 타겟팅 진입
            {
                if (_player.AtbGauge < 40.0)
                {
                    addLog($"⚠️ 게이지 부족! 일반 공격에는 최소 4칸(40%)이 필요합니다. (현재: {Math.Round(_player.AtbGauge)}%)");
                    return;
                }

                _isTargetingMode = true;
                addLog("🎯 공격할 적의 번호([1번], [2번]...)를 입력하세요! (취소: Q)");
            }
        }
    }
}
