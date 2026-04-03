import requests
from requests.auth import HTTPBasicAuth

BASE_URL = "http://localhost:7270"
AUTH_LOGIN_PATH = "/api/Auth/login"
CLIENTS_ME_PATH = "/api/Clients/me"

USERNAME = "admin@surveybasket.com"
PASSWORD = "Admin@123"
TIMEOUT = 30

def get_jwt_token():
    login_url = BASE_URL + AUTH_LOGIN_PATH
    login_payload = {
        "email": USERNAME,
        "password": PASSWORD
    }
    try:
        response = requests.post(login_url, json=login_payload, timeout=TIMEOUT)
        response.raise_for_status()
        json_response = response.json()
        return json_response.get("token") or json_response.get("jwt") or json_response.get("accessToken")
    except Exception as e:
        raise RuntimeError(f"Authentication failed: {e}")

def test_put_api_clients_me_update_client_profile():
    token = get_jwt_token()
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    url = BASE_URL + CLIENTS_ME_PATH

    # Valid payload for update
    valid_payload = {
        "firstName": "Test",
        "lastName": "User",
        "phone": "1234567890",
        "company": "Test Company",
        "title": "Tester"
    }

    # Send valid update request
    try:
        response = requests.put(url, json=valid_payload, headers=headers, timeout=TIMEOUT)
        response.raise_for_status()
        assert response.status_code == 200, f"Expected 200 OK but got {response.status_code}"
        updated_profile = response.json()
        for key, value in valid_payload.items():
            assert key in updated_profile, f"Response missing key {key}"
            assert updated_profile[key] == value, f"Expected {key} to be {value}, got {updated_profile[key]}"
    except Exception as e:
        raise AssertionError(f"Valid payload update failed: {e}")

    # Invalid payload (e.g., missing required fields or wrong data types)
    invalid_payload = {
        "firstName": "",
        "lastName": 12345,  # invalid data type
        "phone": "invalid-phone-number",
        "company": None,
        "title": ""
    }

    # Send invalid update request
    try:
        response = requests.put(url, json=invalid_payload, headers=headers, timeout=TIMEOUT)
        assert response.status_code == 400, f"Expected 400 Bad Request but got {response.status_code}"
    except requests.exceptions.HTTPError as http_err:
        if http_err.response.status_code != 400:
            raise AssertionError(f"Invalid payload update failed with unexpected HTTP error: {http_err}")
    except Exception as e:
        raise AssertionError(f"Invalid payload update failed: {e}")

test_put_api_clients_me_update_client_profile()