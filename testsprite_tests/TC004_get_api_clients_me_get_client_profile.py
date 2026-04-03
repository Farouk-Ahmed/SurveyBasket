import requests

BASE_URL = "http://localhost:7270"
AUTH_LOGIN_PATH = "/api/Auth/login"
CLIENT_PROFILE_PATH = "/api/Clients/me"
USERNAME = "admin@surveybasket.com"
PASSWORD = "Admin@123"
TIMEOUT = 30

def test_get_api_clients_me_get_client_profile():
    # Step 1: Login to get JWT token
    login_url = BASE_URL + AUTH_LOGIN_PATH
    login_payload = {"email": USERNAME, "password": PASSWORD}
    try:
        login_response = requests.post(login_url, json=login_payload, timeout=TIMEOUT)
        assert login_response.status_code == 200, f"Login failed with status {login_response.status_code}"
        login_data = login_response.json()
        assert "token" in login_data or "jwt" in login_data, "JWT token not found in login response"
        token = login_data.get("token") or login_data.get("jwt")
    except requests.RequestException as e:
        assert False, f"Login request failed: {e}"

    headers_auth = {"Authorization": f"Bearer {token}"}
    profile_url = BASE_URL + CLIENT_PROFILE_PATH

    # Step 2: Fetch client profile with valid JWT
    try:
        profile_response = requests.get(profile_url, headers=headers_auth, timeout=TIMEOUT)
        assert profile_response.status_code == 200, f"Expected 200 OK, got {profile_response.status_code}"
        profile_data = profile_response.json()
        assert isinstance(profile_data, dict), "Profile data is not a JSON object"
        # Basic check for expected client profile fields could be added if known
    except requests.RequestException as e:
        assert False, f"Profile request with valid JWT failed: {e}"

    # Step 3: Fetch client profile without Authorization header
    try:
        profile_response_no_auth = requests.get(profile_url, timeout=TIMEOUT)
        assert profile_response_no_auth.status_code == 401, f"Expected 401 Unauthorized, got {profile_response_no_auth.status_code}"
    except requests.RequestException as e:
        assert False, f"Profile request without auth failed: {e}"

test_get_api_clients_me_get_client_profile()
