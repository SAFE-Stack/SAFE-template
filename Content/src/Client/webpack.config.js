var path = require("path");
var webpack = require("webpack");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var MiniCssExtractPlugin = require("mini-css-extract-plugin");
var MinifyPlugin = require("terser-webpack-plugin");

var CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: "./public/index.html",
    fsharpEntry: "./Client.fsproj",
    cssEntry: "./public/style.sass",
    outputDir: "./deploy",
    assetsDir: "./public",
    devServerPort: 8080,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: undefined,
    // Use babel-preset-env to generate JS compatible with most-used browsers.
    // More info at https://babeljs.io/docs/en/next/babel-preset-env.html
    babel: {
        presets: [
            ["@babel/preset-env", {
                "targets": "> 0.25%, not dead",
                "modules": false,
                // This adds polyfills when needed. Requires core-js dependency.
                // See https://babeljs.io/docs/en/babel-preset-env#usebuiltins
                "useBuiltIns": "usage",
            }]
        ],
    }
}

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

// If we're running the webpack-dev-server, assume we're in development mode
var isProduction = !process.argv.find(v => v.indexOf('webpack-dev-server') !== -1);
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");


// The HtmlWebpackPlugin allows us to use a template for the index.html page
// and automatically injects <script> or <link> tags for generated bundles.
var commonPlugins = [
    new HtmlWebpackPlugin({
        filename: 'index.html',
        template: resolve(CONFIG.indexHtmlTemplate)
    })
];

// Use babel-preset-env to generate JS compatible with most-used browsers.
// More info at https://github.com/babel/babel/blob/master/packages/babel-preset-env/README.md
var babelOptions = {
    presets: [
        ["@babel/preset-env", {
            "targets": {
                "browsers": ["last 2 versions"]
            },
            "modules": false,
            "useBuiltIns": "usage",
        }]
    ],
    // A plugin that enables the re-use of Babel's injected helper code to save on codesize.
    plugins: ["@babel/plugin-transform-runtime"]
};

module.exports = {
     // In development, bundle styles together with the code so they can also
    // trigger hot reloads. In production, put them in a separate CSS file.
    entry: isProduction ? {
        app: [resolve(CONFIG.fsharpEntry), resolve(CONFIG.cssEntry)]
    } : {
            app: [resolve(CONFIG.fsharpEntry)],
            style: [resolve(CONFIG.cssEntry)]
        },
    output: {
        path: resolve('./public/js'),
        publicPath: "/js",
        filename: "[name].js"
    },
    // Set up default webpack optimisations for prod or dev builds
    mode: isProduction ? "production" : "development",
    // Turn on source maps when debugging
    devtool: isProduction ? undefined : "source-map",
    // Turn off symlinks for module resolution
    resolve: { symlinks: false },
    optimization: {
        // Split the code coming from npm packages into a different file.
        // 3rd party dependencies change less often, let the browser cache them.
        splitChunks: {
            cacheGroups: {
                commons: {
                    test: /node_modules/,
                    name: "vendors",
                    chunks: "all"
                }
            }
        },
        // In production, turn on minification to make JS files smaller
        minimizer: isProduction ? [new MinifyPlugin({
            terserOptions: {
              compress: {
                  // See https://github.com/SAFE-Stack/SAFE-template/issues/190
                  inline: false
              }
            }
          })] : []
    },
    // In development, enable hot reloading when code changes without refreshing the browser or losing state.
    plugins: isProduction ? [] : [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
    ],
    // Configuration for the development server
    devServer: {
        // redirect requests that start with /api/* to the server on port 8085
        proxy: {
            '/api/*': {
                target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
                changeOrigin: true
            }
        },
        // turn on hot module reloading
        hot: true,
        // more automatic reloading
        inline: true,
        // default page
        historyApiFallback: { index: resolve("./index.html") },
        // where to server static files from
        contentBase: resolve("./public")
    },
    // The modules that are used by webpack for transformations
    module: {
        rules: [
            {
                // - fable-loader: transforms F# into JS
                test: /\.fs(x|proj)?$/,
                use: {
                    loader: "fable-loader",
                    options: {
                        babel: babelOptions,
                        define: isProduction ? [] : ["DEBUG"]
                   }
                },
            },
            {
                // - babel-loader: transforms JS to old syntax (compatible with old browsers)
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: babelOptions
                },
            }
        ]
    }
};
