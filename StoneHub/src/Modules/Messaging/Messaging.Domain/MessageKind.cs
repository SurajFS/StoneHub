namespace Messaging.Domain;

// Text or image today; Video is reserved for a later addition (needs a player + poster on the
// client and larger uploads).
public enum MessageKind
{
    Text = 0,
    Image = 1
}
