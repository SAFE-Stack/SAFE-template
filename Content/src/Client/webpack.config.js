var path = require("path");
var webpack = require("webpack");

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

var CONFIG = {
    fsharpEntry: [
        "whatwg-fetch",
        "@babel/polyfill",
        resolve("./Client.fsproj")
    ],
    devServerProxy: {
        '/api/*': {
            target: 'http://localhost:' + (process.env.SUAVE_FABLE_PORT || "8085"),
            changeOrigin: true
        }
    },
    historyApiFallback: {
        index: resolve("./index.html")
    },
    contentBase: resolve("./public"),
    // Use babel-preset-env to generate JS compatible with most-used browsers.
    // More info at https://github.com/babel/babel/blob/master/packages/babel-preset-env/README.md
    babel: {
        presets: [
            ["@babel/preset-env", {
                "targets": {
                    "browsers": ["last 2 versions"]
                },
                "modules": false,
                "useBuiltIns": "usage",
            }]
        ],
        plugins: ["@babel/plugin-transform-runtime"]
    }
}

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

module.exports = {
    devtool: isProduction ? undefined : "source-map",
    entry : CONFIG.fsharpEntry,
    mode: isProduction ? "production" : "development",
    output: {
        path: resolve('./public/js'),
        publicPath: "/js",
        filename: "bundle.js"
    },
    resolve: {
        symlinks: false,
        modules: [resolve("../../node_modules/")]
    },
    // Configuration for webpack-dev-server
    devServer: {
        proxy: CONFIG.devServerProxy,
        hot: true,
        inline: true,
        historyApiFallback: CONFIG.historyApiFallback,
        contentBase: CONFIG.contentBase
    },
    // - fable-loader: transforms F# into JS
    // - babel-loader: transforms JS to old syntax (compatible with old browsers)
    module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/,
                use: "fable-loader"
            },
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: CONFIG.babel
                },
            }
        ]
    },
    plugins: isProduction ? [] : [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
    ]
};
