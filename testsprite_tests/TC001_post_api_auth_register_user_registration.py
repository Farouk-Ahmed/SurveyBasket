import requests
import uuid

BASE_URL = "http://localhost:7270"
REGISTER_PATH = "/api/Auth/register"
TIMEOUT = 30

def test_post_api_auth_register_user_registration():
    headers = {
        "Content-Type": "application/json"
    }

    # Generate unique email for testing success registration
    unique_email = f"testuser_{uuid.uuid4()}@example.com"
    password = "StrongP@ssw0rd1"

    payload = {
        "email": unique_email,
        "password": password
    }

    user_id = None

    try:
        # Test successful registration
        resp = requests.post(
            BASE_URL + REGISTER_PATH,
            json=payload,
            headers=headers,
            timeout=TIMEOUT
        )
        assert resp.status_code == 201, f"Expected 201 Created, got {resp.status_code}"
        json_resp = resp.json()
        assert "id" in json_resp and json_resp["id"], "Response JSON must contain non-empty 'id'"
        user_id = json_resp["id"]

        # Test registration with the same email again to get 400 Bad Request
        resp_dup = requests.post(
            BASE_URL + REGISTER_PATH,
            json=payload,
            headers=headers,
            timeout=TIMEOUT
        )
        assert resp_dup.status_code == 400, f"Expected 400 Bad Request for duplicate email, got {resp_dup.status_code}"

    finally:
        if user_id:
            # Cleanup: delete the test user if API available. Since no delete endpoint given, skip cleanup.
            pass

test_post_api_auth_register_user_registration()
