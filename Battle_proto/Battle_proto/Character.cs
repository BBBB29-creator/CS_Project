namespace RealtimeAtbRpg
{
    public class Character
    {
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Speed { get; set; }
        public bool IsPlayer { get; set; }
        public double CritChance { get; set; }
        public double DodgeChance { get; set; }
        public double AtbGauge { get; set; }
        public Action<Character, Character> Skill { get; set; }

        // 💡 여기에 기획서 스탯을 반영할 공격력(Damage) 변수를 추가합니다!
        public int Damage { get; set; }

        // 생성자 수정
        public Character(string name, int hp, int speed, bool isPlayer, double crit, double dodge, int damage)
        {
            Name = name;
            Hp = hp;
            MaxHp = hp;
            Speed = speed;
            IsPlayer = isPlayer;
            CritChance = crit;
            DodgeChance = dodge;
            Damage = damage;
            AtbGauge = 0;
        }
    }
}
