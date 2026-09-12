using Catalog.Application;
using Identity.Application;
using MediatR;
using Messaging.Domain;
using SharedKernel;

namespace Messaging.Application;

public sealed class StartConversationCommandHandler(
    IConversationRepository conversations,
    IProductLookup productLookup,
    IUserDirectory userDirectory) : IRequestHandler<StartConversationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(StartConversationCommand request, CancellationToken ct)
    {
        var product = await productLookup.GetOwnerInfoAsync(request.ProductId, ct);
        if (product is null || !product.IsActive)
            return Result.NotFound<Guid>("Product not found.");
        if (product.OwnerId == request.CallerId)
            return Result.Failure<Guid>("You cannot chat about your own listing.");

        var caller = await userDirectory.GetAsync(request.CallerId, ct);
        if (caller is null)
            return Result.NotFound<Guid>("Your profile was not found.");
        var owner = await userDirectory.GetAsync(product.OwnerId, ct);
        if (owner is null)
            return Result.NotFound<Guid>("The listing owner was not found.");

        if (!ChatPolicy.CanConverse(caller.Role, owner.Role))
            return Result.Forbidden<Guid>("Chat isn't available between these accounts.");

        // Idempotent: one thread per pair, regardless of who opens it or from which listing.
        var existing = await conversations.GetByParticipantsAsync(caller.UserId, owner.UserId, ct);
        if (existing is not null)
            return Result.Success(existing.Id);

        var conversation = Conversation.Start(
            caller.UserId, caller.Name, caller.AvatarUrl,
            owner.UserId, owner.Name, owner.AvatarUrl,
            request.ProductId, product.Title);

        conversations.Add(conversation);
        await conversations.SaveChangesAsync(ct);

        return Result.Success(conversation.Id);
    }
}
