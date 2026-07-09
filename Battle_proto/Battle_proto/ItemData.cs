namespace RealtimeAtbRpg
{
    // 1. 아이템 타입 정의
    public enum ItemType
    {
        Equipment,  // 장비
        Consumable  // 소모품
    }

    // 2. HP 회복/버프 효과의 종류 정의 (기획서의 Full, (Max)+10 대응)
    public enum HpEffectType
    {
        None,           // HP 관련 효과 없음 (무기 등)
        FixedValue,     // 고정 수치 회복 (물약: 30, 50)
        FullRecovery,   // 전액 회복 (대형 물약: Full)
        MaxHpIncrease   // 최대 체력 증가 (신성초: Max+10)
    }

    public class ItemData
    {
        // 기획서 컬럼 매칭
        public int Id { get; set; }                 // ID
        public ItemType Type { get; set; }         // Type (장비 / 소모품)
        public string Name { get; set; }           // 이름
        public string Description { get; set; }    // 설명

        // HP 효과 처리용
        public HpEffectType HpType { get; set; }   // HP 효과 종류
        public int HpValue { get; set; }           // HP 효과 수치 (30, 50, 10 등)

        public int MoveCost { get; set; }          // MoveCost
        public int Damage { get; set; }            // Damage (물약이나 신성초는 0)
        public int Price { get; set; }             // Price

        // 비고란의 특수 효과 처리를 위한 속성 (필요 시 확장)
        public string Remarks { get; set; }        // 비고

        // 생성자
        public ItemData(int id, ItemType type, string name, string description,
                        HpEffectType hpType, int hpValue, int moveCost, int damage, int price, string remarks = "")
        {
            Id = id;
            Type = type;
            Name = name;
            Description = description;
            HpType = hpType;
            HpValue = hpValue;
            MoveCost = moveCost;
            Damage = damage;
            Price = price;
            Remarks = remarks;
        }
    }
}