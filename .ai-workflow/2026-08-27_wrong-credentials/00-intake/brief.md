# Brief

## Ticket
- Key: AATP-2
- Summary: User cannot sign in with wrong credentials

## Journey (one only)
- Title: Sign in fails with wrong credentials
- Actor: Visitor (not signed in)
- Goal: Be blocked when entering incorrect sign-in details so the account stays protected and the attempt failure is clear

## Preconditions
- Application is reachable at the configured BaseUrl
- User is not already signed in
- Invalid credentials are available from configuration / user-secrets / env (not hard-coded in the feature)

## Business steps
| # | When (action) | Then (observable) |
|---|---------------|-------------------|
| 1 | Open the sign in page | Sign in form is displayed |
| 2 | Enter an incorrect email and/or password and submit | Sign-in is rejected |
| 3 | Observe the page | User remains on the sign in experience; an error (or failure feedback) is shown; dashboard is not shown |

## Acceptance criteria
- User can reach the sign in page
- Submitting wrong credentials does **not** navigate to the dashboard
- The user is informed that sign-in failed (visible error / failure message on the sign in page)
- Feature text stays business language (no URLs, selectors, or credential values in Gherkin)

## Data needs
| Name | Source | Redacted value in artifacts? |
|------|--------|------------------------------|
| Invalid email / password | config / user-secrets / env (dedicated invalid-user settings) | yes |
| BaseUrl | appsettings (`https://aatp.vercel.app`, overridable in CI) | no |

## Out of scope
- Successful sign in (already covered by the existing sample scenario — extend Login feature/pages; do not re-implement happy path)
- Account lockout after N failures
- Password reset, SSO, remember-me
- Empty-field validation only (unless the same wrong-credentials path covers it)

## Suggested tags
`@login` `@negative` `@authentication`

## Open questions (blockers vs nice-to-know)
| # | Question | Blocking? |
|---|----------|-----------|
| 1 | Are dedicated invalid-user settings already defined in config / user-secrets / env? | yes |
| 2 | Exact wording or locator for the failure feedback message (error test id to be confirmed during Explore)? | no |

## Attachments
- page.html: absent
