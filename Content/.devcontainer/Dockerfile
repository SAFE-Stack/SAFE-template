FROM mcr.microsoft.com/dotnet/core/sdk:2.2

# Add keys and sources lists
RUN curl -sL https://deb.nodesource.com/setup_11.x | bash
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" \
    | tee /etc/apt/sources.list.d/yarn.list

# Install node, 7zip, yarn, git, process tools
RUN apt-get update && apt-get install -y nodejs p7zip-full yarn git procps

# Clean up
RUN apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# Install fake
RUN dotnet tool install fake-cli -g

# Install Paket
RUN dotnet tool install paket -g

# add dotnet tools to path to pick up fake and paket installation
ENV PATH="/root/.dotnet/tools:${PATH}"

# Copy endpoint specific user settings into container to specify
# .NET Core should be used as the runtime.
COPY settings.vscode.json /root/.vscode-remote/data/Machine/settings.json
