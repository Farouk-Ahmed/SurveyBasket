import requests

def test_get_api_polls_get_polls_list():
    base_url = "http://localhost:7270"
    url = f"{base_url}/api/Polls"

    try:
        response = requests.get(url, timeout=30)
        response.raise_for_status()
    except requests.RequestException as e:
        assert False, f"Request to get polls list failed: {e}"

    assert response.status_code == 200, f"Expected status code 200, got {response.status_code}"

    try:
        data = response.json()
    except ValueError:
        assert False, "Response is not valid JSON"

    assert isinstance(data, list) or isinstance(data, dict), "Polls data should be a list or dict"
    # Additional checks can be added here depending on the polls data schema

test_get_api_polls_get_polls_list()