var path = require('path')

module.exports = {
    entry: "./tests/Client.Tests/SAFE.App.Client.Tests.fsproj",
    output: {
        path: path.join(__dirname, "./tests/Client.Tests"),
        filename: "SAFE.App.Client.Tests.js",
    },
    devServer: {
        contentBase: "./tests/Client.Tests",
        port: 8081,
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}