using System;

namespace RealtimeAtbRpg
{
    // BattleCore.cs 파일 내부라고 가정
    public static class BattleCore
    {
        //public static Queue<Character> MonsterWave = new Queue<Character>();
        private static readonly Random _random = new Random();

        public static bool CheckDodge(Character defender) => _random.NextDouble() < defender.DodgeChance;
        public static bool CheckCritical(Character attacker) => _random.NextDouble() < attacker.CritChance;

        public static string ProcessAttack(Character attacker, Character defender)
        {
            if (CheckDodge(defender))
            {
                return $"💨 {attacker.Name}의 공격! {defender.Name}이(가) 회피했습니다! (Miss)";
            }

            // 💡 [수정] 고정 숫자 대신 캐릭터가 가진 고유의 Damage 스탯을 기본값으로 사용합니다!
            int baseDamage = attacker.Damage;

            bool isCrit = CheckCritical(attacker);
            int finalDamage = isCrit ? (int)(baseDamage * 1.5) : baseDamage;

            defender.Hp = Math.Max(0, defender.Hp - finalDamage);

            if (attacker.IsPlayer)
            {
                return isCrit
                    ? $"🔥💥 [CRITICAL!!!] 모험가의 일격! -> {defender.Name}에게 {finalDamage}의 치명상!"
                    : $"⚔️ [용사] 공격! -> {defender.Name}에게 {finalDamage}의 피해!";
            }
            else
            {
                return isCrit
                    ? $"🚨 💥 {attacker.Name}의 치명타! -> [모험가]에게 {finalDamage}의 치명상!"
                    : $"💥 {attacker.Name}의 기습 공격! -> [모험가]에게 {finalDamage}의 피해!";
            }
        }
    }
}
