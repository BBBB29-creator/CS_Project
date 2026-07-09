using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public static class GaugeManager
    {
        // 💡 현재 게임의 상태를 매개변수로 함께 받도록 수정합니다.
        public static void UpdateAtb(List<Character> characters, GameState currentState, Action<string> onMonsterAttack)
        {
            // 💡 핵심: 전투 상태가 아니라면(인벤토리나 상점 중이라면) 시간이 흐르지 않습니다!
            if (currentState != GameState.Battle) return;

            foreach (var character in characters)
            {
                if (character.Hp <= 0) { character.AtbGauge = 0; continue; }

                if (character.AtbGauge < 100)
                {
                    character.AtbGauge += character.Speed * 0.1;
                    if (character.AtbGauge > 100) character.AtbGauge = 100;
                }

                // 몬스터 공격 트리거 (기존 코드 유지)
                if (!character.IsPlayer && character.AtbGauge >= 100)
                {
                    Character player = characters.Find(c => c.IsPlayer);
                    character.Skill?.Invoke(character, player);
                    character.AtbGauge = 0;
                }
            }
        }
    }
}
