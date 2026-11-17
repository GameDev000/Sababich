"""JuniorZ Exercise 2: direct API tests for Cloud Code SetDisplayName.

Run after publishing SetDisplayName in the `juniorz-test` environment:
    py .\test\test_cloud_code_validation.py

Only Python stdlib; creates a throwaway anonymous player in juniorz-test.
This tests Cloud Code + Protected Player Data, NOT the migration of the
existing game client off Default Cloud Save (that is a separate task).
A protected-write endpoint requires server authority; player tokens can
return HTTP 401 or 403. We additionally verify the record is unchanged.
"""

import json
import os
import unittest
from urllib.error import HTTPError
from urllib.request import Request, urlopen

PROJECT_ID = os.environ.get("UNITY_PROJECT_ID", "35da4438-78cb-4c10-a72a-984418a8a547")
ENVIRONMENT = "juniorz-test"
AUTH_URL = "https://player-auth.services.api.unity.com/v1/authentication/anonymous"
CLOUD_CODE_URL = f"https://cloud-code.services.api.unity.com/v1/projects/{PROJECT_ID}/scripts/SetDisplayName"
CLOUD_SAVE_BASE = f"https://cloud-save.services.api.unity.com/v1/data/projects/{PROJECT_ID}/players"


def request_json(method, url, payload=None, headers=None):
    data = None if payload is None else json.dumps(payload).encode("utf-8")
    request = Request(
        url,
        data=data,
        headers={"Content-Type": "application/json", **(headers or {})},
        method=method,
    )
    try:
        with urlopen(request, timeout=25) as response:
            return response.status, response.read().decode("utf-8")
    except HTTPError as error:
        return error.code, error.read().decode("utf-8", errors="replace")


class CloudCodeDisplayNameTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        if ENVIRONMENT != "juniorz-test":
            raise RuntimeError("Safety check: test environment only")
        status, body = request_json("POST", AUTH_URL, {}, {
            "ProjectId": PROJECT_ID,
            "UnityEnvironment": ENVIRONMENT,
        })
        if status != 200:
            raise RuntimeError(f"Could not create test player: HTTP {status}: {body[:400]}")
        auth = json.loads(body)
        cls.player_id = auth["userId"]
        cls.token = auth["idToken"]
        cls.auth_header = {"Authorization": f"Bearer {cls.token}"}
        print(f"\nEnvironment: {ENVIRONMENT} (new temporary player)")

    def run_cloud_code(self, display_name):
        return request_json(
            "POST", CLOUD_CODE_URL, {"params": {"displayName": display_name}},
            self.auth_header,
        )

    def test_01_whitespace_name_is_rejected_by_cloud_code(self):
        status, body = self.run_cloud_code("   ")
        print(f"Cloud Code whitespace name: HTTP {status}: {body[:450]}")
        self.assertEqual(status, 422, "Expected Cloud Code script validation error (422).")
        self.assertIn(
            "INVALID_DISPLAY_NAME", body,
            "422 alone is insufficient: must confirm validation caused the rejection.",
        )

    def test_02_valid_name_is_saved_as_protected_data(self):
        status, body = self.run_cloud_code("  JuniorZ Tester  ")
        print(f"Cloud Code valid name: HTTP {status}: {body[:450]}")
        self.assertEqual(status, 200, "Expected successful Cloud Code call.")
        output = json.loads(body).get("output", {})
        self.assertEqual(output.get("success"), True)
        self.assertEqual(output.get("displayName"), "JuniorZ Tester")

        # Read back what the server wrote; a player may READ protected data.
        url = f"{CLOUD_SAVE_BASE}/{self.player_id}/protected/items"
        read_status, read_body = request_json("GET", url, headers=self.auth_header)
        print(f"Protected data read-back: HTTP {read_status}: {read_body[:450]}")
        self.assertEqual(read_status, 200)
        values = {
            item.get("key"): item.get("value")
            for item in json.loads(read_body).get("results", [])
        }
        self.assertEqual(values.get("displayName"), "JuniorZ Tester")

    def test_03_direct_client_write_to_protected_data_is_denied(self):
        # Establish a known, valid protected value through the server script.
        setup_status, setup_body = self.run_cloud_code("JuniorZ Tester")
        self.assertEqual(
            setup_status, 200,
            f"Test setup failed: could not create protected displayName: {setup_body[:300]}",
        )

        # Bypass Cloud Code: authenticated player tries to overwrite Protected.
        # This endpoint is restricted to SERVER authority. A player JWT may be
        # rejected as 401 (invalid authentication type) BEFORE 403 (authorization).
        url = f"{CLOUD_SAVE_BASE}/{self.player_id}/protected/items"
        status, body = request_json(
            "POST", url, {"key": "displayName", "value": "   "},
            self.auth_header,
        )
        print(f"Direct Protected write: HTTP {status}: {body[:450]}")
        self.assertIn(
            status, (401, 403),
            "A player token must be rejected by the server-authority-only "
            f"protected write endpoint. Got HTTP {status}: {body[:300]}",
        )

        # Crucial verification: the rejected HTTP request DID NOT change the
        # protected value. A mere 401/403 without read-back is weaker evidence.
        read_status, read_body = request_json(
            "GET", url, headers=self.auth_header
        )
        self.assertEqual(
            read_status, 200,
            f"Could not verify protected value after rejected write: {read_body[:300]}",
        )
        items = json.loads(read_body).get("results", [])
        display_name = next(
            (item.get("value") for item in items if item.get("key") == "displayName"),
            None,
        )
        print(f"Protected displayName after rejected write: {display_name!r}")
        self.assertEqual(
            display_name, "JuniorZ Tester",
            "A direct client request must not modify protected displayName.",
        )


if __name__ == "__main__":
    unittest.main(verbosity=2)
