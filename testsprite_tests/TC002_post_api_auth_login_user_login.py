import requests
from requests.auth import HTTPBasicAuth

BASE_URL = "http://localhost:7270"
LOGIN_ENDPOINT = "/api/Auth/login"
TIMEOUT = 30

def test_post_api_auth_login_user_login():
    url = BASE_URL + LOGIN_ENDPOINT

    # Valid credentials from instructions
    valid_email = "admin@surveybasket.com"
    valid_password = "Admin@123"

    headers = {
        "Content-Type": "application/json"
    }

    # Test login with valid credentials
    valid_payload = {
        "email": valid_email,
        "password": valid_password
    }

    response = requests.post(url, json=valid_payload, headers=headers, timeout=TIMEOUT)
    assert response.status_code == 200, f"Expected 200 OK but got {response.status_code} for valid login"

    json_response = response.json()
    assert "token" in json_response and isinstance(json_response["token"], str) and json_response["token"], "JWT token missing or empty"
    assert "refreshToken" in json_response and isinstance(json_response["refreshToken"], str) and json_response["refreshToken"], "Refresh token missing or empty"

    # Test login with incorrect password
    invalid_payload = {
        "email": valid_email,
        "password": "wrongpassword"
    }
    response_invalid = requests.post(url, json=invalid_payload, headers=headers, timeout=TIMEOUT)
    assert response_invalid.status_code == 401, f"Expected 401 Unauthorized but got {response_invalid.status_code} for invalid login"

test_post_api_auth_login_user_login()