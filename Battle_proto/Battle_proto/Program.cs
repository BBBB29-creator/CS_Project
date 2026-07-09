using System;
using System.Collections.Generic;
using System.Threading;

namespace RealtimeAtbRpg
{
    class Program
    {
        private static Queue<string> _battleLogs = new Queue<string>();
        private static bool _isBattleOver = false;
        private static string _battleResultText = "";

        // 💡 [화면 고정의 핵심]: 이전 프레임의 게임 상태를 기억하여, 
        // 화면 상태가 바뀔 때만 강제로 Console.Clear()를 시켜주는 감시 변수입니다.
        private static GameState _lastState = GameState.Battle;

        public static void AddLog(string message)
        {
            _battleLogs.Enqueue(message);
            if (_battleLogs.Count > 5) _battleLogs.Dequeue();
        }

        public static void ClearBattleLogs()
        {
            _battleLogs.Clear();
        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Clear();

            List<Character> activeCharacters = new List<Character>();
            Player player = new Player("모험가", 100, 50, 0.1, 0.0);

            ItemData hpPotion = new ItemData(1010, ItemType.Consumable, "물약", "체력 30 회복", HpEffectType.FixedValue, 30, 1, 0, 1);
            player.MyInventory.AddItem(hpPotion);
            player.AtbGauge = 50;
            player.Trophy = 15; // 보유 Trophy 15개

            Shop.InitializeShop();
            WaveManager.InitializeWave();
            WaveManager.SpawnRandomMonsters(activeCharacters);
            activeCharacters.Add(player);

            InputManager inputManager = new InputManager(player);

            AddLog("⚔️ 실시간 ATB RPG 테스트를 시작합니다!");

            // 시작할 때 화면 한번 쾌적하게 렌더링
            Render(activeCharacters, inputManager, player);

            while (true)
            {
                // 💡 [화면 갱신 조건문 개편]: 
                // 1. 일반 실시간 전투(Battle) 중일 때는 0.1초마다 실시간 게이지 변화 때문에 화면을 계속 다시 그립니다.
                // 2. 하지만 상점, 인벤토리, 선택창 등 시간이 멈춘 정비 상태일 때는 0.1초마다 화면을 그리지 않고 스킵합니다!
                if (inputManager.CurrentState == GameState.Battle)
                {
                    Render(activeCharacters, inputManager, player);
                }

                if (_isBattleOver)
                {
                    if (Console.KeyAvailable) break;
                    Thread.Sleep(100);
                    continue;
                }

                // 일반 전투 상태일 때만 게이지를 실시간으로 흐르게 만듭니다.
                if (inputManager.CurrentState == GameState.Battle && !inputManager.IsTargetingMode)
                {
                    GaugeManager.UpdateAtb(activeCharacters, inputManager.CurrentState, (monsterLog) => {
                        AddLog(monsterLog);
                    });
                }

                // 전투 종료 및 상점 이동 트리거
                if (inputManager.CurrentState == GameState.Battle)
                {
                    string waveStatus = WaveManager.CheckWave(activeCharacters);
                    if (!string.IsNullOrEmpty(waveStatus))
                    {
                        if (waveStatus.Contains("💀 패배"))
                        {
                            _isBattleOver = true;
                            _battleResultText = waveStatus;
                            Render(activeCharacters, inputManager, player);
                        }
                        else if (waveStatus.Contains("🎉 승리"))
                        {
                            player.Trophy += 5;
                            
                            inputManager.CurrentState = GameState.SelectNext;
                            
                            AddLog($"🎉 전투 승리! 전리품 [+5 Trophy] 획득! (현재: {player.Trophy}개)");
                            AddLog("👉 1번을 눌러 상점에 가거나, 2번을 눌러 다음 전투로 가세요.");

                            // 💡 상태가 변했으므로 화면을 즉시 단 한 번 갱신해 줍니다.
                            Render(activeCharacters, inputManager, player);
                        }
                    }
                }

                // 💡 키보드 조작 접수 (유저가 키를 누른 바로 그 타이밍에만 이벤트 처리)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    char keyChar = char.ToLower(keyInfo.KeyChar);

                    // 입력 처리 수행
                    inputManager.HandleGlobalInput(keyChar, activeCharacters, AddLog);

                    // 💡 [가장 중요]: 유저가 상점 등에서 키를 눌러 아이템을 사거나 정비를 했다면,
                    // 무한 루프가 아닌 '키 입력 이벤트 직후'에 화면을 딱 한 번만 수동으로 갱신해 줍니다!
                    Render(activeCharacters, inputManager, player);
                }

                Thread.Sleep(100);
            }
        }

        // 💡 렌더링 코드가 중복되는 것을 막고, 화면 전환 시 완전 세탁(Clear) 기능을 통합한 헬퍼 메서드
        private static void Render(List<Character> activeCharacters, InputManager inputManager, Player player)
        {
            int remainingMonsters = activeCharacters.FindAll(c => !c.IsPlayer && c.Hp > 0).Count;

            // 💡 전투 -> 상점 / 상점 -> 전투 등으로 화면 상태(State)가 바뀌는 역사적인 순간에만 
            // 딱 한 번 도화지를 완전히 탈탈 털어서 스크롤 밀림 현상을 물리적으로 파괴합니다!
            if (inputManager.CurrentState != _lastState)
            {
                Console.Clear();
                _lastState = inputManager.CurrentState; // 최신 상태 업데이트
            }

            UIManager.RenderScreen(
                activeCharacters,
                _battleLogs,
                _isBattleOver,
                _battleResultText,
                remainingMonsters,
                inputManager.CurrentState,
                player,
                inputManager.IsTargetingMode
            );
        }
    }
}
