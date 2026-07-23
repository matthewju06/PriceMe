using System.ComponentModel.DataAnnotations;

namespace PearlMetric.GatewayApi.Validation;

public static class RequestValidator
{
    public static IResult? Validate<T>(T model) where T : class
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        if (Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            return null;
        }

        var errors = results
            .SelectMany(result =>
            {
                var members = result.MemberNames.Any()
                    ? result.MemberNames
                    : ["request"];

                return members.Select(member => (Member: member, Error: result.ErrorMessage ?? "Invalid value."));
            })
            .GroupBy(item => item.Member, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Error).Distinct().ToArray());

        return Results.ValidationProblem(errors);
    }
}
