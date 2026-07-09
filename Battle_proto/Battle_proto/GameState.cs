// GameState.cs 파일 내용
namespace RealtimeAtbRpg
{
    public enum GameState
    {
        Battle,      // 전투 중
        Inventory,   // 인벤토리 창
        SelectNext,  // 💡 전투 승리 후 [1: 상점 / 2: 다음 전투] 선택 대기 상태
        Shop         // 💡 상점 창
    }
}