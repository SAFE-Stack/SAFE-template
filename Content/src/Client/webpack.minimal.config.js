var path = require('path');
var { HotModuleReplacementPlugin } = require('webpack');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');

function resolve(filePath) {
    return path.join(__dirname, filePath);
}

// The HtmlWebpackPlugin allows us to use a template for the index.html page
// and automatically injects <script> or <link> tags for generated bundles.
var htmlPlugin =
    new HtmlWebpackPlugin({
        filename: 'index.html',
        template: resolve('./index.html')
    });

// Copies static assets to output directory
var copyPlugin =
    new CopyWebpackPlugin([{
        from: resolve('./public')
    }]);

// Enables hot reloading when code changes without refreshing
var hmrPlugin =
    new HotModuleReplacementPlugin();

// Configuration for webpack-dev-server
var devServer = {
    publicPath: '/',
    contentBase: resolve('./public'),
    host: '0.0.0.0',
    port: 8080,
    hot: true,
    inline: true,
    proxy: {
        // Redirect requests that start with /api/ to the server on port 8085
        '/api/**': {
            target: 'http://localhost:8085',
            changeOrigin: true
        },
        // Redirect websocket requests that start with /socket/ to the server on the port 8085
        // This is used by Hot Module Replacement
        '/socket/**': {
            target: 'http://localhost:8085',
            ws: true
        }
    }
};

// If we're running the webpack-dev-server, assume we're in development mode
var isProduction = !process.argv.find(v => v.indexOf('webpack-dev-server') !== -1);
console.log('Bundling for ' + (isProduction ? 'production' : 'development') + '...');

module.exports = {
    entry: { app: resolve('./Client.fsproj') },
    output: { path: resolve('./../../deploy/public') },
    resolve: { symlinks: false }, // See https://github.com/fable-compiler/Fable/issues/1490
    mode: isProduction ? 'production' : 'development',
    plugins: isProduction ? [htmlPlugin, copyPlugin] : [htmlPlugin, hmrPlugin],
    optimization: { splitChunks: { chunks: 'all' } },
    devServer: devServer,
    module: {
        rules: [
            {
                // transform F# into JS
                test: /\.fs(x|proj)?$/,
                use: { loader: 'fable-loader' }
            },
            {
                // transform JS to old syntax (compatible with old browsers)
                test: /\.js$/,
                exclude: /node_modules/,
                use: { loader: 'babel-loader' },
            }
        ]
    }
};