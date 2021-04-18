var path = require("path");

module.exports = {
    entry: "./tests/Tests.fs.js",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js",
    },
    devServer: {
        contentBase: "./public",
        port: 8080,
    }
}