import requests
from requests.auth import HTTPBasicAuth

BASE_URL = "http://localhost:7270"
USERNAME = "admin@surveybasket.com"
PASSWORD = "Admin@123"
TIMEOUT = 30

def test_put_api_clients_me_avatar_update_client_avatar():
    # Login to get JWT token
    login_url = f"{BASE_URL}/api/Auth/login"
    login_payload = {
        "email": USERNAME,
        "password": PASSWORD
    }
    login_resp = requests.post(login_url, json=login_payload, timeout=TIMEOUT)
    assert login_resp.status_code == 200, f"Login failed: {login_resp.text}"
    login_data = login_resp.json()
    assert "token" in login_data, "JWT token missing in login response"
    token = login_data["token"]

    # Prepare headers with Bearer token authorization
    headers = {
        "Authorization": f"Bearer {token}"
    }

    # Prepare image file for multipart upload
    # Using a small PNG image in bytes - a 1x1 transparent pixel
    image_bytes = (
        b'\x89PNG\r\n\x1a\n\x00\x00\x00\rIHDR\x00\x00\x00\x01'
        b'\x00\x00\x00\x01\x08\x06\x00\x00\x00\x1f\x15\xc4\x89'
        b'\x00\x00\x00\nIDATx\xdac`\x00\x00\x00\x02\x00\x01'
        b'\xe2!\xbc\x33\x00\x00\x00\x00IEND\xaeB`\x82'
    )
    files = {
        "avatar": ("avatar.png", image_bytes, "image/png")
    }

    url = f"{BASE_URL}/api/Clients/me/avatar"
    resp = requests.put(url, headers=headers, files=files, timeout=TIMEOUT)
    assert resp.status_code == 200, f"Failed to update avatar: {resp.status_code} {resp.text}"

    resp_json = resp.json()
    assert "avatarUrl" in resp_json, "Response missing 'avatarUrl'"
    avatar_url = resp_json["avatarUrl"]
    assert isinstance(avatar_url, str) and avatar_url.startswith("http"), "Invalid avatar URL returned"

test_put_api_clients_me_avatar_update_client_avatar()