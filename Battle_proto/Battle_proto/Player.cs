using System;

namespace RealtimeAtbRpg
{
    // Character의 기본 스탯을 물려받고, 용사 고유의 기능을 추가합니다.
    public class Player : Character
    {
        public Player(string name, int hp, int speed, double crit, double dodge)
            : base(name, hp, speed, true, crit, dodge) // IsPlayer는 무조건 true
        {
        }

        // [용사 조작 노드] 키보드 입력을 받아 행동을 분기합니다.
        public void HandleInput(char key, Character target, Action<string> addLog)
        {
            if (key == '1' && target != null)
            {
                ExecuteAttack(target, addLog);
            }
            else if (key == '2')
            {
                ExecuteHeal(addLog);
            }
        }

        // [용사 행동 노드 1] 일반 공격
        private void ExecuteAttack(Character target, Action<string> addLog)
        {
            if (this.AtbGauge < 40)
            {
                addLog("⚠️ [시스템] 게이지가 부족합니다! (공격: 최소 4칸 필요)");
                return;
            }

            this.AtbGauge -= 40;

            // BattleCore의 핵심 연산을 그대로 가져와 수행
            string resultLog = BattleCore.ProcessAttack(this, target);
            addLog(resultLog);
        }

        // [용사 행동 노드 2] 자가 회복
        private void ExecuteHeal(Action<string> addLog)
        {
            if (this.AtbGauge < 100)
            {
                addLog("⚠️ [시스템] 게이지가 부족합니다! (회복: 10칸 필요)");
                return;
            }

            int healAmount = 25;
            this.Hp = Math.Min(this.MaxHp, this.Hp + healAmount);
            this.AtbGauge -= 100;

            addLog($"✨ [용사] 힐! -> 자신의 HP를 {healAmount} 회복했습니다.");
        }
    }
}
