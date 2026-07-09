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

            // 초기 세팅
            ItemData hpPotion = new ItemData(1010, ItemType.Consumable, "물약", "체력 30 회복", HpEffectType.FixedValue, 30, 1, 0, 1);
            player.MyInventory.AddItem(hpPotion);
            player.AtbGauge = 50;
            player.Trophy = 15; // 초기 자금 15개 지급

            Shop.InitializeShop();
            WaveManager.InitializeWave();
            WaveManager.SpawnRandomMonsters(activeCharacters);
            activeCharacters.Add(player);

            InputManager inputManager = new InputManager(player);

            // 💡 [3단계 수정]: 게임 시작 시 기본 상태를 'Title'로 강제 지정합니다!
            inputManager.CurrentState = GameState.Title;

            AddLog("⚔️ 실시간 ATB RPG 테스트를 시작합니다!");

            // 첫 타이틀 화면 렌더링
            Render(activeCharacters, inputManager, player);

            // 💡 메인 루프 시작
            while (true)
            {
                // 💡 [3단계 수정]: 전투 중이거나 타이틀 화면일 때만 화면을 실시간 갱신합니다.
                if (inputManager.CurrentState == GameState.Battle || inputManager.CurrentState == GameState.Title)
                {
                    Render(activeCharacters, inputManager, player);
                }

                if (_isBattleOver)
                {
                    if (Console.KeyAvailable) break;
                    Thread.Sleep(100);
                    continue;
                }

                // 💡 [3단계 수정]: 오직 전투(Battle) 상태이면서 조준 중이 아닐 때만 시간이 실시간으로 흐릅니다.
                if (inputManager.CurrentState == GameState.Battle && !inputManager.IsTargetingMode)
                {
                    GaugeManager.UpdateAtb(activeCharacters, inputManager.CurrentState, (monsterLog) => {
                        AddLog(monsterLog);
                    });
                }

                // 전투 상태일 때만 승리/패배 조건을 체크합니다 (데자뷔 버그 차단)
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
                            player.Trophy += 5; // 🏆 전리품 +5 지급
                            inputManager.CurrentState = GameState.SelectNext;
                            AddLog($"🎉 전투 승리! 전리품 [+5 Trophy] 획득! (현재: {player.Trophy}개)");
                            AddLog("👉 1번을 눌러 상점에 가거나, 2번을 눌러 다음 전투로 가세요.");
                            Render(activeCharacters, inputManager, player);
                        }
                    }
                }

                // 💡 키보드 조작 접수
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    char keyChar = char.ToLower(keyInfo.KeyChar);

                    // 💡 [3단계 수정]: 현재 타이틀 화면 상태라면 'S' 키 입력을 감지해 탈출합니다.
                    if (inputManager.CurrentState == GameState.Title)
                    {
                        if (keyChar == 's')
                        {
                            Console.Clear();
                            inputManager.CurrentState = GameState.Battle; // 전투 개시!
                            AddLog("🚩 모험이 시작되었습니다! 전장의 적들을 격파하세요!");
                        }
                    }
                    else
                    {
                        // 일반 인게임(전투, 상점, 인벤토리) 조작 접수
                        inputManager.HandleGlobalInput(keyChar, activeCharacters, AddLog);
                    }

                    // 입력 직후 화면을 딱 1프레임 갱신해 줍니다.
                    Render(activeCharacters, inputManager, player);
                }

                Thread.Sleep(100);
            }
        }

        private static void Render(List<Character> activeCharacters, InputManager inputManager, Player player)
        {
            int remainingMonsters = activeCharacters.FindAll(c => !c.IsPlayer && c.Hp > 0).Count;

            // 상태가 바뀔 때(Title -> Battle, Battle -> Shop 등)만 도화지를 완전히 탈탈 틉니다.
            if (inputManager.CurrentState != _lastState)
            {
                Console.Clear();
                _lastState = inputManager.CurrentState;
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
