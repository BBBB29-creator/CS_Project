using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    // 몬스터에게 스킬을 심어주기 위해 스탯 모델 클래스에 딱 한 줄만 추가합니다.
    // (Character 클래스 내부에 public Action<Character, Character> Skill { get; set; } 추가 필요)

    public static class WaveManager
    {
        public static Queue<Character> MonsterWave { get; } = new Queue<Character>();

        public static void InitializeWave()
        {
            // 1. 초록 슬라임: 평범한 몸통 박치기 스킬
            var slime = new Character("초록 슬라임", 140, 20, false, 0.05, 0.10);
            slime.Skill = (self, target) => {
                string log = BattleCore.ProcessAttack(self, target); // 일반 공격 메커니즘 활용
                Program.AddLog(log);
            };
            MonsterWave.Enqueue(slime);

            // 2. 박쥐 빌리: 공격하면서 동시에 자신의 피를 흡혈하는 스킬!
            var bat = new Character("박쥐 빌리", 135, 35, false, 0.10, 0.30);
            bat.Skill = (self, target) => {
                target.Hp = Math.Max(0, target.Hp - 10); // 용사에게 10 피해
                self.Hp = Math.Min(self.MaxHp, self.Hp + 8); // 박쥐 피 8 흡혈!
                Program.AddLog($"🦇 {self.Name}의 흡혈 킥! -> [용사]에게 10의 피해를 주고 자신의 HP를 8 회복!");
            };
            MonsterWave.Enqueue(bat);

            // 3. 고블린 정찰병: 용사의 ATB 게이지를 깎아버리는 기분 나쁜 스킬!
            var goblin = new Character("고블린 정찰병", 155, 28, false, 0.15, 0.15);
            goblin.Skill = (self, target) => {
                target.Hp = Math.Max(0, target.Hp - 12);
                target.AtbGauge = Math.Max(0, target.AtbGauge - 30); // 용사 ATB 게이지 3칸 증발!
                Program.AddLog($"🏹 {self.Name}의 모래 뿌리기! -> 피해 12를 주고 [용사]의 ATB 게이지를 30% 깎았습니다!");
            };
            MonsterWave.Enqueue(goblin);

            // 4. 사막 전갈: 치명타 확률이 엄청나게 높은 독침 공격 스킬
            var scorpion = new Character("사막 전갈", 160, 22, false, 0.50, 0.05); // 치명타 50%
            scorpion.Skill = (self, target) => {
                string log = BattleCore.ProcessAttack(self, target);
                Program.AddLog($"🦂 {self.Name}이 집게를 휘두릅니다!\n" + log);
            };
            MonsterWave.Enqueue(scorpion);

            // 5. 오크 전사: 자신의 피가 적을수록 대미지가 폭발하는 격노 스킬
            var orc = new Character("오크 전사", 190, 18, false, 0.25, 0.05);
            orc.Skill = (self, target) => {
                int bonusDamage = (self.Hp < self.MaxHp * 0.5) ? 22 : 12; // 피가 50% 이하이면 대미지 대폭 상승
                target.Hp = Math.Max(0, target.Hp - bonusDamage);
                Program.AddLog($"🪓 {self.Name}의 격노 일격! -> [용사]에게 {bonusDamage}의 묵직한 피해!");
            };
            MonsterWave.Enqueue(orc);
        }

        public static string CheckWave(List<Character> characters)
        {
            Character player = characters.Find(c => c.IsPlayer);
            Character monster = characters.Find(c => !c.IsPlayer);

            if (player == null || player.Hp <= 0)
                return "▶ 💀 패배... 용사가 차가운 바닥에 쓰러졌습니다.";

            if (monster != null && monster.Hp <= 0)
            {
                if (MonsterWave.Count > 0)
                {
                    characters.Remove(monster);
                    Character next = MonsterWave.Dequeue();
                    characters.Add(next);
                    return $"📢 [경고] {next.Name}이(가) 전장에 난입했습니다!";
                }
                return "▶ 🎉 최종 승리! 모든 던전의 몬스터를 토벌했습니다!";
            }
            return "";
        }
    }
}
