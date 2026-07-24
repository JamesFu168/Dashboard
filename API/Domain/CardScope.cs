namespace Dashboard.Api.Domain;

/// <summary>
/// 表示看板卡片的可見範疇。
/// </summary>
public enum CardScope
{
    /// <summary>
    /// 個人卡片 (僅卡片擁有者可見)
    /// </summary>
    Personal = 0,

    /// <summary>
    /// 組織/部門卡片 (同部門成員與細項被指派人可見)
    /// </summary>
    Organization = 1
}
