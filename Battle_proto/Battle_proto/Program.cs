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

        public static void AddLog(string message)
        {
            _battleLogs.Enqueue(message);
            if (_battleLogs.Count > 5) _battleLogs.Dequeue();
        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Clear();

            List<Character> activeCharacters = new List<Character>();
            Player player = new Player("용사", 100, 50, 0.1, 0.0);

            ItemData hpPotion = new ItemData(1010, ItemType.Consumable, "물약", "체력 30 회복", HpEffectType.FixedValue, 30, 1, 0, 1);
            ItemData bigPotion = new ItemData(1012, ItemType.Consumable, "대형 물약", "체력을 전부 회복", HpEffectType.FullRecovery, 0, 8, 0, 6);
            ItemData bomb = new ItemData(1008, ItemType.Consumable, "폭탄", "다중 피해 투척 무기", HpEffectType.None, 0, 3, 30, 4);

            player.MyInventory.AddItem(hpPotion);
            player.MyInventory.AddItem(bigPotion);
            player.MyInventory.AddItem(bomb);
            player.AtbGauge = 50;

            WaveManager.InitializeWave();
            WaveManager.SpawnRandomMonsters(activeCharacters);
            activeCharacters.Add(player);

            InputManager inputManager = new InputManager(player);

            AddLog("⚔️ 실시간 ATB RPG 테스트를 시작합니다!");

            while (true)
            {
                int remainingMonsters = activeCharacters.FindAll(c => !c.IsPlayer && c.Hp > 0).Count;

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

                if (_isBattleOver)
                {
                    if (Console.KeyAvailable) break;
                    Thread.Sleep(100);
                    continue;
                }

                // 💡 [시간 정지 버그 해결]: 인벤토리뿐만 아니라, 플레이어가 조준 모드(IsTargetingMode)일 때도 
                // 게이지가 흘러가지 않도록 차단하여 유저가 신중하게 적 번호를 고를 수 있게 보장합니다!
                if (inputManager.CurrentState == GameState.Battle && !inputManager.IsTargetingMode)
                {
                    GaugeManager.UpdateAtb(activeCharacters, inputManager.CurrentState, (monsterLog) => {
                        AddLog(monsterLog);
                    });
                }

                string waveStatus = WaveManager.CheckWave(activeCharacters);
                if (!string.IsNullOrEmpty(waveStatus))
                {
                    if (waveStatus.Contains("💀 패배"))
                    {
                        _isBattleOver = true;
                        _battleResultText = waveStatus;
                    }
                    else if (waveStatus.Contains("🎉 승리") && inputManager.CurrentState == GameState.Battle)
                    {
                        inputManager.CurrentState = GameState.SelectNext;
                        AddLog("🏆 전투 승리! 다음 행동을 선택하세요.");
                    }
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    char keyChar = char.ToLower(keyInfo.KeyChar);
                    inputManager.HandleGlobalInput(keyChar, activeCharacters, AddLog);
                }

                Thread.Sleep(100);
            }
        }
    }
}
