using Xunit;

namespace SharedKernel.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_HasNoErrorClassification()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Null(result.Error);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public void Conflict_SetsConflictClassification()
    {
        var result = Result.Conflict("An account with this email already exists.");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal("An account with this email already exists.", result.Error);
    }

    [Fact]
    public void Unauthorized_SetsUnauthorizedClassification()
    {
        var result = Result.Unauthorized<int>("Invalid email or password.");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public void FailureFromResult_PreservesClassificationAcrossTypes()
    {
        Result original = Result.Unauthorized<int>("bad creds");

        var propagated = Result.Failure<string>(original);

        Assert.True(propagated.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, propagated.ErrorType);
        Assert.Equal("bad creds", propagated.Error);
    }

    [Fact]
    public void ValidationFailure_CarriesFieldLevelErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = ["Email is required."],
            ["Password"] = ["Password is too short."]
        };

        var result = Result.ValidationFailure(errors);

        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Equal(["Email is required."], result.ValidationErrors["Email"]);
        Assert.Equal(["Password is too short."], result.ValidationErrors["Password"]);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result.Failure<string>("nope");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Value_OnSuccess_ReturnsValue()
    {
        var result = Result.Success("ok");

        Assert.Equal("ok", result.Value);
    }
}
