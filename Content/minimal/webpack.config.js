var path = require('path');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');

// If we're running the webpack-dev-server, assume we're in development mode
var isProduction = !process.argv.find(v => v.indexOf('webpack-dev-server') !== -1);
var mode = isProduction ? 'production' : 'development';
process.env.NODE_ENV = mode;

function resolve(filePath) {
    return path.join(__dirname, filePath);
}

// The HtmlWebpackPlugin allows us to use a template for the index.html page
// and automatically injects <script> or <link> tags for generated bundles.
var htmlPlugin =
    new HtmlWebpackPlugin({
        filename: 'index.html',
        template: resolve('./src/Client/index.html')
    });

// Copies static assets to output directory
var copyPlugin =
    new CopyWebpackPlugin({
        patterns: [{
            from: resolve('./src/Client/public')
        }]
    });


// Configuration for webpack-dev-server
var devServer = {
    static: {
        directory: resolve('./src/Client/public'),
        publicPath: '/'
    },
    host: '0.0.0.0',
    port: 8080,
    proxy: {
        // Redirect requests that start with /api/ to the server on port 8085
        '/api/**': {
            target: 'http://localhost:8085',
            changeOrigin: true
        }
    },
    hot: true,
    historyApiFallback: true
};

console.log('Bundling for ' + mode + '...');

module.exports = {
    entry: { app: resolve('./src/Client/Client.fs.js') },
    output: { path: resolve('./deploy/public') },
    resolve: { symlinks: false }, // See https://github.com/fable-compiler/Fable/issues/1490
    mode: mode,
    plugins: isProduction ? [htmlPlugin, copyPlugin] : [htmlPlugin],
    optimization: {
        runtimeChunk: "single",
        moduleIds: 'deterministic',
        // Split the code coming from npm packages into a different file.
        // 3rd party dependencies change less often, let the browser cache them.
        splitChunks: {
            chunks: 'all'
        }
    },
    devServer: devServer
};