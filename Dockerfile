FROM dotnet9:latest
LABEL anonymous="true"
LABEL name="DotNET Serverless Example"
LABEL description="DotNET serverless hello world function"
COPY . /app
WORKDIR /app
RUN dotnet build
EXPOSE 3000
#  here add the env command for dotnet
# ENV NODE_ENV=production
ENTRYPOINT ["/app/bin/Debug/net9.0/dotnettest"]
