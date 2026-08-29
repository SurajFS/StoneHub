namespace SharedKernel;

// Classifies an expected failure so a single transport-layer mapper can translate it to
// the right HTTP status without controllers re-deciding per action.
public enum ErrorType
{
    None = 0,
    Failure,      // generic bad request (400)
    Validation,   // input validation, carries field-level errors (400)
    NotFound,     // resource does not exist (404)
    Conflict,     // violates a uniqueness / state invariant (409)
    Unauthorized, // caller is not authenticated / bad credentials (401)
    Forbidden     // authenticated but not allowed (403)
}
