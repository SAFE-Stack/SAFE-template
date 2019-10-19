FROM microsoft/dotnet:3.0-aspnetcore-runtime-alpine
COPY /deploy /
WORKDIR /Server
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]