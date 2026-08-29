using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Buyers;

public sealed class RegisterBuyerCommandHandler(
    IIdentityService identityService,
    IBuyerProfileRepository buyerProfileRepository,
    IJwtTokenService jwtTokenService) : IRequestHandler<RegisterBuyerCommand, Result<AuthResultDto>>
{
    private const string Role = "Buyer";

    public async Task<Result<AuthResultDto>> Handle(RegisterBuyerCommand request, CancellationToken ct)
    {
        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var buyerProfile = BuyerProfile.Register(userResult.Value, request.DisplayName, request.BuyerType, request.City);

        buyerProfileRepository.Add(buyerProfile);
        await buyerProfileRepository.SaveChangesAsync(ct);

        var token = jwtTokenService.GenerateToken(userResult.Value, request.Email, Role);
        return Result.Success(new AuthResultDto(userResult.Value, request.Email, Role, token));
    }
}
