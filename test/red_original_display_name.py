"""JuniorZ exercise 2 — RED test and pre-fix valid-name baseline.

Run from the repository root:
    py test/test_display_name_server.py

This test uses only the Python standard library. It talks directly to Unity
Authentication and Cloud Save in the isolated juniorz-test environment.
It does not use the Unity game's UI, and it creates a throwaway test player.
"""

import json
import os
import unittest
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

# Taken from ProjectSettings/ProjectSettings.asset in the Sababich repository.
# Verify it is also the Project ID shown in Unity Dashboard.
PROJECT_ID = os.environ.get("UNITY_PROJECT_ID", "35da4438-78cb-4c10-a72a-984418a8a547")
ENVIRONMENT = "juniorz-test"
AUTH_URL = "https://player-auth.services.api.unity.com/v1/authentication/anonymous"
CLOUD_SAVE_BASE = "https://cloud-save.services.api.unity.com/v1/data/projects"


def post_json(url, payload, headers):
    request = Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json", **headers},
        method="POST",
    )
    try:
        with urlopen(request, timeout=20) as response:
            return response.status, response.read().decode("utf-8")
    except HTTPError as error:
        return error.code, error.read().decode("utf-8", errors="replace")


class DisplayNameServerValidationTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        if ENVIRONMENT != "juniorz-test":
            raise RuntimeError("Safety check: tests must run in juniorz-test only")

        status, body = post_json(
            AUTH_URL,
            {},
            {"ProjectId": PROJECT_ID, "UnityEnvironment": ENVIRONMENT},
        )
        if status != 200:
            raise RuntimeError(
                "Could not create an isolated Unity test player. "
                f"HTTP {status}: {body[:500]}"
            )
        auth = json.loads(body)
        cls.player_id = auth["userId"]
        cls.token = auth["idToken"]
        print(f"\nTest environment: {ENVIRONMENT} (temporary player created)")

    def test_direct_api_accepts_valid_display_name_before_fix(self):
        """Baseline: a normal display name is accepted on the CURRENT direct-write path.

        After moving name writes behind Cloud Code + Protected Player Data,
        replace this baseline with a test of the validated Cloud Code endpoint.
        """
        url = f"{CLOUD_SAVE_BASE}/{PROJECT_ID}/players/{self.player_id}/item-batch"
        status, body = post_json(
            url,
            {"data": [{"key": "displayName", "value": "JuniorZ Tester"}]},
            {"Authorization": f"Bearer {self.token}"},
        )
        print(f"Valid displayName response: HTTP {status}: {body[:300]}")
        self.assertEqual(status, 200, f"A valid name was rejected: HTTP {status}: {body[:300]}")
        result = json.loads(body)
        self.assertTrue(
            any(item.get("key") == "displayName" for item in result.get("results", [])),
            "Save did not confirm the expected displayName key.",
        )

    def test_direct_api_rejects_whitespace_only_display_name(self):
        """A client must not bypass name validation by calling Cloud Save directly."""
        url = f"{CLOUD_SAVE_BASE}/{PROJECT_ID}/players/{self.player_id}/item-batch"
        status, body = post_json(
            url,
            {"data": [{"key": "displayName", "value": "   "}]},
            {"Authorization": f"Bearer {self.token}"},
        )
        print(f"Cloud Save response: HTTP {status}: {body[:300]}")

        # RED (before fix): server accepts the invalid value -> test FAILS.
        # GREEN (after fix): direct bypass is rejected -> test PASSES.
        # 401/404/5xx are deliberately NOT treated as a correct rejection:
        # they would indicate auth, routing, or server problems instead.
        self.assertIn(
            status,
            (400, 403, 422),
            "Server accepted an invalid displayName or returned an unrelated error. "
            "Expected validation/access rejection (400/403/422).",
        )


if __name__ == "__main__":
    unittest.main(verbosity=2)
