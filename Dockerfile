FROM dotnet9:latest
LABEL name="DotNET Serverless Example"
LABEL anonymous="true"
LABEL idle_timeout="-1"
COPY . /app
WORKDIR /app
RUN dotnet build
# Use dotnet to run the application instead of running the binary directly
ENTRYPOINT ["dotnet", "/app/bin/Debug/net9.0/dotnettest.dll"]
