# AATP-2 — Sign in fails with wrong credentials

> Sample ticket for this template suite. Target: [Agile Actors' Testing Playground](https://aatp.vercel.app/).  
> Use with the orchestrator to demonstrate the full pipeline on a journey **not** already automated (successful login already exists — prefer **extend** Login feature/pages, do not re-implement happy path).

## Key

`AATP-2`

## Summary

User cannot sign in with wrong credentials

## Type

Manual test case → automate

## Priority

High

## Target application

https://aatp.vercel.app/

## User story / context

As a visitor  
I want to be blocked when I enter incorrect sign-in details  
So that my account stays protected and I know the attempt failed

## Preconditions

- Application is reachable at the configured BaseUrl
- User is not already signed in
- Invalid credentials are available from configuration / user-secrets / env (not hard-coded in the feature)

## Steps

| # | Action | Expected result |
|---|--------|-----------------|
| 1 | Open the sign in page | Sign in form is displayed |
| 2 | Enter an incorrect email and/or password and submit | Sign-in is rejected |
| 3 | Observe the page | User remains on the sign in experience; an error (or failure feedback) is shown; dashboard is not shown |

## Acceptance criteria

- User can reach the sign in page
- Submitting wrong credentials does **not** navigate to the dashboard
- The user is informed that sign-in failed (visible error / failure message on the sign in page)
- Feature text stays business language (no URLs, selectors, or credential values in Gherkin)

## Test data

| Name | Value / source | Notes |
|------|----------------|-------|
| Invalid email / password | config / user-secrets / env (e.g. dedicated invalid user settings) | Must not use the known-valid playground credentials |
| BaseUrl | `https://aatp.vercel.app` via `appsettings` | Overridable in CI |

## Out of scope

- Successful sign in (already covered by the existing sample scenario)
- Account lockout after N failures
- Password reset, SSO, remember-me
- Empty-field validation only (unless the same wrong-credentials path covers it)

## Suggested tags

`@login` `@negative` `@authentication`

## Attachments (optional)

- [ ] Page HTML / screenshot path: _(optional — reuse known `data-test-id`s on the form: `login-form`, `email-input`, `password-input`, `login-button`; discover error-message test id during Explore)_
