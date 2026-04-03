import requests
from requests.auth import HTTPBasicAuth

BASE_URL = "http://localhost:7270"
ADMIN_USERNAME = "admin@surveybasket.com"
ADMIN_PASSWORD = "Admin@123"
TIMEOUT = 30


def test_get_api_dashboard_summary():
    # Helper function to get JWT token using login
    def get_jwt_token(username, password):
        url = f"{BASE_URL}/api/Auth/login"
        payload = {"email": username, "password": password}
        headers = {"Content-Type": "application/json"}
        resp = requests.post(url, json=payload, headers=headers, timeout=TIMEOUT)
        resp.raise_for_status()
        data = resp.json()
        assert "token" in data or "jwt" in data or "accessToken" in data, "JWT token not found in login response"
        # Accepting possible naming variations in response key for JWT token
        token = data.get("token") or data.get("jwt") or data.get("accessToken")
        return token

    # Get valid admin JWT token
    valid_jwt = get_jwt_token(ADMIN_USERNAME, ADMIN_PASSWORD)

    # Test 1: Retrieve dashboard summary with valid admin JWT - expect 200 OK and summary data
    url_summary = f"{BASE_URL}/api/Dashboard/summary"
    headers_valid = {"Authorization": f"Bearer {valid_jwt}"}
    resp_valid = requests.get(url_summary, headers=headers_valid, timeout=TIMEOUT)
    assert resp_valid.status_code == 200, f"Expected 200 OK, got {resp_valid.status_code}"
    json_valid = resp_valid.json()
    assert isinstance(json_valid, dict), "Dashboard summary response should be a JSON object"
    assert len(json_valid) > 0, "Dashboard summary data should not be empty"

    # Test 2: Retrieve dashboard summary with expired JWT - expect 401 Unauthorized
    # Since we don't have a real expired token, simulate by altering valid token or using invalid JWT
    expired_jwt = valid_jwt[:-1] + ('a' if valid_jwt[-1] != 'a' else 'b')  # alter last char to invalidate token
    headers_expired = {"Authorization": f"Bearer {expired_jwt}"}
    resp_expired = requests.get(url_summary, headers=headers_expired, timeout=TIMEOUT)
    assert resp_expired.status_code == 401, f"Expected 401 Unauthorized, got {resp_expired.status_code}"

test_get_api_dashboard_summary()