Feature: Login
  As a registered user
  I want to sign in to the testing playground
  So that I can browse the product catalogue

  Scenario: Successful sign in lands the user on the dashboard
    Given the user is on the sign in page
    When the user signs in with valid credentials
    Then the dashboard is displayed

  @login @negative @authentication
  Scenario: Sign in fails with wrong credentials
    Given the user is on the sign in page
    When the user signs in with invalid credentials
    Then sign in failure is shown
    And the user remains on the sign in page
    And the dashboard is not displayed
