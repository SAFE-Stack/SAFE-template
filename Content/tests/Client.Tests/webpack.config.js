var path = require('path')

module.exports = {
    entry: "./tests/Client.Tests/Client.Tests.fsproj",
    output: {
        path: path.join(__dirname, "./tests/Client.Tests"),
        filename: "Client.Tests.js",
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