FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
COPY /deploy /
WORKDIR /Server
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]