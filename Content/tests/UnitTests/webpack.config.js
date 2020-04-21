var path = require('path')

module.exports = {
    entry: "./tests/UnitTests/UnitTests.fsproj",
    output: {
        path: path.join(__dirname, "./tests/UnitTests"),
        filename: "Client.Tests.js",
    },
    devServer: {
        contentBase: "./tests/UnitTests",
        port: 8081,
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}