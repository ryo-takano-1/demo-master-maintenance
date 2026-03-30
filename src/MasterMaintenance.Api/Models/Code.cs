namespace MasterMaintenance.Api.Models;

/// <summary>
/// コードマスタのエンティティ。テーブル: Codes
/// </summary>
public class Code
{
    /// <summary>コード ID（int, PK, 自動採番）</summary>
    public int Id { get; set; }

    /// <summary>コード種別 ID（int, FK → CodeTypes.Id）</summary>
    public int CodeTypeId { get; set; }

    /// <summary>コード値（string, 必須, max:50, unique, 例: "DEPT_01"）</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>コード名（string, 必須, max:100, 例: "営業部"）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順（int）</summary>
    public int DisplayOrder { get; set; }

    /// <summary>有効フラグ（bool）</summary>
    public bool IsActive { get; set; }

    /// <summary>作成日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時（DateTime, UTC）</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>コード種別（CodeType, ナビゲーションプロパティ）</summary>
    public CodeType CodeType { get; set; } = null!;
}
