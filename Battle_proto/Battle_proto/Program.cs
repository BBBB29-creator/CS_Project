using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleAtbRpg
{
    // [비주얼 스크립트 노드]
    public class Character
    {
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Speed { get; set; }      // 게이지가 차오르는 속도
        public double AtbGauge { get; set; } // 현재 ATB 게이지 (100이 되면 턴 획득)
        public bool IsPlayer { get; set; }

        public Character(string name, int hp, int speed, bool isPlayer)
        {
            Name = name;
            Hp = hp;
            MaxHp = hp;
            Speed = speed;
            AtbGauge = 0;
            IsPlayer = isPlayer;
        }
    }

    class Program
    {
        private static List<Character> _characters = new List<Character>();
        private static bool _isBattleOver = false;

        static async Task Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("=== 비주얼 스크립트 스타일 ATB 전투 시작 ===");

            // 캐릭터 데이터 노드 초기화
            _characters.Add(new Character("용사 (플레이어)", 100, 40, true));
            _characters.Add(new Character("슬라임 (몬스터)", 60, 25, false));

            // 메인 루프 (실시간 dynamic 연산 구동)
            while (!_isBattleOver)
            {
                UpdateAtbGauges();
                RenderStatus();

                // 턴이 가득 찬 캐릭터가 있는지 체크 (비주얼 스크립트의 '조건 분기 노드' 역할)
                Character readyChar = _characters.Find(c => c.AtbGauge >= 100);
                if (readyChar != null)
                {
                    await ExecuteTurnNode(readyChar);
                }

                CheckBattleOver();
                await Task.Delay(100); // 0.1초마다 실시간으로 흐름을 갱신 (Tick)
            }

            Console.WriteLine("\n전투가 종료되었습니다.");
        }

        // [노드 1] 실시간 ATB 게이지 충전 노드
        private static void UpdateAtbGauges()
        {
            foreach (var character in _characters)
            {
                if (character.Hp > 0)
                {
                    // 속도에 비례하여 게이지 누적 (실시간 물리/시간 연산)
                    character.AtbGauge += character.Speed * 0.1;
                    if (character.AtbGauge > 100) character.AtbGauge = 100;
                }
            }
        }

        // [노드 2] 화면 UI 출력 노드 (콘솔 화면을 깔끔하게 유지)
        private static void RenderStatus()
        {
            Console.SetCursorPosition(0, 2);
            foreach (var c in _characters)
            {
                string gaugeBar = new string('■', (int)(c.AtbGauge / 10)) + new string('□', 10 - (int)(c.AtbGauge / 10));
                Console.WriteLine($"{c.Name,-15} | HP: {c.Hp,3}/{c.MaxHp,3} | ATB: [{gaugeBar}] {c.AtbGauge:F0}%   ");
            }
        }

        // [노드 3] 턴 실행 제어 노드 (이벤트 실행 트리거)
        private static async Task ExecuteTurnNode(Character character)
        {
            Console.SetCursorPosition(0, 6);
            Console.WriteLine(new string(' ', Console.WindowWidth)); // 이전 텍스트 지우기
            Console.SetCursorPosition(0, 6);

            if (character.IsPlayer)
            {
                // [서브 노드] 플레이어 입력 대기 노드 진입
                Console.WriteLine($"▶ {character.Name}의 턴! 행동을 선택하세요. (1: 공격 / 2: 방어)");
                string input = Console.ReadLine();

                Character target = _characters.Find(c => !c.IsPlayer && c.Hp > 0);
                if (target != null && input == "1")
                {
                    target.Hp -= 25;
                    Console.WriteLine($"-> {character.Name}이(가) {target.Name}에게 25의 피해를 주었습니다!");
                }
                else
                {
                    Console.WriteLine($"-> {character.Name}은(는) 방어 자세를 취했습니다.");
                }
            }
            else
            {
                // [서브 노드] AI 행동 결정 노드 진ip
                Console.WriteLine($"{character.Name}의 턴! 적이 공격해옵니다.");
                await Task.Delay(800); // 몬스터 행동 연출을 위한 인위적 지연

                Character target = _characters.Find(c => c.IsPlayer && c.Hp > 0);
                if (target != null)
                {
                    target.Hp -= 15;
                    Console.WriteLine($"-> {character.Name}이(가) {target.Name}에게 15의 피해를 주었습니다!");
                }
            }

            // 턴 종료 후 게이지 리셋
            character.AtbGauge = 0;
            await Task.Delay(1500); // 연출을 읽을 수 있도록 대기
            Console.Clear();
            Console.WriteLine("=== 비주얼 스크립트 스타일 ATB 전투 진행 중 ===");
        }

        // [노드 4] 게임 종료 조건 조건식 노드
        private static void CheckBattleOver()
        {
            var player = _characters.Find(c => c.IsPlayer);
            var monster = _characters.Find(c => !c.IsPlayer);

            if (player.Hp <= 0)
            {
                Console.SetCursorPosition(0, 10);
                Console.WriteLine("▶ 패배... 전멸했습니다.");
                _isBattleOver = true;
            }
            else if (monster.Hp <= 0)
            {
                Console.SetCursorPosition(0, 10);
                Console.WriteLine("▶ 승리! 몬스터를 처치했습니다.");
                _isBattleOver = true;
            }
        }
    }
}