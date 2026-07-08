using System.Text;

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
        public double CritChance { get; set; }
        public double DodgeChance { get; set; }

        public Character(string name, int hp, int speed, bool isPlayer, double crit, double dodge)
        {
            Name = name;
            Hp = hp;
            MaxHp = hp;
            Speed = (int)(speed * 0.8);
            AtbGauge = 0;
            IsPlayer = isPlayer;
            CritChance = crit;
            DodgeChance = dodge;
        }
    }

    class Program
    {
        private static List<Character> _characters = new List<Character>();
        private static bool _isBattleOver = false;
        private static string _battleResultText = "";
        private static Queue<string> _battleLogs = new Queue<string>();

        private static readonly object _lockObject = new object();
        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            Console.CursorVisible = false;

            _characters.Add(new Character("용사 (플레이어)", 100, 40, true, 0.35, 0.15));
            _characters.Add(new Character("슬라임 (몬스터)", 60, 25, false, 0.10, 0.20));

            _ = Task.Run(() => StartInputListener());

            AddLog("=== 완전 실시간 게이지 소모 배틀! ===");
            AddLog("▶ 즉시 시전 가능! - 1번 공격(4칸 필요) | 2번 자가회복(10칸 필요)");

            while (!_isBattleOver)
            {
                lock (_lockObject)
                {
                    UpdateAtbGauges();
                    RenderScreen();
                }
                await Task.Delay(100);
            }

            await Task.Delay(200);

            lock (_lockObject)
            {
                Console.Clear();
                RenderScreen();
            }

            Console.SetCursorPosition(0, 14);
            if (_battleResultText.Contains("패배"))
                Console.ForegroundColor = ConsoleColor.Red;
            else
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

                // 몬스터는 기존처럼 10칸(100%)이 꽉 차야만 공격하도록 AI 노드 유지
                if (!character.IsPlayer && character.AtbGauge >= 100)
                {
                    ExecuteMonsterTurn(character);
                }
            }
        }

        private static bool CheckDodge(Character defender)
        {
            return _random.NextDouble() < defender.DodgeChance;
        }

        private static bool CheckCritical(Character attacker)
        {
            return _random.NextDouble() < attacker.CritChance;
        }

        // [핵심 변경] 상시대기 입력 제어 노드 (게이지가 100이 아니어도 즉시 반응)
        private static void StartInputListener()
        {
            while (!_isBattleOver)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                    lock (_lockObject)
                    {
                        if (!_isBattleOver)
                        {
                            Character player = _characters.Find(c => c.IsPlayer);
                            Character target = _characters.Find(c => !c.IsPlayer && c.Hp > 0);

                            if (player == null || player.Hp <= 0) continue;

                            // 1번: 일반 공격 (코스트 4칸 = 40 필요)
                            if (keyInfo.KeyChar == '1' && target != null)
                            {
                                // [조건 변경] 100% 대기가 아니라 40 이상만 있으면 즉시 실행 가능!
                                if (player.AtbGauge >= 40)
                                {
                                    int baseDamage = 20;

                                    if (CheckDodge(target))
                                    {
                                        AddLog($"💨 [용사]의 공격을 {target.Name}이(가) 날렵하게 회피했습니다! (Miss)");
                                    }
                                    else
                                    {
                                        if (CheckCritical(player))
                                        {
                                            int critDamage = (int)(baseDamage * 1.5);
                                            target.Hp -= critDamage;
                                            AddLog($"🔥💥 [CRITICAL!!!] 용사의 일격! -> {target.Name}에게 {critDamage}의 치명상!");
                                        }
                                        else
                                        {
                                            target.Hp -= baseDamage;
                                            AddLog($"⚔️ [용사] 공격! -> {target.Name}에게 {baseDamage}의 피해!");
                                        }
                                    }

                                    if (target.Hp <= 0) target.Hp = 0;

                                    // 공격 코스트 4칸(40%) 차감
                                    player.AtbGauge -= 40;

                                    if (target.Hp <= 0)
                                    {
                                        _battleResultText = "▶ 🎉 승리! 슬라임을 완벽하게 제압했습니다!";
                                        _isBattleOver = true;
                                    }
                                }
                                else
                                {
                                    // 게이지 부족 시 경고 소리 또는 시스템 로그 연출 노드
                                    AddLog("⚠️ [시스템] 게이지가 부족합니다! (공격에는 최소 4칸이 필요합니다)");
                                }
                            }
                            // 2번: 자가 회복 (코스트 10칸 = 100 필요)
                            else if (keyInfo.KeyChar == '2')
                            {
                                if (player.AtbGauge >= 100)
                                {
                                    player.Hp = Math.Min(player.MaxHp, player.Hp + 15);
                                    AddLog($"✨ [용사] 힐! -> 자신의 HP를 15 회복했습니다.");

                                    player.AtbGauge -= 100; // 10칸 전량 소모
                                }
                                else
                                {
                                    AddLog("⚠️ [시스템] 게이지가 부족합니다! (회복에는 10칸(100%)이 필요합니다)");
                                }
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(20);
            }
        }

        private static void ExecuteMonsterTurn(Character monster)
        {
            if (_isBattleOver) return;

            Character target = _characters.Find(c => c.IsPlayer && c.Hp > 0);
            if (target != null)
            {
                int baseDamage = 12;

                if (CheckDodge(target))
                {
                    AddLog($"💨 {monster.Name}의 기습! 용사가 간발의 차로 회피했습니다!");
                }
                else
                {
                    if (CheckCritical(monster))
                    {
                        int critDamage = (int)(baseDamage * 1.5);
                        target.Hp -= critDamage;
                        AddLog($"🚨 💥 {monster.Name}의 뼈아픈 치명타! -> [용사]에게 {critDamage} of 대미지!");
                    }
                    else
                    {
                        target.Hp -= baseDamage;
                        AddLog($"💥 {monster.Name}의 기습 공격! -> [용사]에게 {baseDamage}의 피해!");
                    }
                }

                if (target.Hp <= 0) target.Hp = 0;

                if (target.Hp <= 0)
                {
                    _battleResultText = "▶ 💀 패배... 용사가 차가운 바닥에 쓰러졌습니다.";
                    _isBattleOver = true;
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

                string filledBar = new string('■', visualGauge);
                string emptyBar = new string('□', 10 - visualGauge);
                string gaugeBar = filledBar + emptyBar;

                if (c.IsPlayer)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{c.Name,-15} | {status,-10} | ATB: [{gaugeBar}] {c.AtbGauge:F0}%   ");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"{c.Name,-15} | {status,-10} | ATB: [{gaugeBar}] {c.AtbGauge:F0}%   ");
                }
            }

            Console.WriteLine("=========================================================");

            if (_isBattleOver)
            {
                if (_battleResultText.Contains("패배"))
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine($"{_battleResultText,-55}");
                Console.ResetColor();
            }
            else
            {
                // [가이드라인 변경] 100% 대기 플래그가 없으므로 실시간 행동 가이드 출력
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[★ 실시간 난타전!] 1: 일반공격 (코스트 4칸) | 2: 자가회복 (코스트 10칸)   ");
                Console.ResetColor();
            }

            Console.WriteLine("======================= 배틀 로그 =======================");

            foreach (var log in _battleLogs)
            {
                string paddedLog = log + new string(' ', Console.WindowWidth - log.Length);

                if (log.Contains("[CRITICAL!!!]") || log.Contains("치명타!"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(paddedLog);
                    Console.ResetColor();
                }
                else if (log.Contains("회피했습니다!"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(paddedLog);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(paddedLog);
                }
            }
        }
    }
}