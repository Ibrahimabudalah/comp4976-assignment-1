---
mode: agent
description: Implement one phase of the MEMS Assignment 2 implementation plan, test it, log it, and propose a commit message
---

Implement **phase ${input:phase:phase number, e.g. 3}** of the implementation plan in
`${input:planDoc:path to the plan document}` (the "Implementation Plan" section). Follow the
project rules in `.github/copilot-instructions.md`.

Origin: written from the prompts used while building MEMS.Public, for example:
- "ok let's start with phase 1 of the implementation plan, give me a commit message afterwards
  summarizing the changes so that i can review myself"
- "and you've tested the functionality of this phase?"
- "ok next phase, also keep a note to document changes and coding design choices as long you
  work along the tasks"

## Before writing code

1. Read the phase's steps, its "Check" list, and the "Facts you need" section of the plan.
   Read the code each step touches.
2. List every file the phase must change, and anything it could break: callers, other
   tenants' data, the voting guide's privacy rules.
3. If a step depends on a partner's project (AppHost, Api, Data, Admin), do not edit their
   code. Note the blocker instead.

## While implementing

- Use only `/api/{slug}/...` from the public site. Never send voting-guide selections to the
  API, and never store them anywhere except browser sessionStorage.
- Reuse existing services and components before adding new ones. No new packages unless the
  phase requires them.
- Keep Bootstrap and mobile-first layout, and give friendly error messages (never raw
  exception text).

## Test before saying it's done

- Build the solution with no new warnings.
- Run the app and go through **every item in the phase's Check list**, in a real browser.
  Include edge cases: an unknown slug, a record from another tenant, an empty tenant, the API
  being down, and phone width.
- Say plainly what was tested, what passed, and what could not be tested and why.

## Afterwards

1. Add the phase to the plan document's **Progress Log**: what was done, the design choices
   and why, how it was tested, and any known limits.
2. List the files created or changed.
3. Propose a commit message that summarizes the changes, so I can review them before
   committing. Do not commit.
