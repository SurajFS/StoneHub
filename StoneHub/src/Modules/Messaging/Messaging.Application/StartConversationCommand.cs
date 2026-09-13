using MediatR;
using SharedKernel;

namespace Messaging.Application;

// Opens (or returns the existing) chat thread between the caller and a product's owner, then
// returns the conversation id so the client can navigate straight to the thread.
public sealed record StartConversationCommand(Guid CallerId, Guid ProductId) : IRequest<Result<Guid>>;
