import requests

BASE_URL = "http://localhost:7270"
AUTH_USERNAME = "admin@surveybasket.com"
AUTH_PASSWORD = "Admin@123"
TIMEOUT_SECONDS = 30

def test_get_api_dashboard_audit_logs_with_valid_admin_jwt():
    login_url = f"{BASE_URL}/api/Auth/login"
    audit_logs_url = f"{BASE_URL}/api/Dashboard/audit-logs"

    # Step 1: Login and get JWT token
    login_payload = {
        "email": AUTH_USERNAME,
        "password": AUTH_PASSWORD
    }
    try:
        login_response = requests.post(login_url, json=login_payload, timeout=TIMEOUT_SECONDS)
    except requests.RequestException as e:
        assert False, f"Login request failed with exception: {e}"

    assert login_response.status_code == 200, f"Expected 200 OK from login, got {login_response.status_code}"
    login_json = login_response.json()
    jwt_token = login_json.get("token")
    assert jwt_token and isinstance(jwt_token, str), "JWT token not found or invalid in login response"

    # Step 2: Use JWT token to get audit logs
    headers = {
        "Authorization": f"Bearer {jwt_token}"
    }
    try:
        audit_response = requests.get(audit_logs_url, headers=headers, timeout=TIMEOUT_SECONDS)
    except requests.RequestException as e:
        assert False, f"Audit logs request failed with exception: {e}"

    assert audit_response.status_code == 200, f"Expected 200 OK from audit logs, got {audit_response.status_code}"

    audit_json = audit_response.json()
    assert audit_json is not None, "Audit logs response JSON is None"
    assert isinstance(audit_json, (list, dict)), "Audit logs response JSON should be list or dict"

test_get_api_dashboard_audit_logs_with_valid_admin_jwt()
