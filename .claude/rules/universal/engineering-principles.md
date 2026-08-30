# Universal Engineering Principles

These rules apply to every project, every language, every stack. They are non-negotiable
unless a project's own CLAUDE.md explicitly overrides one of them with a documented reason.

---

## 1. Treat every change as production code

This codebase is maintained by a professional engineering team. No rushed code. No "I'll
clean this up later." No throwaway scripts left in feature branches. If it's committed,
it's production-grade.

Prefer **boring, readable, testable** code over clever code. The job of code is to be
read 100 times for every time it's written.

---

## 2. Strict typing — never `any`, never untyped

Whatever language you're in, use its type system at full strength.

- **TypeScript**: never `any`. Use `unknown` for genuinely untrusted data, then narrow
  with Zod or a type guard before use. Every function parameter, every return type,
  every component prop must be explicitly typed. `as` casts only after schema validation.
- **Go**: avoid `interface{}` / `any` outside genuine generic code. Define real types.
- **Rust**: no `unwrap()` outside tests. Use `?`, `Result`, and proper error types.
- **Python**: type hints on all function signatures. Use `mypy --strict`.
- **Dart**: no `dynamic`. Sound null safety on. Specify generics explicitly.

If you reach for `any` / `interface{}` / `dynamic` / `unwrap()`, you are working around
a design problem. Fix the design instead.

---

## 3. File length — hard limit 250 lines

No source file exceeds **250 lines**. If it approaches the limit, split it. There is
always a way to split.

How to split:
- Extract child components, sub-modules, or helpers into their own files
- One concept per file. One handler per file. One screen-section per file.
- Use a package/folder per feature rather than one giant file

Reasoning: 250 lines is the threshold above which file-level reasoning breaks down
for both humans and AI. Below it, everything stays scannable.

---

## 4. Separation of concerns is strict

These layers do not mix:

```
UI / presentation        → layout, rendering, user input. No business decisions.
Business logic / service → decisions, orchestration, transforms. No fetching, no rendering.
Data access / repository → DB, API, file I/O. No business logic.
Domain types             → pure type definitions. No runtime behavior.
Validation schemas       → Zod / validator tags. Boundary only.
Infrastructure helpers   → logging, config, low-level utilities.
```

A UI component never calls `fetch()`. A handler never writes SQL. A service never
renders. If you find yourself crossing a layer, you're modeling the problem wrong.

---

## 5. No magic values anywhere

- No hardcoded URLs, route paths, or base URLs outside the dedicated config file
- No hardcoded colors, font sizes, spacing, or radii outside the design token file
- No hardcoded secrets, API keys, or credentials — environment variables only
- No magic numbers in business logic — use named constants with documented meaning

If a value appears in more than one place, or could change, name it.

---

## 6. Errors are first-class

Errors are not afterthoughts. They are part of the API of every function.

- **Never silently swallow errors.** No empty catch blocks. No `try { ... } catch {}`.
- **Always add context when propagating errors.** `fmt.Errorf("create user: %w", err)`
  not bare `return err`. Stack traces alone don't tell the operator what was being
  attempted.
- **Return typed errors** for cases callers need to handle specifically.
  Use `errors.Is` / `errors.As` (Go), discriminated unions (TS), `Result<T, E>` (Rust).
  Never string-match error messages.
- **HTTP / RPC handlers translate errors** to appropriate status codes and structured
  response bodies. Never expose internal error details in production responses —
  log them, return a stable error code to the client.
- **No `panic!` / `process.exit` / unhandled rejection** in production code paths.

---

## 7. Validate at every trust boundary

Anywhere data crosses from "untrusted" to "trusted," it must be validated against an
explicit schema:

- **API request bodies** — validator tags (Go) or Zod (TS) on every endpoint
- **API response bodies** — Zod-parse before use, even from your own backend
- **External webhooks** — signature verification + schema validation
- **Database results** that you don't fully control the shape of
- **Form inputs** — validate on submit, surface field-level errors

Validation errors return structured, field-level details so the UI can show errors
inline next to the offending field — not a generic "something went wrong."

---

## 8. No business logic in handlers or components

This is the most-violated rule and the highest-leverage one to enforce.

- **HTTP handlers**: parse input → call service → return response. That's the whole
  handler. If a handler is more than 30 lines, it has business logic in it.
- **UI components**: receive props → render → emit events. If a component is making
  decisions about *what* to do (not just *how* to display), the decision belongs in
  a service, hook, or store.

Test: would this logic still make sense if you changed the transport from HTTP to
gRPC, or the UI from web to mobile? If yes, it belongs in a service, not a handler
or component.

---

## 9. State lives where it belongs, and nowhere else

- **Server data** lives in domain-scoped stores / caches / hooks. Never in
  component-local state.
- **UI state** (which tab is open, which modal is showing) lives in a UI store or
  the URL — not scattered across components.
- **Form state** lives in a form library or a scoped form store, reset on unmount.
- **Bookmarkable state** lives in the URL, not in memory.
- **Secrets and tokens** live in httpOnly cookies or secret managers, never in
  localStorage, sessionStorage, or any client-side store.

One source of truth per piece of state. No duplication, no "syncing."

---

## 10. Concurrency: explicit context, no leaks

Any function that does I/O or could be slow takes a context / cancellation token as
its first parameter. Long-running work happens in background workers (Asynq, Sidekiq,
BullMQ, Celery — whatever the stack uses), never inside HTTP handlers.

- Always pass context through call chains
- Always provide cancellation paths
- Always test concurrent code with race detectors enabled
- Never start fire-and-forget goroutines / promises / tasks in request handlers

---

## 11. Logging is structured, not printf

Use the language's structured logger (`slog` in Go, `pino`/`winston` in Node, `tracing`
in Rust, `structlog` in Python). Never `fmt.Println`, `console.log`, or `println!`
in committed code.

Every log line has:
- A level (debug / info / warn / error)
- A message that's a stable string (no string interpolation in the message itself)
- Key-value attributes for the dynamic data

```
slog.Info("influencer synced", "influencer_id", id, "platform", "youtube", "duration_ms", elapsed)
```

Never log secrets, tokens, passwords, full request bodies that may contain PII, or
full document contents from user uploads.

---

## 12. Tests for risky logic

Not 100% coverage. **Tests where they matter:**

- Business rules with branching logic
- Money / billing / pricing math
- Auth and authorization decisions
- Tenant isolation (in multi-tenant systems)
- Data transforms with edge cases
- Anything you've gotten wrong before

Use table-driven tests for functions with many input variants. Run tests with race
detectors / strict mode enabled. Mock external services, use real DB for integration
tests where possible.

Tests written *after* the implementation by the same author tend to rubber-stamp
whatever the code does. For risky logic, write the test first.

---

## 13. Dependencies are a liability

Every dependency added is a long-term commitment. Before adding one:

1. Can the standard library do this in <50 lines?
2. Is the dependency actively maintained? (commits in last 6 months)
3. What's its transitive dependency tree?
4. Is its license compatible with the project?

Never add a dependency without explicit approval. Justify in the PR description why
an existing dependency or the standard library wouldn't work.

---

## 14. Security defaults

- Never commit secrets. Use `.env` locally, secret managers in production.
- Never log tokens, passwords, OAuth credentials, or full PII.
- Encrypt sensitive data at rest (OAuth tokens, payment details).
- Use parameterized queries always. No string-interpolated SQL.
- Validate authorization on every protected route — don't trust client-supplied IDs.
- In multi-tenant systems, every query is tenant-scoped. No exceptions.
- Rate limit all public endpoints.
- Sanitize any user input used in file paths, shell commands, or rendered HTML.

---

## 15. Generated code is not edited by hand

OpenAPI specs, TypeScript types generated from schemas, ORM bindings, GraphQL types —
if a file says "auto-generated" at the top, do not edit it. Regenerate it from source.

If you need to change the output, change the source (annotations, schema, model) and
re-run the generator.

---

## 16. Lint and typecheck pass before "done"

A task is not complete until:
- Linter passes with zero warnings (no `// eslint-disable` without documented reason)
- Type checker passes with zero errors
- Tests pass
- The change builds in CI configuration, not just locally

If the linter complains, fix the underlying issue. Don't disable the rule.

---

## 17. Commits are atomic, messages are real

- One logical change per commit. No "WIP" or "fixes" merged into main.
- Commit messages explain *why*, not *what* (the diff shows what).
- Reference the issue / ticket when relevant.
- Squash before merge.

---

## 18. Never silently improvise on requirements

If a spec, design, or rule is ambiguous, ask. Do not invent an interpretation and ship
it without flagging that you made a choice.

If you decide to deviate from the established pattern (for performance, for clarity,
for any reason), call that out explicitly in the PR so a reviewer can sign off on it.

The worst class of bug is the one introduced confidently and silently.

---

## 19. Async style

Use `async` / `await` everywhere. No `.then(...).catch(...)` chains in primary
flow. The only acceptable `.catch` is on a deliberately fire-and-forget side
effect that must not break the surrounding loop or caller — and that case gets
a comment explaining why.

Every async function has an explicit return type (`Promise<T>`), never an
inferred `Promise<any>`. Don't leave floating promises — `await` them, or assign
them to a variable you `await` later.

Use sequential `for...of` with `await` when ordering matters or when downstream
rate limits apply. Use `Promise.all` only when the operations are truly
independent and parallel-safe.

---

## 20. Error handling — top-level try/catch, throw inside

Each entrypoint (HTTP handler, Lambda handler, job handler, CLI command) wraps
its body in **one** top-level `try` / `catch`. Every function it calls is
inside that `try`.

Inner functions **throw on failure** — they do not return error tuples, do not
swallow errors locally, do not wrap every call in its own try/catch. If a
function returns normally, the caller treats it as success and moves on.

```ts
export const main = async (event) => {
  try {
    const input = validateInput(event)        // throws on bad input
    const data  = await fetchData(input)      // throws on fetch failure
    const result = await process(data)        // throws on process failure
    await persist(result)                     // throws on write failure
    return { statusCode: 200, body: ... }
  } catch (err) {
    logger.error(err, 'handler failed')
    if (err instanceof CustomError) return { statusCode: err.statusCode, ... }
    return { statusCode: 500, ... }
  }
}
```

The `catch` block is the **only** place that decides how to respond to failure
(status code, error shape, alerting). Don't duplicate that logic per call.

Exceptions to the rule are deliberate, not accidental:
- A utility may catch and return an empty result when the surrounding flow
  should continue (e.g., "one venue fails, others keep processing"). When you
  do this, leave a comment saying why — otherwise the next reader will assume
  it's a missing throw.
- A fire-and-forget side effect (logging, optional S3 upload) may use a
  scoped `.catch` to log + suppress.

Use typed error classes (e.g. `CustomError`) with a `statusCode` and a `name`
so the top-level `catch` can map known cases to known responses and treat
everything else as a 500.

---

## 21. Functional approach preferred

Prefer functions and immutable data over classes and mutation.

- Reach for `function` / `const x = () => ...` first; only introduce a class
  when there's clear shared state, lifecycle, or polymorphism that a closure
  doesn't express more cleanly.
- Build new objects with spread + override (`{ ...old, field: newValue }`)
  rather than mutating in place. Same for arrays — `[...arr, item]`, `arr.filter(...)`,
  not `arr.push` / `splice` on shared data.
- Use `map`, `filter`, `reduce`, and small named pure helpers over imperative
  accumulation when the transformation is the point. Use a `for...of` loop
  when ordering, early exit, or sequential `await` is the point — that's still
  fine, "functional" doesn't mean "no loops."
- Side effects (I/O, logging, persistence) belong at the edges, not interleaved
  through pure transformation code. Compute first, write once.
- No hidden global state. Pass dependencies in; don't reach for module-level
  mutable singletons.

---

## 22. UI responsiveness is felt, not measured

The "feels slow" of a UI is almost never raw performance. It's whether the
UI acknowledges what the user did, how fast it acknowledges it, and what it
shows them while it works. Solve these and even a slow app feels fast. Skip
them and even a fast app feels broken.

This principle is platform- and framework-agnostic. A Flutter app, a React
Native app, a SwiftUI app, a web app — none of them feel slow when these
are right, and none of them feel fast when they're missing. **The fix is
the design discipline, not the framework.** Switching stacks to "make it
feel faster" without changing these habits reproduces the same feel
exactly.

### Non-negotiables for every interactive element

- **Visible press feedback within ~100ms.** A tap that doesn't visibly
  respond instantly registers as broken. Scale (0.96–0.97) + haptic on
  touch-down, spring back on release. This applies even when the actual
  work hasn't started yet — the acknowledgement is independent of the
  outcome.
- **Haptic feedback on every meaningful interaction.** Light impact for
  selections and taps, medium for state changes and confirmations, heavy
  for errors and celebrations. Phones and tablets all support this; it's
  free responsiveness signal that costs nothing to ship.
- **Animated state changes — never hard cuts.** If a value changes, it
  animates. Implicit animation widgets (`AnimatedContainer`,
  `AnimatedOpacity`, `AnimatedSwitcher`, or the framework's equivalent)
  are the default; explicit controllers only when sequencing demands it.
- **Skeletons, not spinners.** A spinner says "wait." A skeleton that
  matches the shape of the eventual content says "loading content like
  this." Massive perception difference for the same wait. Generic gray
  rectangles count as half-done — match the actual content shape.
- **Optimistic UI for safe operations.** Toggle a switch and see it move
  immediately; reconcile on network response. Don't make the user wait
  for a round-trip when the operation is idempotent and recoverable on
  failure.
- **Explicit page transitions.** Shared-axis, fade-through, or a
  platform-appropriate slide — never the framework's default jerk. The
  transition between screens is part of the design system, not an
  afterthought.

### Tokens, never inline values

- All durations and curves are named tokens. No `Duration(milliseconds: 300)`
  and no `Curves.ease` inline in feature code. They live in
  `design_system/motion.dart` (or the framework's equivalent) and are
  referenced by name.
- Default curve is springy, not linear. `easeOutBack`, `easeOutExpo`,
  `elasticOut`, or a custom spring. `linear` and default `ease` feel inert.
- Press scale, ripple opacity, shadow elevation — all tokens. Inline
  magic numbers in widgets are a design-discipline failure, not a style
  preference.

### Operational test

Open the app. Tap five things at random. If any tap doesn't visibly
respond within ~100ms, or doesn't trigger a haptic, you haven't shipped
this principle yet — regardless of how fast the API is.
