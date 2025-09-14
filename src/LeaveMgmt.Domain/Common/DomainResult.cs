namespace LeaveMgmt.Domain.Common;

public readonly struct DomainResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    private DomainResult(bool ok, string? err) { IsSuccess = ok; Error = err; }
    public static DomainResult Success() => new(true, null);
    public static DomainResult Fail(string error) => new(false, error);
}

public readonly struct DomainResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    private DomainResult(bool ok, T? value, string? err) { IsSuccess = ok; Value = value; Error = err; }
    public static DomainResult<T> Success(T value) => new(true, value, null);
    public static DomainResult<T> Fail(string error) => new(false, default, error);
}
