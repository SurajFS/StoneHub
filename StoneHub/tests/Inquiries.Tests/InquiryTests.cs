using Inquiries.Domain;
using SharedKernel;
using Xunit;

namespace Inquiries.Tests;

public sealed class InquiryTests
{
    private static readonly Guid Seller = Guid.NewGuid();
    private static readonly Guid Wholesaler = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();

    private static Result<Inquiry> CreateValid(decimal quantity = 20) =>
        Inquiry.Create(Seller, Wholesaler, ProductId, "Krishna Marble Idol", "Demo Marble Co", quantity, "Need bulk");

    [Fact]
    public void Create_Valid_SucceedsAsPending()
    {
        var result = CreateValid();

        Assert.True(result.IsSuccess);
        Assert.Equal(InquiryStatus.Pending, result.Value.Status);
        Assert.Equal("Demo Marble Co", result.Value.SellerName);
    }

    [Fact]
    public void Create_SelfInquiry_Fails()
    {
        var self = Guid.NewGuid();

        var result = Inquiry.Create(self, self, ProductId, "X", null, 5, null);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_NonPositiveQuantity_Fails(decimal quantity) =>
        Assert.True(Inquiry.Create(Seller, Wholesaler, ProductId, "X", null, quantity, null).IsFailure);

    [Fact]
    public void Create_BlankMessageAndSeller_NormalizeToNull()
    {
        var result = Inquiry.Create(Seller, Wholesaler, ProductId, "X", "   ", 5, "   ");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Message);
        Assert.Null(result.Value.SellerName);
    }

    [Fact]
    public void Accept_FromPending_Succeeds()
    {
        var inquiry = CreateValid().Value;

        Assert.True(inquiry.Accept().IsSuccess);
        Assert.Equal(InquiryStatus.Accepted, inquiry.Status);
    }

    [Fact]
    public void Decline_FromPending_Succeeds()
    {
        var inquiry = CreateValid().Value;

        Assert.True(inquiry.Decline().IsSuccess);
        Assert.Equal(InquiryStatus.Declined, inquiry.Status);
    }

    [Fact]
    public void Accept_AfterDecline_FailsAndKeepsStatus()
    {
        var inquiry = CreateValid().Value;
        inquiry.Decline();

        var result = inquiry.Accept();

        Assert.True(result.IsFailure);
        Assert.Equal(InquiryStatus.Declined, inquiry.Status);
    }
}
