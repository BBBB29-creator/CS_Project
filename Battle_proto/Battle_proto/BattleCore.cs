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
            // 1. 적의 회피율 체크
            if (CheckDodge(defender))
            {
                return $"💨 {attacker.Name}의 공격! {defender.Name}이(가) 회피했습니다! (Miss)";
            }

            // 💡 [버그 해결 핵심]: 기본 대미지 추출 메커니즘 전면 개편!
            int baseDamage = attacker.Damage;

            // 만약 공격자가 플레이어(IsPlayer)라면, Character 타입을 자식인 Player 타입으로 
            // 안전하게 형변환(Casting)하여 장착된 무기가 반영된 진짜 'Damage' 스탯을 꺼내옵니다!
            if (attacker.IsPlayer && attacker is Player player)
            {
                baseDamage = player.Damage; // 대검 장착 시 110 대미지 자동 주입!
            }

            // 2. 치명타 연산 및 최종 대미지 확정
            bool isCrit = CheckCritical(attacker);
            int finalDamage = isCrit ? (int)(baseDamage * 1.5) : baseDamage;

            // 3. 대상의 체력 차감 (Math.Max 안전장치 적용)
            defender.Hp = Math.Max(0, defender.Hp - finalDamage);

            // 4. 연출 텍스트 리턴
            if (attacker.IsPlayer)
            {
                return isCrit
                    ? $"🔥💥 [CRITICAL!!!] 모험가의 일격! -> {defender.Name}에게 {finalDamage}의 치명상!"
                    : $"⚔️ [모험가] 공격! -> {defender.Name}에게 {finalDamage}의 피해!";
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
