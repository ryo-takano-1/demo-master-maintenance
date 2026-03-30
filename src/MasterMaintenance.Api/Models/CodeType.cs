namespace MasterMaintenance.Api.Models;

public class CodeType
{
    public int Id { get; set; }                            // PK, 自動採番
    public string Key { get; set; } = string.Empty;        // 一意キー（DEPT, ROLE 等）
    public string Name { get; set; } = string.Empty;       // 表示名
    public string Color { get; set; } = "secondary";       // バッジ色（Bootstrap色クラス名）
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ナビゲーションプロパティ
    public ICollection<Code> Codes { get; set; } = [];
}
