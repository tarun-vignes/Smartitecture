import requests
import json

def test_api():
    base_url = "http://127.0.0.1:8000"
    
    print("🧪 Testing Smartitecture Python API")
    print("=" * 40)
    
    # Test 1: Health check
    try:
        response = requests.get(f"{base_url}/health")
        print(f"✅ Health check: {response.status_code} - {response.json()}")
    except Exception as e:
        print(f"❌ Health check failed: {e}")
        return
    
    # Test 2: Root endpoint
    try:
        response = requests.get(f"{base_url}/")
        print(f"✅ Root endpoint: {response.status_code} - {response.json()}")
    except Exception as e:
        print(f"❌ Root endpoint failed: {e}")
    
    # Test 3: Get config
    try:
        response = requests.get(f"{base_url}/api/config")
        print(f"✅ Get config: {response.status_code} - {response.json()}")
    except Exception as e:
        print(f"❌ Get config failed: {e}")
    
    # Test 4: Set config
    try:
        data = {"key": "test_key", "value": "test_value"}
        response = requests.post(f"{base_url}/api/config", json=data)
        print(f"✅ Set config: {response.status_code} - {response.json()}")
    except Exception as e:
        print(f"❌ Set config failed: {e}")
    
    # Test 5: Process text
    try:
        data = {"input_text": "Hello, world!", "model": "gpt-4"}
        response = requests.post(f"{base_url}/api/process", json=data)
        print(f"✅ Process text: {response.status_code} - {response.json()}")
    except Exception as e:
        print(f"❌ Process text failed: {e}")
    
    print("\n🎉 API testing completed!")

if __name__ == "__main__":
    test_api()
