from setuptools import setup, find_packages

with open("requirements.txt") as f:
    requirements = f.read().splitlines()

setup(
    name="smartitecture",
    version="1.0.0",
    description="Smartitecture Python Desktop Application",
    author="Tarun Vignes",
    packages=find_packages(),
    install_requires=requirements,
    entry_points={
        'console_scripts': [
            'smartitecture=main:main',
        ],
    },
    include_package_data=True,
    python_requires='>=3.9',
    classifiers=[
        'Development Status :: 3 - Alpha',
        'Intended Audience :: Developers',
        'License :: OSI Approved :: MIT License',
        'Programming Language :: Python :: 3.9',
        'Programming Language :: Python :: 3.10',
        'Programming Language :: Python :: 3.11',
    ],
)
