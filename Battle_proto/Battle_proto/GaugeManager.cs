using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public static class GaugeManager
    {
        public static void UpdateAtb(List<Character> characters, System.Action<string> onMonsterAttack)
        {
            foreach (var character in characters)
            {
                if (character.Hp <= 0) { character.AtbGauge = 0; continue; }

                if (character.AtbGauge < 100)
                {
                    character.AtbGauge += character.Speed * 0.1;
                    if (character.AtbGauge > 100) character.AtbGauge = 100;
                }

                // 몬스터 게이지 100 도달 시 즉시 공격 트리거 발동
                if (!character.IsPlayer && character.AtbGauge >= 100)
                {
                    Character player = characters.Find(c => c.IsPlayer);

                    // 몬스터에게 지정된 고유 스킬 이벤트를 실시간 트리거!
                    character.Skill?.Invoke(character, player);

                    character.AtbGauge = 0;
                }
            }
        }
    }
}
