from fastapi import FastAPI
import uvicorn

app = FastAPI()

@app.get("/")
async def read_root():
    return {"message": "Hello, this is a test!"}

@app.get("/health")
async def health_check():
    return {"status": "ok", "service": "test_app"}

if __name__ == "__main__":
    print("Starting test FastAPI server...")
    print("Try these endpoints in your browser:")
    print(" - http://127.0.0.1:8000/")
    print(" - http://127.0.0.1:8000/health")
    print(" - http://127.0.0.1:8000/docs (for Swagger UI)")
    uvicorn.run("test_app:app", host="127.0.0.1", port=8000, reload=True)
