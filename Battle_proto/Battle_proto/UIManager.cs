namespace RealtimeAtbRpg
{
    public static class UIManager
    {
        public static void RenderScreen(
            List<Character> characters,
            Queue<string> battleLogs,
            bool isBattleOver,
            string battleResultText,
            int remainingMonsters)
        {
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("=========================================================");

            foreach (var c in characters)
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
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{c.Name,-15} | {status,-10} | ATB: [{gaugeBar}] {c.AtbGauge:F0}%   ");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("=========================================================");

            if (isBattleOver)
            {
                if (battleResultText.Contains("패배"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }

                // 공백 패딩(,-55)을 제거하여 유니코드 이모지로 인한 줄밀림 버그를 차단합니다.
                Console.WriteLine(battleResultText);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[★ 실시간 난타전!] 1: 일반공격(4칸) | 2: 자가회복(10칸) [남은 적: {remainingMonsters}마리]   ");
                Console.ResetColor();
            }

            Console.WriteLine("======================= 배틀 로그 =======================");

            foreach (var log in battleLogs)
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
