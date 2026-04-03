import requests
from requests.auth import HTTPBasicAuth

BASE_URL = "http://localhost:7270"
LOGIN_PATH = "/api/Auth/login"
REFRESH_PATH = "/api/Auth/refresh"
USERNAME = "admin@surveybasket.com"
PASSWORD = "Admin@123"
TIMEOUT = 30

def test_post_api_auth_refresh_token_refresh():
    # Step 1: Login to get valid refresh token
    login_url = BASE_URL + LOGIN_PATH
    login_payload = {"email": USERNAME, "password": PASSWORD}
    login_headers = {"Content-Type": "application/json"}

    login_response = requests.post(
        login_url,
        json=login_payload,
        headers=login_headers,
        auth=HTTPBasicAuth(USERNAME, PASSWORD),
        timeout=TIMEOUT,
    )
    assert login_response.status_code == 200, f"Login failed: {login_response.text}"
    login_json = login_response.json()
    assert "refreshToken" in login_json, "Response missing refreshToken"
    valid_refresh_token = login_json["refreshToken"]

    refresh_url = BASE_URL + REFRESH_PATH
    refresh_headers = {"Content-Type": "application/json"}

    # Test valid refresh token (expect 200 OK with new JWT)
    refresh_payload_valid = {"refreshToken": valid_refresh_token}
    refresh_response_valid = requests.post(
        refresh_url,
        json=refresh_payload_valid,
        headers=refresh_headers,
        auth=HTTPBasicAuth(USERNAME, PASSWORD),
        timeout=TIMEOUT,
    )
    assert refresh_response_valid.status_code == 200, (
        f"Valid refresh token refresh failed: {refresh_response_valid.text}"
    )
    refresh_json = refresh_response_valid.json()
    assert "token" in refresh_json and refresh_json["token"], "New JWT token missing"
    assert "refreshToken" in refresh_json and refresh_json["refreshToken"], "New refreshToken missing"

    # Test invalid refresh token (expect 401 Unauthorized)
    invalid_refresh_payload = {"refreshToken": "revoked_or_invalid_token"}
    refresh_response_invalid = requests.post(
        refresh_url,
        json=invalid_refresh_payload,
        headers=refresh_headers,
        auth=HTTPBasicAuth(USERNAME, PASSWORD),
        timeout=TIMEOUT,
    )
    assert refresh_response_invalid.status_code == 401, (
        f"Invalid refresh token did not return 401: {refresh_response_invalid.text}"
    )

test_post_api_auth_refresh_token_refresh()
