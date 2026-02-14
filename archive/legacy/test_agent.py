#!/usr/bin/env python3
"""
Simple test script for Smartitecture Agent API
Run this to test your agent without any C# build issues.
"""

import requests
import json

def test_agent(command):
    """Test the agent with a given command."""
    url = "http://127.0.0.1:8001/agent/run"
    payload = {"input": command}
    
    try:
        print(f"\n🤖 Testing command: '{command}'")
        print("=" * 50)
        
        response = requests.post(url, json=payload)
        
        if response.status_code == 200:
            result = response.json()
            print("✅ Success!")
            print(json.dumps(result, indent=2))
        else:
            print(f"❌ Error: HTTP {response.status_code}")
            print(response.text)
            
    except requests.exceptions.ConnectionError:
        print("❌ Error: Cannot connect to backend. Make sure it's running on port 8001")
    except Exception as e:
        print(f"❌ Error: {e}")

def test_health():
    """Test if the backend is running."""
    try:
        response = requests.get("http://127.0.0.1:8001/health")
        if response.status_code == 200:
            print("✅ Backend is running!")
            return True
        else:
            print(f"❌ Backend health check failed: {response.status_code}")
            return False
    except:
        print("❌ Backend is not running or not accessible")
        return False

if __name__ == "__main__":
    print("🚀 Smartitecture Agent Test Script")
    print("=" * 40)
    
    # Test backend health first
    if not test_health():
        print("\n💡 Make sure to start the backend first:")
        print("cd backend-python")
        print("python -m uvicorn smartitecture.api.main:app --host 127.0.0.1 --port 8001")
        exit(1)
    
    # Test commands
    test_commands = [
        "2 + 2",
        "what time is it?",
        "calculate 15 * 7",
        "hello world"
    ]
    
    for command in test_commands:
        test_agent(command)
    
    print("\n🎉 Testing complete!")
    print("\n💡 You can also test manually by running:")
    print("python test_agent.py")
