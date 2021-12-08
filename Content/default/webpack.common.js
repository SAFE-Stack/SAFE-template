const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

function resolve(filePath) {
    return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}

function buildConfig(config, mode) {
    const isProduction = mode === 'production';

    // The HtmlWebpackPlugin allows us to use a template for the index.html page
    // and automatically injects <script> or <link> tags for generated bundles.
    const commonPlugins = [
        new HtmlWebpackPlugin({
            filename: 'index.html',
            template: resolve(config.indexHtmlTemplate)
        })
    ];

    return {
        // In development, split the JavaScript and CSS files in order to
        // have a faster HMR support. In production bundle styles together
        // with the code because the MiniCssExtractPlugin will extract the
        // CSS in a separate files.
        entry: {
            app: resolve(config.fsharpEntry)
        },
        // Add a hash to the output file name in production
        // to prevent browser caching if code changes
        output: {
            path: resolve(config.outputDir),
            publicPath: '/',
            filename: isProduction ? '[name].[contenthash].js' : '[name].js'
        },
        mode: mode,
        devtool: isProduction ? 'source-map' : 'eval-source-map',
        optimization: {
            runtimeChunk: 'single',
            moduleIds: 'deterministic',
            // Split the code coming from npm packages into a different file.
            // 3rd party dependencies change less often, let the browser cache them.
            splitChunks: {
                chunks: 'all'
            }
        },
        // Besides the HtmlPlugin, we use the following plugins:
        // PRODUCTION
        //      - MiniCssExtractPlugin: Extracts CSS from bundle to a different file
        //          To minify CSS, see https://github.com/webpack-contrib/mini-css-extract-plugin#minimizing-for-production
        //      - CopyWebpackPlugin: Copies static assets to output directory
        // DEVELOPMENT
        //      - HotModuleReplacementPlugin: Enables hot reloading when code changes without refreshing
        plugins: isProduction ?
            commonPlugins.concat([
                new MiniCssExtractPlugin({ filename: 'style.[name].[contenthash].css' }),
                new CopyWebpackPlugin({ patterns: [{ from: resolve(config.assetsDir) }] }),
            ])
            : commonPlugins,
        resolve: {
            // See https://github.com/fable-compiler/Fable/issues/1490
            symlinks: false
        },
        // Configuration for webpack-dev-server
        devServer: {
            static: {
                directory: resolve(config.assetsDir),
                publicPath: '/'
            },
            host: '0.0.0.0',
            port: config.devServerPort,
            proxy: config.devServerProxy,
            hot: true,
            historyApiFallback: true
        },
        // - sass-loaders: transforms SASS/SCSS into JS
        // - file-loader: Moves files referenced in the code (fonts, images) into output folder
        module: {
            rules: [
                {
                    test: /\.(sass|scss|css)$/,
                    use: [
                        isProduction
                            ? MiniCssExtractPlugin.loader
                            : 'style-loader',
                        'css-loader',
                        {
                            loader: 'sass-loader',
                            options: { implementation: require('sass') }
                        }
                    ],
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
                    use: ['file-loader']
                },
                {
                    test: /\.js$/,
                    enforce: 'pre',
                    use: ['source-map-loader'],
                }
            ]
        }
    };
}

module.exports = {
    buildConfig
};