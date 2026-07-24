namespace Dashboard.Api.Domain;

/// <summary>
/// 表示看板卡片的狀態位置。
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// 規劃中
    /// </summary>
    Plan = 0,

    /// <summary>
    /// 待辦事項
    /// </summary>
    ToDo = 1,

    /// <summary>
    /// 進行中
    /// </summary>
    Doing = 2,

    /// <summary>
    /// 已完成
    /// </summary>
    Done = 3,

    /// <summary>
    /// 自動結案 (前一個月已完成之卡片自動轉入此狀態，預設不顯示於看板)
    /// </summary>
    AutoClosed = 4
}
