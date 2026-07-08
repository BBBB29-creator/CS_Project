using System;

namespace RealtimeAtbRpg
{
    public class Character
    {
        public Action<Character, Character> Skill { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Speed { get; set; }
        public double AtbGauge { get; set; }
        public bool IsPlayer { get; set; }
        public double CritChance { get; set; }
        public double DodgeChance { get; set; }

        public Character(string name, int hp, int speed, bool isPlayer, double crit, double dodge)
        {
            Name = name;
            Hp = hp;
            MaxHp = hp;
            Speed = (int)(speed * 0.6); // 속도
            AtbGauge = 0;
            IsPlayer = isPlayer;
            CritChance = crit;
            DodgeChance = dodge;
        }
    }
}
