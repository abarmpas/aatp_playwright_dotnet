# AATP-1 — Successful sign in lands on dashboard

> Sample ticket for this template suite. Target: [Agile Actors' Testing Playground](https://aatp.vercel.app/).  
> Use with the orchestrator to demonstrate Intake → … → MR on the existing Login journey (heavy **reuse** expected).

## Key

`AATP-1`

## Summary

Successful sign in lands the user on the dashboard

## Type

Manual test case → automate

## Priority

High

## Target application

https://aatp.vercel.app/

## User story / context

As a registered user  
I want to sign in to the testing playground  
So that I can browse the product catalogue

## Preconditions

- Application is reachable at the configured BaseUrl
- A valid test user exists (credentials from configuration / user-secrets / env — not hard-coded in the feature)

## Steps

| # | Action | Expected result |
|---|--------|-----------------|
| 1 | Open the sign in page | Sign in form is displayed |
| 2 | Enter valid email and password and submit | User is authenticated |
| 3 | Observe the landing page | Dashboard is displayed (container / products area visible) |

## Acceptance criteria

- User can reach the sign in page
- Signing in with valid credentials navigates to the dashboard
- Dashboard UI for the logged-in home state is visible
- Feature text stays business language (no URLs, selectors, or credentials in Gherkin)

## Test data

| Name | Value / source | Notes |
|------|----------------|-------|
| Email / password | `TestSettings` / user-secrets / env | App publishes demo creds on the page; suite must still read from config |
| BaseUrl | `https://aatp.vercel.app` via `appsettings` | Overridable in CI |

## Out of scope

- Invalid credentials / error messages
- Password reset, SSO, remember-me
- Add to cart or product details after login

## Suggested tags

`@login` `@smoke` `@authentication`

## Attachments (optional)

- [ ] Page HTML / screenshot path: _(optional for demo — Prefer existing `data-test-id`s: `login-form`, `email-input`, `password-input`, `login-button`, `dashboard-container`, `products-grid`)_
