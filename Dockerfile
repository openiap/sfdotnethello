FROM dotnet9:latest
LABEL anonymous="true"
LABEL name="DotNET Serverless Example"
LABEL description="DotNET serverless hello world function"

COPY . /app
WORKDIR /app
RUN dotnet build dotnettest.csproj
EXPOSE 3000

# Set environment for .NET
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH=$PATH:/usr/share/dotnet

ENTRYPOINT ["dotnet", "/app/bin/Debug/net9.0/dotnettest.dll"]
