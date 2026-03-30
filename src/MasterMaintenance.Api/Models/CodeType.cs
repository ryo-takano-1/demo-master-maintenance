namespace MasterMaintenance.Api.Models;

/// <summary>
/// コード種別マスタのエンティティ。テーブル: CodeTypes
/// </summary>
public class CodeType
{
    /// <summary>コード種別 ID（int, PK, 自動採番）</summary>
    public int Id { get; set; }

    /// <summary>種別キー（string, 必須, max:50, unique, 大文字英字+アンダースコア, 例: "DEPT"）</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>種別名（string, 必須, max:100, 例: "部門"）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>バッジ色（string, 必須, max:20, Bootstrap 色クラス名, 既定: "secondary"）</summary>
    public string Color { get; set; } = "secondary";

    /// <summary>作成日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時（DateTime, UTC）</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>配下のコード一覧（ICollection&lt;Code&gt;, ナビゲーションプロパティ）</summary>
    public ICollection<Code> Codes { get; set; } = [];
}
