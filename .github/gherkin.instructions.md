---
applyTo: '**/*.feature'
---
# Feature File & Gherkin Guidelines
Rule: Each scenario must describe a single business behavior. Do not mix unrelated checks.
Rule: Keep scenarios to approximately 3-9 steps. If a scenario becomes too long, split or refactor it.
Rule: Write scenarios so that a non-technical stakeholder can understand the system behavior without code context.
Rule: Use concrete, example-driven outcomes (e.g., "Then the participation button is visible" not "Then the state has changed").
Rule: Scenarios must be independent and order-agnostic. Required state should be set up in Given steps or via hooks.
Rule: Do not express conditional logic (if/else/either-or) in scenarios. Write separate explicit scenarios for each branch.
Rule: Use business/domain language in step text, not technical detail (e.g., "Given the user has a settled winning bet" not "Given the user has a record in the Bets table with status = WON").
Rule: Do not include locators, element types, CSS, or XPath in Gherkin step text.
Rule: Use Scenario Outline with examples when behavior is the same and only data values change.
Rule: Prefer one feature file per user journey or business capability. Split large files by sub-capability.
Rule: Use Background only for common, stable prerequisites that apply to every scenario in the file.

# Given / When / Then Semantics
Rule: Given steps are for setup and state only — no assertions or primary business actions.
Rule: When steps represent the core business action under test. Prefer one primary When per scenario; use And for any subsequent actions within the same phase rather than repeating the When keyword.
Rule: Then steps are for observable, verifiable outcomes only — no additional actions or side effects.
Rule: And / But are continuation keywords that extend the preceding Given, When, or Then — never use them to introduce a new phase. Consecutive steps of the same type must use And, not the keyword repeated (e.g., When → And, Given → And, Then → And).
