const { src, dest, series, parallel } = require('gulp');
const less = require("gulp-less");

const coreScripts = 'Styles/**/*.less'; // your main scripts
const scriptsTask = function () {
    return src(coreScripts)
        .pipe(less())
        .pipe(dest('./wwwroot//css'));
};

exports.build = series(scriptsTask);
