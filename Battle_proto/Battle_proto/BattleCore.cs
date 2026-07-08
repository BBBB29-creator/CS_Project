using System;

namespace RealtimeAtbRpg
{
    public static class BattleCore
    {
        private static readonly Random _random = new Random();

        public static bool CheckDodge(Character defender) => _random.NextDouble() < defender.DodgeChance;
        public static bool CheckCritical(Character attacker) => _random.NextDouble() < attacker.CritChance;

        public static string ProcessAttack(Character attacker, Character defender)
        {
            if (CheckDodge(defender))
            {
                return $"💨 {attacker.Name}의 공격! {defender.Name}이(가) 회피했습니다! (Miss)";
            }

            int baseDamage = attacker.IsPlayer ? 20 : 12;
            bool isCrit = CheckCritical(attacker);
            int finalDamage = isCrit ? (int)(baseDamage * 1.5) : baseDamage;

            defender.Hp = Math.Max(0, defender.Hp - finalDamage);

            if (attacker.IsPlayer)
            {
                return isCrit
                    ? $"🔥💥 [CRITICAL!!!] 용사의 일격! -> {defender.Name}에게 {finalDamage}의 치명상!"
                    : $"⚔️ [용사] 공격! -> {defender.Name}에게 {finalDamage}의 피해!";
            }
            else
            {
                return isCrit
                    ? $"🚨 💥 {attacker.Name}의 치명타! -> [용사]에게 {finalDamage}의 치명상!"
                    : $"💥 {attacker.Name}의 기습 공격! -> [용사]에게 {finalDamage}의 피해!";
            }
        }
    }
}
