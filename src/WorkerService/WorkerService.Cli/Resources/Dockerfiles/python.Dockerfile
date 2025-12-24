ARG LANGUAGE_IMAGE=python:3.11-slim

FROM ${LANGUAGE_IMAGE}
WORKDIR /app
COPY main.py .
CMD ["python", "main.py"]