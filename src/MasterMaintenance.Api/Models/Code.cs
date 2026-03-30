namespace MasterMaintenance.Api.Models;

public class Code
{
    public int Id { get; set; }                            // PK, 自動採番
    public int CodeTypeId { get; set; }                    // FK → CodeTypes.Id
    public string Value { get; set; } = string.Empty;      // コード値（DEPT_01 等）
    public string Name { get; set; } = string.Empty;       // コード名
    public int DisplayOrder { get; set; }                  // 表示順
    public bool IsActive { get; set; }                     // 有効/無効
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ナビゲーションプロパティ
    public CodeType CodeType { get; set; } = null!;
}
