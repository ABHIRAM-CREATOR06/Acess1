from setuptools import setup, find_packages

with open("README.md", "r", encoding="utf-8") as fh:
    long_description = fh.read()

setup(
    name="axis-core",
    version="1.0.0",
    author="ABHIRAM-CREATOR06",
    description="AXIS-CORE SDK for programmatic web accessibility checking (Python)",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/ABHIRAM-CREATOR06/Acess1",
    packages=find_packages(),
    classifiers=[
        "Development Status :: 3 - Alpha",
        "Intended Audience :: Developers",
        "License :: OSI Approved :: GNU General Public License v3 or later (GPLv3+)",
        "Operating System :: OS Independent",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
    ],
    python_requires=">=3.8",
    install_requires=[
        "requests",
        "beautifulsoup4",
        "lxml",
    ],
)