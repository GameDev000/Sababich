# Exercise 2 – Server-Side Display Name Validation

## Problem

In Sababich, the Unity client validates the player's display name using `Trim()` and `IsNullOrEmpty()`. This prevents users from entering an empty name or a name consisting only of spaces.

However, this validation runs only on the client. A user can bypass the Unity interface and send an authenticated HTTP request directly to Unity Cloud Save.

## Test Environment

- Project: Sababich
- Unity Environment: `juniorz-test`
- Test file: `test_display_name_server.py`
- Test type: Direct API integration test

## RED – Before the Fix

The tests were written and executed before implementing server-side validation.

**Test 1 – Valid display name**

Input: `"JuniorZ Tester"`

Expected: HTTP 200 OK

Actual: HTTP 200 OK

Result: **PASS**

**Test 2 – Whitespace-only display name**

Input: `"   "`

Expected: HTTP 400, 403, or 422 (request rejected)

Actual: HTTP 200 OK (data accepted)

Result: **FAIL**

### Conclusion

The server accepted an invalid display name when the request bypassed the Unity client. This demonstrates that the validation is not enforced on the current Cloud Save write path.

The RED phase was completed successfully: 2 tests executed, 1 passed and 1 failed as expected.

Full test output: `red_before_fix.txt`

## GREEN – After the Fix

To be completed after implementing and testing server-side validation.