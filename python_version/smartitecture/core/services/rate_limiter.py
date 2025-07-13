import time
from typing import Dict, Optional
from collections import defaultdict
from .base_service import BaseService

class RateLimiter(BaseService):
    def __init__(self):
        super().__init__()
        self.requests: Dict[str, list] = defaultdict(list)
        self.max_requests = Settings.MAX_REQUESTS_PER_MINUTE
        self.window = 60  # 1 minute
        
    def check_limit(self, user_id: str) -> bool:
        """Check if user has exceeded rate limit"""
        current_time = time.time()
        
        # Remove old requests
        self.requests[user_id] = [
            t for t in self.requests[user_id]
            if current_time - t <= self.window
        ]
        
        # Check limit
        if len(self.requests[user_id]) >= self.max_requests:
            return False
            
        # Add current request
        self.requests[user_id].append(current_time)
        return True
        
    def get_remaining_requests(self, user_id: str) -> int:
        """Get remaining requests for user"""
        self.check_limit(user_id)  # This also cleans up old requests
        return self.max_requests - len(self.requests[user_id])
        
    def reset(self, user_id: str) -> None:
        """Reset rate limit for user"""
        self.requests[user_id] = []
