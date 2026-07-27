namespace PaymentsMatchingTool.Domain;

public enum MatchStatus
{
    Matched,
    OnlySystem,
    OnlyProvider,
    AmountMismatch
}
