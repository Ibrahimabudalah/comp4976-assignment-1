---
mode: agent
description: Audit the solution against the assignment spec and marking criteria, and write up whatever is left
---

Check the code against `${input:spec:path to the assignment spec}` and the plan document
`${input:planDoc:path to the plan document}`, and confirm that every requirement has been
met. If something hasn't, write the remaining tasks out, with their context, in the plan
document.

Origin: written from the prompts used before submission, for example:
- "check back with the progress log to make sure nothing has been skipped. also make sure to
  check for extra use cases and what hasn't been checked to ensure no marks can be lost from
  the criteria"
- "ok check with the A2 documentation and A2 spec and see that all requirements have been
  fufilled. if not, write the remaining tasks out with its context in the documentation"
- "ok what's left"

## How to audit

1. Go through the spec section by section (multi-tenancy, admin, public site, privacy, API,
   database, auth, Aspire, deficiencies, validation, UI, logging, Copilot, deliverables).
   Turn each "must" into one checklist row.
2. For each row, find the evidence in the code or in a test that was actually run. Mark it
   ✅ met, ⚠️ met but a marker could read it differently, or ❌ not done. Don't mark something
   met from memory: open the file or run the request.
3. Compare every phase's Check list in the progress log with what was really tested. Test the
   gaps, plus extra use cases a marker might try: editing the URL or API request to reach
   another tenant, a new tenant with no data, unknown slugs, outages, and phone width.
4. Check the marking guide category by category for anything that could still lose marks.

## Write-up

- Add a dated section to the plan document with the checklist table (requirement, status,
  evidence).
- List the remaining tasks in priority order, split by owner (me / partner). For each task
  give: why it matters (which marks), what exactly to do, how long it should take, and a
  "Done when" condition.
- For gray areas, recommend either a code change or an explanation in the documentation,
  whichever is cheaper this close to the deadline.
- Do not change any code during the audit.
