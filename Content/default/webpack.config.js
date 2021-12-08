// In most cases, you'll only need to edit the CONFIG/TEST_CONFIG objects
// CONFIG is the configuration used to run the application.
// TEST_CONFIG is the configuration used to run tests.
// If you need better fine-tuning of Webpack options check the buildConfig function.
const common = require("./webpack.common");

const CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './src/Client/index.html',
    fsharpEntry: './src/Client/output/App.js',
    outputDir: './deploy/public',
    assetsDir: './src/Client/public',
    devServerPort: 8080,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: {
        // redirect requests that start with /api/ to the server on port 8085
        '/api/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || '8085'),
            changeOrigin: true
        },
        // redirect websocket requests that start with /socket/ to the server on the port 8085
        '/socket/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || '8085'),
            ws: true
        }
    }
}

const TEST_CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: 'tests/Client/index.html',
    fsharpEntry: 'tests/Client/output/Client.Tests.js',
    outputDir: 'tests/Client',
    assetsDir: 'tests/Client',
    devServerPort: 8081,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: undefined,
}

module.exports = function(env, arg) {
    // Mode is passed as a flag to npm run. see the docs for more details on flags https://webpack.js.org/api/cli/#flags
    const mode = arg.mode ?? 'development';
    // environment variables docs: https://webpack.js.org/api/cli/#environment-options
    const config = env.test ? TEST_CONFIG : CONFIG;

    console.log(`Bundling for ${env.test ? 'test' : 'run'} - ${mode} ...`);

    return common.buildConfig(config, mode);
}