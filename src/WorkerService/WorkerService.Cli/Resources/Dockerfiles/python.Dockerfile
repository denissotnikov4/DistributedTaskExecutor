ARG LANGUAGE_IMAGE

FROM ${LANGUAGE_IMAGE}
WORKDIR /app
COPY main.py .
CMD ["python", "main.py"]