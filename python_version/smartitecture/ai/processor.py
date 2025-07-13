import openai
import json
from typing import Dict, List

class AIProcessor:
    def __init__(self, api_key: str):
        self.api_key = api_key
        self.client = openai.OpenAI(api_key=api_key)
        self.system_prompt = """
        You are Smartitecture, an intelligent AI assistant.
        Your goal is to help users with their tasks and questions.
        Always provide clear, concise, and helpful responses.
        """
        
    def process_request(self, user_input: str, history: List[Dict]) -> str:
        """Process user input and generate response"""
        messages = [
            {"role": "system", "content": self.system_prompt}
        ] + history + [
            {"role": "user", "content": user_input}
        ]
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-4",
                messages=messages,
                temperature=0.7,
                max_tokens=2000
            )
            return response.choices[0].message.content
        except Exception as e:
            return f"Error processing request: {str(e)}"
        
    def analyze_code(self, code: str) -> Dict:
        """Analyze code and provide suggestions"""
        prompt = f"""Analyze this code and provide suggestions:
        ```
        {code}
        ```
        """
        return self.process_request(prompt, [])
        
    def generate_code(self, description: str) -> Dict:
        """Generate code based on description"""
        prompt = f"""Generate code that implements this functionality:
        {description}
        """
        return self.process_request(prompt, [])
