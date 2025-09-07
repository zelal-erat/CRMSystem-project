using CRMSystem.Application.Common;

namespace CRMSystem.Domain.Rules;

public interface IBusinessRule
{
    string ErrorMessage { get; }
    Task<bool> IsValidAsync();
}

public class BusinessRulesValidator
{
    public async Task<Result> ValidateAsync(params IBusinessRule[] rules)
    {
        foreach (var rule in rules)
        {
            if (!await rule.IsValidAsync())
                return Result.Failure(rule.ErrorMessage);
        }
        return Result.Success();
    }

    public async Task<Result> ValidateAsync(IEnumerable<IBusinessRule> rules)
    {
        foreach (var rule in rules)
        {
            if (!await rule.IsValidAsync())
                return Result.Failure(rule.ErrorMessage);
        }
        return Result.Success();
    }
}
