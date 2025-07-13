from setuptools import setup, find_packages
import os

# Read requirements
with open("requirements.txt", "r") as f:
    requirements = f.read().splitlines()

# Read README
with open("README.md", "r") as f:
    long_description = f.read()

setup(
    name="smartitecture",
    version="1.0.0",
    author="Smartitecture Team",
    author_email="team@smartitecture.ai",
    description="A desktop AI assistant built with Python",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/yourusername/smartitecture",
    packages=find_packages(where="python_version"),
    package_dir={"": "python_version"},
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ],
    python_requires='>=3.8',
    install_requires=requirements,
    entry_points={
        'console_scripts': [
            'smartitecture=smartitecture.ui.main_window:main',
        ],
    },
    include_package_data=True,
    package_data={
        'smartitecture': ['*.ui', '*.qss'],
    },
)
