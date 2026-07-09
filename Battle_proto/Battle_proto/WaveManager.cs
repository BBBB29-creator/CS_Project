using System;
using System.Collections.Generic;

namespace RealtimeAtbRpg
{
    public static class MonsterSkills
    {
        // 1. 일반 공격 (슬라임, 전갈 등)
        public static void NormalAttack(Character self, Character target, string introText = "")
        {
            if (!string.IsNullOrEmpty(introText))
            {
                Program.AddLog(introText);
            }
            string log = BattleCore.ProcessAttack(self, target);
            Program.AddLog(log);
        }

        // 2. 흡혈 공격 (박쥐 등)
        public static void DrainAttack(Character self, Character target, int damage, int healAmount)
        {
            target.Hp = Math.Max(0, target.Hp - damage);
            self.Hp = Math.Min(self.MaxHp, self.Hp + healAmount);
            Program.AddLog($"🦇 {self.Name}의 흡혈 킥! -> [용사]에게 {damage}의 피해를 주고 자신의 HP를 {healAmount} 회복!");
        }

        // 3. ATB 게이지 감소 공격 (고블린 등)
        public static void AtbBurnAttack(Character self, Character target, int damage, double burnAmount)
        {
            target.Hp = Math.Max(0, target.Hp - damage);
            target.AtbGauge = Math.Max(0, target.AtbGauge - burnAmount);
            Program.AddLog($"🏹 {self.Name}의 모래 뿌리기! -> 피해 {damage}를 주고 [용사]의 ATB 게이지를 {burnAmount}% 깎았습니다!");
        }

        // 4. 조건부 격노 공격 (오크 등)
        public static void RageAttack(Character self, Character target, double hpThresholdRatio, int normalDamage, int enragedDamage)
        {
            int actualDamage = (self.Hp < self.MaxHp * hpThresholdRatio) ? enragedDamage : normalDamage;
            target.Hp = Math.Max(0, target.Hp - actualDamage);
            Program.AddLog($"🪓 {self.Name}의 격노 일격! -> [용사]에게 {actualDamage}의 묵직한 피해!");
        }
    }

    public static class WaveManager
    {
        // 원본 몬스터 원형(Prototype) 목록
        private static List<Character> _monsterTemplates = new List<Character>();
        private static Random _random = new Random();

        public static void InitializeWave()
        {
            _monsterTemplates.Clear();

            // 기획서 스탯 반영하여 원형 등록 (이름, HP, 속도, 크리, 회피, 대미지)
            _monsterTemplates.Add(new Character("초록 슬라임", 140, 20, false, 0.05, 0.10, 20) { Skill = (self, target) => MonsterSkills.NormalAttack(self, target) });
            _monsterTemplates.Add(new Character("박쥐 빌리", 135, 35, false, 0.10, 0.30, 35) { Skill = (self, target) => MonsterSkills.DrainAttack(self, target, 10, 8) });
            _monsterTemplates.Add(new Character("고블린 정찰병", 155, 28, false, 0.15, 0.15, 28) { Skill = (self, target) => MonsterSkills.AtbBurnAttack(self, target, 12, 70) });
            _monsterTemplates.Add(new Character("사막 전갈", 160, 22, false, 0.50, 0.05, 22) { Skill = (self, target) => MonsterSkills.NormalAttack(self, target, "🦂 사막 전갈이 집게를 휘두릅니다!") });
            _monsterTemplates.Add(new Character("오크 전사", 190, 18, false, 0.25, 0.05, 18) { Skill = (self, target) => MonsterSkills.RageAttack(self, target, 0.6, 12, 22) });
        }

        // 💡 전장에 무작위 몬스터 2마리를 생성하여 채워주는 핵심 메서드
        public static void SpawnRandomMonsters(List<Character> activeCharacters)
        {
            // 기존에 남아있던 시체나 이전 몬스터 제거 (플레이어만 남김)
            activeCharacters.RemoveAll(c => !c.IsPlayer);

            for (int i = 0; i < 2; i++)
            {
                int randomIndex = _random.Next(_monsterTemplates.Count);
                Character template = _monsterTemplates[randomIndex];

                // 💡 중요: 원본을 복사해서 새 객체로 만들어야 스탯이 꼬이지 않습니다.
                // 알파벳 A, B를 붙여 구분하기 쉽게 만듭니다.
                char suffix = (i == 0) ? 'A' : 'B';
                Character newMonster = new Character($"{template.Name} {suffix}", template.MaxHp, template.Speed, false, template.CritChance, template.DodgeChance, template.Damage)
                {
                    Skill = template.Skill
                };

                activeCharacters.Add(newMonster);
            }
        }

        // 💡 실시간 승리/패배 상태 체크 로직
        public static string CheckWave(List<Character> characters)
        {
            Character player = characters.Find(c => c.IsPlayer);
            if (player == null || player.Hp <= 0)
                return "▶ 💀 패배... 모험가가 차가운 바닥에 쓰러졌습니다.";

            // 전장에 살아있는 몬스터가 0마리라면 이번 판 승리!
            bool isAllMonstersDead = !characters.Exists(c => !c.IsPlayer && c.Hp > 0);
            if (isAllMonstersDead)
            {
                return "▶ 🎉 승리! 전장의 적들을 모두 소탕했습니다! 선택의 기로에 섭니다.";
            }

            return "";
        }
    }
}
