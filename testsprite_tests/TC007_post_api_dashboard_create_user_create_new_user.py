import requests
import uuid

BASE_URL = "http://localhost:7270"
TIMEOUT = 30

def get_jwt_token(email: str, password: str) -> str:
    login_url = f"{BASE_URL}/api/Auth/login"
    payload = {"email": email, "password": password}
    resp = requests.post(login_url, json=payload, timeout=TIMEOUT)
    resp.raise_for_status()
    return resp.json().get("jwt") or resp.json().get("token") or resp.json().get("accessToken")

def test_post_api_dashboard_create_user_create_new_user():
    admin_email = "admin@surveybasket.com"
    admin_password = "Admin@123"

    # Use an existing non-admin user, or create one
    # For purpose of testing forbidden access, create a non-admin user first
    # Create non-admin user via register endpoint
    register_url = f"{BASE_URL}/api/Auth/register"
    non_admin_email = f"testuser_{uuid.uuid4().hex[:8]}@example.com"
    non_admin_password = "Password@123"
    # Register non-admin user
    register_payload = {"email": non_admin_email, "password": non_admin_password}
    reg_resp = requests.post(register_url, json=register_payload, timeout=TIMEOUT)
    assert reg_resp.status_code == 201, f"Expected 201 on register non-admin user, got {reg_resp.status_code}"

    admin_jwt = get_jwt_token(admin_email, admin_password)
    non_admin_jwt = get_jwt_token(non_admin_email, non_admin_password)

    create_user_url = f"{BASE_URL}/api/Dashboard/create-user"
    new_user_email = f"newuser_{uuid.uuid4().hex[:8]}@example.com"
    new_user_password = "NewUserPass@123"
    payload = {
        "email": new_user_email,
        "password": new_user_password,
        "role": "user"
    }

    # Test with admin JWT - Expect 201 Created with user id
    headers_admin = {
        "Authorization": f"Bearer {admin_jwt}",
        "Content-Type": "application/json"
    }
    create_resp = requests.post(create_user_url, json=payload, headers=headers_admin, timeout=TIMEOUT)
    try:
        assert create_resp.status_code == 201, f"Expected 201 Created for admin user creation, got {create_resp.status_code}"
        json_resp = create_resp.json()
        user_id = json_resp.get("id") or json_resp.get("userId")
        assert user_id is not None, "Response does not contain new user id"
    finally:
        # Clean up: delete the created user if possible
        if create_resp.status_code == 201 and user_id:
            # No delete endpoint specified in PRD. If available, this would be the place to delete.
            pass

    # Test with non-admin JWT - Expect 403 Forbidden
    headers_non_admin = {
        "Authorization": f"Bearer {non_admin_jwt}",
        "Content-Type": "application/json"
    }
    forbidden_resp = requests.post(create_user_url, json=payload, headers=headers_non_admin, timeout=TIMEOUT)
    assert forbidden_resp.status_code == 403, f"Expected 403 Forbidden for non-admin user creation, got {forbidden_resp.status_code}"

test_post_api_dashboard_create_user_create_new_user()