using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public static class UIManager
    {
        public static void RenderScreen(
            List<Character> characters,
            Queue<string> battleLogs,
            bool isBattleOver,
            string battleResultText,
            int remainingMonsters,
            GameState currentState,
            Player player,
            bool isTargetingMode)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.SetCursorPosition(0, 0);

            WriteCleanLine("=========================================================");
            string timeStatus = (currentState == GameState.Battle ? "RUNNING" : "PAUSED");
            WriteCleanLine($" [현재 상태]: {currentState,-10} | 시간 흐름: {timeStatus}");
            WriteCleanLine("=========================================================");

            // =================================================================
            // 💡 [레이아웃 대수술]: 플레이어 스탯창, 구분선, 몬스터 시체 목록 전체를 
            // 오직 일반 전투(Battle) 진행 중이거나 공격 조준 모드(isTargetingMode)일 때만 출력합니다!
            // 상점(Shop)이나 마을 갈림길 상태일 때는 전투 관련 레이아웃을 통째로 숨깁니다.
            // =================================================================
            if (currentState == GameState.Battle || isTargetingMode)
            {
                // 1. 플레이어 항상 맨 위에 고정 출력
                Character playerChar = characters.Find(c => c.IsPlayer);
                if (playerChar != null)
                {
                    string status = playerChar.Hp <= 0 ? "[사망]" : $"HP: {playerChar.Hp,3}/{playerChar.MaxHp,3}";
                    int visualGauge = Math.Max(0, Math.Min(10, (int)(playerChar.AtbGauge / 10)));
                    string gaugeBar = new string('■', visualGauge) + new string('□', 10 - visualGauge);

                    // 💡 [UI 반영]: 모험가 객체를 캐스팅하여 현재 장착한 무기가 있다면 이름을 띄우고, 없으면 [기본 검]으로 표시!
                    Player p = playerChar as Player;
                    string weaponName = (p != null && p.EquippedWeapon != null) ? p.EquippedWeapon.Name : "기본 검";

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    // 무기 이름(weaponName) 항목을 스탯 바 중앙에 이쁘게 배치합니다.
                    Console.WriteLine($"{playerChar.Name,-8} ({weaponName}) | {status,-10} | ATB: [{gaugeBar}] {playerChar.AtbGauge:F0}% \x1b[K");
                    Console.ResetColor();
                }

                WriteCleanLine("---------------------------------------------------------");

                // 2. 몬스터들 출력 및 조준 화살표 연출
                var monsters = characters.FindAll(c => !c.IsPlayer);
                for (int i = 0; i < monsters.Count; i++)
                {
                    var m = monsters[i];
                    string status = m.Hp <= 0 ? "[사망]" : $"HP: {m.Hp,3}/{m.MaxHp,3}";
                    int visualGauge = Math.Max(0, Math.Min(10, (int)(m.AtbGauge / 10)));
                    string gaugeBar = new string('■', visualGauge) + new string('□', 10 - visualGauge);

                    string targetPrefix = m.Hp > 0 ? $"[{i + 1}번] " : "      ";
                    string pointer = (isTargetingMode && m.Hp > 0) ? " <--- TARGET" : "";

                    if (isTargetingMode && m.Hp > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{targetPrefix}{m.Name,-11} | {status,-10} | ATB: [{gaugeBar}] {m.AtbGauge:F0}%{pointer} \x1b[K");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{targetPrefix}{m.Name,-11} | {status,-10} | ATB: [{gaugeBar}] {m.AtbGauge:F0}% \x1b[K");
                        Console.ResetColor();
                    }
                }

                WriteCleanLine("=========================================================");
            }

            WriteCleanLine("=========================================================");

            // 3. 상태별 조작 메뉴 분기 처리
            WriteCleanLine("=========================================================");

            // =================================================================
            // 💡 [2단계 코드가 들어갈 정확한 위치!]
            // 기존 if (isBattleOver)를 else if로 밀어내고, 맨 위에 Title 분기를 얹어줍니다.
            // =================================================================
            if (currentState == GameState.Title)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                WriteCleanLine("   =====================================================");
                WriteCleanLine("      ____  _____    _    _   _____ ___ __  __ _____ ");
                WriteCleanLine("     |  _ \\| ____|  / \\  | | |_   _|_ _|  \\/  | ____|");
                WriteCleanLine("     | |_) |  _|   / _ \\ | |   | |  | || |\\/| |  _|  ");
                WriteCleanLine("     |  _ <| |___ / ___ \\| |___| |  | || |  | | |___ ");
                WriteCleanLine("     |_| \\_\\_____/_/   \\_\\_____|_| |___|_|  |_|_____|");
                WriteCleanLine("                                                      ");
                WriteCleanLine("                 O=======================>            ");
                WriteCleanLine("                   REALTIME ATB RPG v1.0              ");
                WriteCleanLine("   =====================================================");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteCleanLine("");
                WriteCleanLine("          👉 [ Press 'S' Key to Start Adventure ]      ");
                WriteCleanLine("");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                WriteCleanLine("            (C) 2026 Adventurer Studio. All Rights Reserved. ");
                Console.ResetColor();

                return; // 타이틀 화면일 때는 하단 몬스터나 배틀로그를 그리지 않고 탈출!
            }
            // 💡 여기서부터 기존에 가지고 계시던 조건문들이 자연스럽게 'else if'로 이어집니다.
            else if (isBattleOver)
            {
                Console.ForegroundColor = battleResultText.Contains("패배") ? ConsoleColor.Red : ConsoleColor.Cyan;
                Console.WriteLine(battleResultText + " \x1b[K");
                Console.ResetColor();
            }
            else if (isTargetingMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" [조준 모드] 공격할 적의 번호(1~2)를 입력하세요! | Q: 취소 \x1b[K");
                Console.ResetColor();
            }
            else if (currentState == GameState.Inventory)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" [인벤토리] 번호(1~4)를 눌러 사용 | Q: 가방 닫기 \x1b[K");
                Console.ResetColor();

                var items = player.MyInventory.Items;
                for (int i = 0; i < 4; i++)
                {
                    if (i < items.Count)
                        Console.WriteLine($" [{i + 1}] {items[i].Name,-10} (비용: {items[i].MoveCost} / 효과: {items[i].Description}) \x1b[K");
                    else
                        Console.WriteLine($" [{i + 1}] ----- 비어 있음 ----- \x1b[K");
                }
            }
            // 🏪 상점 화면 출력 분기
            else if (currentState == GameState.Shop)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                // 💡 역슬래시 대신 일반 슬래시(/)와 파이프(|) 기호 조합으로 변경하여 
                // 컴파일러 오작동과 윗줄 반짝임(깜빡임) 버그를 완벽하게 차단했습니다!
                WriteCleanLine("               /___________________/               ");
                WriteCleanLine("              ||   M A R K E T   ||                ");
                WriteCleanLine("   ___________||_________________||___________    ");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;

                WriteCleanLine($"   [ 모험가 ]  보유 전리품 : {player.Trophy,3} 개   ");
                Console.ResetColor();
                WriteCleanLine("   ===========================================   ");

                var items = Shop.ShopItems;

                // ⚔️ 장비류 (0~3)
                Console.ForegroundColor = ConsoleColor.Cyan;
                WriteCleanLine("   [ ⚔️ 무기 및 장비류 ]");
                Console.ResetColor();
                for (int i = 0; i < 4; i++)
                {
                    if (i < items.Count)
                        WriteCleanLine($"    [{(char)('A' + i)}] {items[i].Name,-10} | {items[i].Price,2} Trophy | {items[i].Description}");
                }
                WriteCleanLine("   -------------------------------------------   ");

                // 💣 공격 소모품 (4~7)
                Console.ForegroundColor = ConsoleColor.Red;
                WriteCleanLine("   [ 💣 공격형 소모품 ]");
                Console.ResetColor();
                for (int i = 4; i < 8; i++)
                {
                    if (i < items.Count)
                        WriteCleanLine($"    [{(char)('A' + i)}] {items[i].Name,-10} | {items[i].Price,2} Trophy | {items[i].Description}");
                }
                WriteCleanLine("   -------------------------------------------   ");

                // 🧪 회복 소모품 (8~11)
                Console.ForegroundColor = ConsoleColor.Green;
                WriteCleanLine("   [ 🧪 회복 및 버프류 ]");
                Console.ResetColor();
                for (int i = 8; i < 12; i++)
                {
                    if (i < items.Count)
                        WriteCleanLine($"    [{(char)('A' + i)}] {items[i].Name,-10} | {items[i].Price,2} Trophy | {items[i].Description}");
                }
                WriteCleanLine("   ===========================================   ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                WriteCleanLine("    ℹ️ 구매할 상품의 알파벳(A~L) 입력 | [Q]: 상점 나가기   ");
                Console.ResetColor();
            }
            else if (currentState == GameState.SelectNext)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(" [전투 승리!] 다음 행동을 고르세요 ➡️ 1: 상점 방문 | 2: 다음 전투 속행 \x1b[K");
                Console.ResetColor();
                for (int i = 0; i < 4; i++) WriteCleanLine("");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" [★ 실시간!] 1: 일반공격(4칸) | 2: 인벤토리 열기(시간정지) [남은 적: {remainingMonsters}마리] \x1b[K");
                Console.ResetColor();
            }

            // =================================================================
            // 💡 [최종 버그 해결]: 상태에 따라 출력할 로그 목록(foreach)을 완전히 분리합니다!
            // =================================================================

            // ⚔️ Case 1. 일반 전투 상태 또는 조준 모드일 때 (순수한 전투 로그만 출력)
            if (currentState == GameState.Battle || isTargetingMode)
            {
                WriteCleanLine("======================= 배틀 로그 =======================");

                foreach (var log in battleLogs)
                {
                    if (log.Contains("[CRITICAL!!!]") || log.Contains("치명타!"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(log + " \x1b[K");
                        Console.ResetColor();
                    }
                    else if (log.Contains("회피했습니다!"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(log + " \x1b[K");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(log + " \x1b[K");
                    }
                }
            }
            // 🏪 Case 2. 상점 화면 상태일 때 (전투 흔적을 싹 가리고 상점 관련 영수증만 출력!)
            else if (currentState == GameState.Shop)
            {
                WriteCleanLine("======================= 상점 영수증 =======================");

                foreach (var log in battleLogs)
                {
                    // 💡 핵심: 큐에 든 전체 로그 중 '구매'나 '입장' 키워드가 든 상점용 텍스트만 쏙 골라내서 출력합니다!
                    // 이 필터링 덕분에 박쥐 빌리에게 준 피해 같은 전투 흔적이 화면에 절대 나오지 못합니다.
                    if (log.Contains("구매 완료") || log.Contains("구매 성공") || log.Contains("입장했습니다"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(log + " \x1b[K");
                        Console.ResetColor();
                    }
                }
            }
            // ⚙️ Case 3. 그 외의 상태 (인벤토리, 선택창 등 시스템 안내 출력)
            else
            {
                WriteCleanLine("======================= 시스템 로그 =======================");

                foreach (var log in battleLogs)
                {
                    // 전투나 상점 로그가 아닌 시스템 일반 알림만 제한적으로 출력
                    if (log.Contains("승리") || log.Contains("선택"))
                    {
                        Console.WriteLine(log + " \x1b[K");
                    }
                }
            }

            // 💡 [마지막 방어벽]: 칠판 밑바닥 청소용 공백 처리 (이전 동일)
            if (currentState == GameState.Shop || currentState == GameState.SelectNext)
            {
                for (int i = 0; i < 8; i++)
                {
                    WriteCleanLine("");
                }
            }

        } // 💡 RenderScreen 함수 전체가 끝나는 닫는 중괄호

        // 💡 절대 깨지지 않는 무적의 우측 잔상 지우개 헬퍼 함수
        private static void WriteCleanLine(string text)
        {
            Console.WriteLine(text + " \x1b[K");
        }
    } // UIManager 클래스가 끝나는 중괄호
} // namespace가 끝나는 중괄호