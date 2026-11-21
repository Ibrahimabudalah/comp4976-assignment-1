---
mode: agent
description: Verify a partner's pushed fixes against the handoff list, without changing their code
---

My partner pushed changes that should cover the items in
`${input:handoffDoc:path to the handoff or issues list}`. Check off each item and make sure
each fix is correct and valid. **Do not modify any of their code.**

Origin: written from the prompts used to review the partner's admin portal and API work, for
example:
- "check to see if the admin portal fixes are correct, do not modify any of their code."
- "ok let's review handoff.md, my partner pushed changes that should cover the items. check
  off and ensure its correctness and validity. you can document these results in another
  section in documentation.md"
- "ok my partner pushed new changes. check to see if they have fixed the MAJOR ticket issues"

## How to verify

1. Read the diff of the new commits and match each change to a handoff item.
2. **Run it, don't just read it.** Start the AppHost on a fresh SQL volume and test each item
   the way a marker would:
   - Admin portal: as `Super` and as a tenant admin (e.g. `Burnabyadmin`), in a real browser.
   - API: with real JWT tokens. Check 401 without a token, 403 for another tenant, and 400
     for invalid input.
   - Each item's "Done when" condition, exactly.
3. If a fix needs proving, test it on a copy (for example `git archive HEAD` into a scratch
   folder), never in the partner's files.
4. Look for new problems the changes introduce, not only the listed ones.

## Report

- For each item: ✅ fixed, ⚠️ partly fixed, or ❌ not fixed, with the evidence (command,
  status code, screenshot).
- New problems as tickets: severity, what's wrong, how it was found, the suggested fix, and a
  "Done when" condition.
- Add the results as a new section in the plan document, and update the status of each item
  in the handoff list.
- Say what could not be checked and why.
