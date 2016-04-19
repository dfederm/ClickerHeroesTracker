/// <binding AfterBuild='build' Clean='clean' ProjectOpened='watch' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    typescript = require("gulp-typescript"),
    typings = require("gulp-typings"),
    rename = require('gulp-rename');

var paths = {
    webroot: "./wwwroot/"
};

paths.typingsFile = "./typings/browser.d.ts";
paths.tsFiles = paths.webroot + "ts/**/*.ts";

paths.jsDir = paths.webroot + "js/";
paths.jsFiles = paths.jsDir + "**/*.js";
paths.jsMinFiles = paths.jsDir + "**/*.min.js";

paths.cssDir = paths.webroot + "css/";
paths.cssFiles = paths.cssDir + "**/*.css";
paths.cssMinFiles = paths.cssDir + "**/*.min.css";

gulp.task("clean:js", function (cb)
{
    rimraf(paths.jsFiles, cb);
});

gulp.task("clean:css", function (cb)
{
    rimraf(paths.cssMinFiles, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", ["ts"], function ()
{
    return gulp.src([paths.jsFiles, "!" + paths.jsMinFiles])
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(paths.jsDir));
});

gulp.task("min:css", function ()
{
    return gulp.src([paths.cssFiles, "!" + paths.cssMinFiles])
        .pipe(cssmin())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(paths.cssDir));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task("typings", function ()
{
    return gulp.src("./typings.json")
        .pipe(typings());
});

gulp.task("ts", ["typings"], function ()
{
    return gulp.src([paths.tsFiles, paths.typingsFile])
        .pipe(typescript({
            noEmitOnError: true,
            noImplicitAny: true,
            noImplicitReturns: true
        }))
        .pipe(gulp.dest(paths.jsDir));
});

gulp.task("build", ["ts", "min"]);

gulp.task("watch:ts", function ()
{
    gulp.watch(paths.tsFiles, ["ts"]);
});

gulp.task("watch:js", function ()
{
    gulp.watch([paths.jsFiles, "!" + paths.jsMinFiles], ["min:js"]);
});

gulp.task("watch:css", function ()
{
    gulp.watch([paths.cssFiles, "!" + paths.cssMinFiles], ["min:css"]);
});

gulp.task("watch", ["watch:ts", "watch:js", "watch:css"]);
