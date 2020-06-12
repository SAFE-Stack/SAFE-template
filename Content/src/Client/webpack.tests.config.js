var path = require('path')

module.exports = {
    entry: "../../tests/Client/Client.Tests.fsproj",
    output: {
        path: path.join(__dirname, "../../tests/Client"),
        filename: "Client.Tests.js",
    },
    devServer: {
        contentBase: "../../tests/Client",
        port: 8081,
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}