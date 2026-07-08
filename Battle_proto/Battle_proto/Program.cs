using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealtimeAtbRpg
{
    public class Character
    {
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Speed { get; set; }
        public double AtbGauge { get; set; }
        public bool IsPlayer { get; set; }

        public Character(string name, int hp, int speed, bool isPlayer)
        {
            Name = name;
            Hp = hp;
            MaxHp = hp;
            Speed = (int)(speed * 0.8);
            AtbGauge = 0;
            IsPlayer = isPlayer;
        }
    }

    class Program
    {
        private static List<Character> _characters = new List<Character>();
        private static bool _isBattleOver = false;
        private static string _battleResultText = ""; // [추가] 최종 승패를 저장할 변수
        private static Queue<string> _battleLogs = new Queue<string>();
        private static bool _isPlayerTurnActive = false;

        private static readonly object _lockObject = new object();

        static async Task Main(string[] args)
        {
            // [핵심 추가] 콘솔창이 유니코드(UTF-8) 이모지를 지원하도록 인코딩 노드 설정
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Clear();
            Console.CursorVisible = false;

            _characters.Add(new Character("용사 (플레이어)", 100, 40, true));
            _characters.Add(new Character("슬라임 (몬스터)", 60, 25, false));

            _ = Task.Run(() => StartInputListener());

            AddLog("=== 실시간 전투 시작! ===");

            // 메인 실시간 루프
            while (!_isBattleOver)
            {
                lock (_lockObject)
                {
                    UpdateAtbGauges();
                    RenderScreen();
                }
                await Task.Delay(100);
            }

            // [핵심 해결] 게임 종료 직후 0.2초 여유를 주어 백그라운드 렌더링 잔상을 기다림
            await Task.Delay(200);

            // 최종 화면 강제 갱신
            lock (_lockObject)
            {
                Console.Clear(); // 콘솔 잔상을 완벽하게 밀어버림
                RenderScreen();
            }

            // 하단 종료 안내
            Console.SetCursorPosition(0, 14);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=========================================================");
            Console.WriteLine(" 전투가 최종 종료되었습니다. 프로그램을 종료하려면 아무 키나 누르세요.");
            Console.WriteLine("=========================================================");
            Console.ResetColor();

            Console.CursorVisible = true;
            Console.ReadKey();
        }

        private static void UpdateAtbGauges()
        {
            if (_isBattleOver) return;

            foreach (var character in _characters)
            {
                if (character.Hp <= 0)
                {
                    character.AtbGauge = 0;
                    continue;
                }

                if (character.AtbGauge < 100)
                {
                    character.AtbGauge += character.Speed * 0.1;
                    if (character.AtbGauge > 100) character.AtbGauge = 100;
                }

                if (!character.IsPlayer && character.AtbGauge >= 100)
                {
                    ExecuteMonsterTurn(character);
                }
                else if (character.IsPlayer && character.AtbGauge >= 100)
                {
                    _isPlayerTurnActive = true;
                }
            }
        }

        private static void StartInputListener()
        {
            while (!_isBattleOver)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                    lock (_lockObject)
                    {
                        if (_isPlayerTurnActive && !_isBattleOver)
                        {
                            Character player = _characters.Find(c => c.IsPlayer);
                            Character target = _characters.Find(c => !c.IsPlayer && c.Hp > 0);

                            if (player == null || player.Hp <= 0) continue;

                            if (keyInfo.KeyChar == '1' && target != null)
                            {
                                target.Hp -= 20;
                                if (target.Hp <= 0) target.Hp = 0;

                                AddLog($"[용사] 공격! -> {target.Name}에게 20의 피해!");
                                ResetPlayerTurn(player);

                                if (target.Hp <= 0)
                                {
                                    _battleResultText = "▶ 🎉 승리! 슬라임을 완벽하게 제압했습니다!";
                                    _isBattleOver = true;
                                    _isPlayerTurnActive = false;
                                }
                            }
                            else if (keyInfo.KeyChar == '2')
                            {
                                player.Hp = Math.Min(player.MaxHp, player.Hp + 15);
                                AddLog($"✨ [용사] 힐! -> 자신의 HP를 15 회복했습니다.");
                                ResetPlayerTurn(player);
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(20);
            }
        }

        private static void ResetPlayerTurn(Character player)
        {
            player.AtbGauge = 0;
            _isPlayerTurnActive = false;
        }

        private static void ExecuteMonsterTurn(Character monster)
        {
            if (_isBattleOver) return;

            Character target = _characters.Find(c => c.IsPlayer && c.Hp > 0);
            if (target != null)
            {
                target.Hp -= 12;
                if (target.Hp <= 0) target.Hp = 0;

                AddLog($"슬라임 (몬스터)의 기습 공격! -> [용사]에게 12의 피해!");

                if (target.Hp <= 0)
                {
                    _battleResultText = "▶ 💀 패배... 용사가 차가운 바닥에 쓰러졌습니다.";
                    _isBattleOver = true;
                    _isPlayerTurnActive = false;
                }
            }
            monster.AtbGauge = 0;
        }

        private static void AddLog(string message)
        {
            _battleLogs.Enqueue(message);
            if (_battleLogs.Count > 5)
            {
                _battleLogs.Dequeue();
            }
        }

        private static void RenderScreen()
        {
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("=========================================================");

            foreach (var c in _characters)
            {
                string status = c.Hp <= 0 ? "[사망]" : $"HP: {c.Hp,3}/{c.MaxHp,3}";
                int visualGauge = (int)(c.AtbGauge / 10);
                string gaugeBar = new string('■', visualGauge) + new string('□', 10 - visualGauge);

                // [핵심 가시성 노드] 플레이어(용사)와 몬스터의 출력 색상을 다르게 분기
                if (c.IsPlayer)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan; // 용사는 선명한 청록색으로 출력
                    Console.WriteLine($"{c.Name,-15} | {status,-10} | ATB: [{gaugeBar}] {c.AtbGauge:F0}%   ");
                    Console.ResetColor(); // 다음 글자를 위해 색상 초기화
                }
                else
                {
                    // 몬스터는 기본 흰색 (혹은 원하시면 ConsoleColor.Red 등을 주셔도 좋습니다)
                    Console.WriteLine($"{c.Name,-15} | {status,-10} | ATB: [{gaugeBar}] {c.AtbGauge:F0}%   ");
                }
            }

            Console.WriteLine("=========================================================");

            if (_isBattleOver)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // 종료 안내는 노란색으로 강조
                Console.WriteLine($"{_battleResultText,-55}");
                Console.ResetColor();
            }
            else if (_isPlayerTurnActive)
            {
                Console.ForegroundColor = ConsoleColor.Green; // 행동 입력 유도는 초록색 유지
                Console.WriteLine("[★ 당신의 턴! 실시간 진행 중] 1: 일반공격 | 2: 자가회복   ");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("[...게이지 충전 중...] 상대의 공격에 대비하세요.           ");
            }

            Console.WriteLine("======================= 배틀 로그 =======================");

            foreach (var log in _battleLogs)
            {
                // 배틀 로그 내부에서도 [용사] 단어가 들어가면 가시성을 주기 위한 처리
                if (log.Contains("[용사]"))
                {
                    // 로그 전체를 청록색 빛이 돌게 하거나 기본 출력
                    Console.WriteLine(log + new string(' ', Console.WindowWidth - log.Length));
                }
                else
                {
                    Console.WriteLine(log + new string(' ', Console.WindowWidth - log.Length));
                }
            }
        }
    }
}
