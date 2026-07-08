using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeAtbRpg
{
    class Program
    {
        private static List<Character> _characters = new List<Character>();
        private static Queue<string> _battleLogs = new Queue<string>();
        private static bool _isBattleOver = false;
        private static string _battleResultText = "";
        private static readonly object _lockObject = new object();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear(); Console.CursorVisible = false;

            // 초기 데이터 세팅 및 몬스터 10마리 명단 생성
            _characters.Add(new Player("용사 (플레이어)", 100, 40, 0.35, 0.15));
            WaveManager.InitializeWave();
            if (WaveManager.MonsterWave.Count > 0) _characters.Add(WaveManager.MonsterWave.Dequeue());

            // 상시 키 입력 루프 가동
            _ = Task.Run(() => {
                while (!_isBattleOver)
                {
                    System.Threading.Thread.Sleep(20);
                    if (!Console.KeyAvailable) continue; // 입력 없으면 패스

                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                    lock (_lockObject)
                    {
                        if (_isBattleOver) break;

                        // Character 리스트에서 Player 객체를 안전하게 찾아옵니다.
                        Player player = _characters.Find(c => c is Player) as Player;
                        Character target = _characters.Find(c => !c.IsPlayer && c.Hp > 0);

                        if (player == null || player.Hp <= 0) continue;

                        // [핵심 축소] 이제 InputManager 대신 용사 본인에게 키보드 값을 던져 처리하게 합니다!
                        player.HandleInput(keyInfo.KeyChar, target, AddLog);
                    }
                }
            });

            AddLog("=== 완전 모듈화 압축 실시간 ATB 배틀 ===");

            // 메인 루프 (중앙 시계탑)
            while (!_isBattleOver)
            {
                lock (_lockObject)
                {
                    GaugeManager.UpdateAtb(_characters, AddLog);
                    string status = WaveManager.CheckWave(_characters);
                    if (status.StartsWith("▶")) { _battleResultText = status; _isBattleOver = true; }
                    else if (!string.IsNullOrEmpty(status)) AddLog(status);

                    UIManager.RenderScreen(_characters, _battleLogs, _isBattleOver, _battleResultText, WaveManager.MonsterWave.Count);
                }
                await Task.Delay(100);
            }

            // 최종 정산 화면 출력
            await Task.Delay(200);
            lock (_lockObject) { Console.Clear(); UIManager.RenderScreen(_characters, _battleLogs, _isBattleOver, _battleResultText, WaveManager.MonsterWave.Count); }

            Console.SetCursorPosition(0, 14);
            Console.ForegroundColor = _battleResultText.Contains("패배") ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.WriteLine("\n=========================================================");
            Console.WriteLine(" 전투가 종료되었습니다. 프로그램을 종료하려면 아무 키나 누르세요.");
            Console.WriteLine("=========================================================");
            Console.ResetColor(); Console.CursorVisible = true; Console.ReadKey();
        }

        public static void AddLog(string message)
        {
            _battleLogs.Enqueue(message);
            if (_battleLogs.Count > 5) _battleLogs.Dequeue();
        }
    }
}
