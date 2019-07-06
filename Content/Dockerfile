//#if (deploy == "docker-build")
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build

RUN TERM=noninteractive apt-get update

# From SAFE-Stack documentation - pull the prereqs
# FAKE
RUN dotnet tool install -g fake-cli
# Node.JS
RUN curl -sL https://deb.nodesource.com/setup_10.x -o nodesource_setup.sh
RUN bash ./nodesource_setup.sh
RUN TERM=noninteractive apt-get -y install build-essential nodejs
# Yarn
RUN curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list
RUN TERM=noninteractive apt-get update && apt-get -y install yarn
# paket
RUN dotnet tool install -g Paket

# Now build the app. For some reason, despite using -g, the tools got installed
# locally only but that's OK, we'll just add them to the PATH

ENV PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/root/.dotnet/tools

# Copy over everything that is important. We don't just ADD . because this
# will destroy the cache. We add the files and dirs in an order that makes
# it possible to use as much of the cache as possible.
RUN mkdir /build
ADD .devcontainer /build/.devcontainer
ADD .paket /build/.paket
ADD .vscode /build/.vscode
ADD .editorconfig /build/
ADD RELEASE_NOTES.md /build/
ADD README.md /build/
ADD *.sln /build/
ADD build.fsx /build/
ADD webpack.config.js /build/
ADD package.json /build/
ADD paket.* /build/
ADD yarn.lock /build/
ADD src /build/src/

# Enter the build dir
WORKDIR /build

# Just to be sure: Run yarn install and paket install to know additional pkgs
# that might have been added
RUN yarn install
RUN paket install

# Now build
RUN fake build -t Bundle

# Switch to a smaller .net Core image for running
FROM mcr.microsoft.com/dotnet/core/runtime:2.2

COPY --from=build /build/deploy/ /app/

# Get into the Server dir, that's where the app is run from
WORKDIR /app/Server

# Runtime options
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]
//#else
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
COPY /deploy /
WORKDIR /Server
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]
//#endif