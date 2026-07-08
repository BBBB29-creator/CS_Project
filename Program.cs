using System;
using System.Text;

namespace ConsoleAtbGame
{
    class AsciiUiProgram
    {
        const int WINDOW_WIDTH = 100;
        const int WINDOW_HEIGHT = 28;

        // [실시간 변경을 위한 글로벌 변수] - 어느 함수에서나 접근 가능하도록 설정
        static int playerHp = 60;       // 현재 체력 (수시로 바뀜)
        static int playerMaxHp = 100;   // 최대 체력
        static int playerMoney = 1500;  // 현재 돈 (수시로 바뀜)
        static int playerMaxMoney = 3000; // 돈 게이지가 가득 차는 기준 금액

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.SetBufferSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.CursorVisible = false;

            // 1. 전체 화면 기본 틀 그리기 (처음 한 번만 실행)
            DrawStaticBaseUi();

            // 2. 실시간/수시로 변하는 상태창 블록 그리기
            UpdateStatusWindow();

            // =================================================================================
            // [기능 테스트 데모 코드] 키보드 입력에 따라 실시간으로 높이가 변하는지 확인해보세요!
            // =================================================================================
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.D2) // 2번 키: 전투를 끝내고 대미지를 입었을 때 가정 (체력 감소)
                    {
                        playerHp -= 20;
                        if (playerHp < 0) playerHp = 0;
                        UpdateStatusWindow(); // 상태창 리프레시
                    }
                    else if (key.Key == ConsoleKey.D3) // 3번 키: 상점이나 아이템으로 회복했을 때 가정 (체력 회복)
                    {
                        playerHp += 20;
                        if (playerHp > playerMaxHp) playerHp = playerMaxHp;
                        UpdateStatusWindow(); // 상태창 리프레시
                    }
                    else if (key.Key == ConsoleKey.D4) // 4번 키: 돈을 획득했을 때 가정 (돈 게이지 상승)
                    {
                        playerMoney += 500;
                        if (playerMoney > playerMaxMoney) playerMoney = playerMaxMoney;
                        UpdateStatusWindow(); // 상태창 리프레시
                    }
                    else if (key.Key == ConsoleKey.D5) // 5번 키: 종료
                    {
                        break;
                    }
                }
            }
        }

        // [함수 1] 변하지 않는 고정된 배경 틀만 그리는 메서드
        static void DrawStaticBaseUi()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            // 상단 타이틀 바
            Console.WriteLine(new string('=', WINDOW_WIDTH));
            Console.WriteLine($"|{" ",47}콘솔 활용 RPG{" ",47}|");
            Console.WriteLine($"|{" ",44}콘솔 앱 프레임워크{" ",43}|");
            Console.WriteLine(new string('=', WINDOW_WIDTH));
            Console.WriteLine();

            // 좌측 메인 메뉴 테두리 및 텍스트
            DrawAsciiBox(4, 5, 42, 15, " [ 메인 메뉴 ] ");
            Console.ForegroundColor = ConsoleColor.White;
            string[] menus = {
                "1. 게임 설명 및 개요",
                "2. 전투 페이즈 (체력깎기 테스트)",
                "3. 상점 페이즈 (체력회복 테스트)",
                "4. 인벤토리 페이즈 (돈획득 테스트)",
                "5. 콘솔창 종료"
            };
            for (int i = 0; i < menus.Length; i++)
            {
                Console.SetCursorPosition(8, 7 + (i * 2));
                Console.Write(menus[i]);
            }

            // 우측 상태창 테두리 고정틀
            Console.ForegroundColor = ConsoleColor.Cyan;
            DrawAsciiBox(50, 5, 44, 15, " [ 상태창 ] ");

            // 변하지 않는 상태창 우측 아이템 리스트 고정 텍스트
            Console.ForegroundColor = ConsoleColor.White;
            string[] statusItems = {
                "무기1   (없음)", "무기2   (없음)", "아이템1 (없음)",
                "아이템2 (없음)", "아이템3 (없음)", "아이템4 (없음)"
            };
            for (int i = 0; i < statusItems.Length; i++)
            {
                Console.SetCursorPosition(75, 7 + i);
                Console.Write(statusItems[i]);
            }

            // 하단 버전 바
            int footerY = 22;
            Console.SetCursorPosition(0, footerY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', WINDOW_WIDTH));
            Console.WriteLine($"|{" ",42}Version_1.0{" ",43}|");
            Console.WriteLine(new string('=', WINDOW_WIDTH));
            Console.ResetColor();
        }

        // [함수 2★] 변수값에 따라 사각형 블록을 "수시로 다시 그려주는" 핵심 가변 메서드
        static void UpdateStatusWindow()
        {
            // 최대 높이는 기획안 기준 5칸 (7층 ~ 11층)
            int maxHeight = 5;

            // 1. 현재 체력 비율에 따른 높이 계산 (0 ~ 5칸)
            double hpRatio = (double)playerHp / playerMaxHp;
            int currentHpHeight = (int)Math.Round(hpRatio * maxHeight);

            // 2. 현재 돈 비율에 따른 높이 계산 (0 ~ 4칸으로 기획안 계단식 조율)
            double moneyRatio = (double)playerMoney / playerMaxMoney;
            int currentMoneyHeight = (int)Math.Round(moneyRatio * 4);

            // -----------------------------------------------------------------
            // 체력/돈 블록 그리기 (잔상이 남지 않도록 항상 위에서 아래로 덮어씌움)
            // -----------------------------------------------------------------
            for (int i = 0; i < maxHeight; i++)
            {
                // 역순 계산 (층수는 7층부터 시작하므로 위에서부터 공백인지 블록인지 판단)
                int currentLevel = maxHeight - i;

                // [체력 블록 렌더링]
                Console.SetCursorPosition(54, 7 + i);
                if (currentLevel <= currentHpHeight)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("■■■");
                }
                else
                {
                    Console.Write("      "); // 체력이 닳은 칸은 공백으로 지우기
                }

                // [돈 블록 렌더링 - 계단식 연출을 위해 8층부터 시작]
                Console.SetCursorPosition(64, 7 + i);
                if (i == 0)
                {
                    Console.Write("      "); // 돈 게이지의 맨 위층(7층)은 무조건 공백 (계단식)
                }
                else if (currentLevel <= currentMoneyHeight)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("■■■");
                }
                else
                {
                    Console.Write("      "); // 돈이 부족한 칸은 공백으로 지우기
                }
            }

            // 하단 텍스트 라벨 및 현재 수치 정보 출력 (Y: 14)
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(53, 14);
            Console.Write($"체력:{playerHp,3}"); // 예: 체력: 60

            Console.SetCursorPosition(63, 14);
            Console.Write($"돈:{playerMoney,4}"); // 예: 돈:1500
        }

        // 테두리 상자 그리기 공용 함수
        static void DrawAsciiBox(int startX, int startY, int width, int height, string title)
        {
            Console.SetCursorPosition(startX, startY);
            Console.Write(new string('-', width));
            for (int i = 1; i < height - 1; i++)
            {
                Console.SetCursorPosition(startX, startY + i);
                Console.Write("|");
                Console.SetCursorPosition(startX + width - 1, startY + i);
                Console.Write("|");
            }
            Console.SetCursorPosition(startX, startY + height - 1);
            Console.Write(new string('-', width));
            Console.SetCursorPosition(startX + 4, startY);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(title);
        }
    }
}
