namespace Inquiries.Application.Dtos;

public sealed record InquiryDto(
    Guid Id,
    Guid SellerId,
    Guid WholesalerId,
    Guid ProductId,
    string ProductTitle,
    string? SellerName,
    decimal Quantity,
    string? Message,
    string Status,
    DateTimeOffset CreatedAt,
    // The caller is a Free wholesaler: Message is withheld until they subscribe.
    bool IsLocked = false);
