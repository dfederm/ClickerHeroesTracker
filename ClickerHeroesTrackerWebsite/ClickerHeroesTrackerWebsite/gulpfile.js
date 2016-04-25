/// <binding AfterBuild='build' Clean='clean' ProjectOpened='watch' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    cleanCSS = require("gulp-clean-css"),
    uglify = require("gulp-uglify"),
    typescript = require("gulp-typescript"),
    typings = require("gulp-typings"),
    rename = require('gulp-rename'),
    tslint = require("gulp-tslint");

var paths = {
    webroot: "./wwwroot/"
};

paths.jsDir = paths.webroot + "js/";
paths.jsFiles = paths.jsDir + "**/*.js";

paths.typingsConfig = "./typings.json";
paths.typingsFiles = "./typings/**/*.d.ts";
paths.ambientTypingsFiles = "./typings/browser/ambient/**/*.d.ts";

paths.tsFiles = paths.jsDir + "**/*.ts";

paths.cssDir = paths.webroot + "css/";
paths.cssFiles = paths.cssDir + "**/*.css";
paths.cssMinFiles = paths.cssDir + "**/*.min.css";

gulp.task("clean:typings", function (cb)
{
    rimraf(paths.typingsFiles, cb);
});

gulp.task("clean:js", function (cb)
{
    rimraf(paths.jsFiles, cb);
});

gulp.task("clean:css", function (cb)
{
    rimraf(paths.cssMinFiles, cb);
});

gulp.task("clean", ["clean:typings", "clean:js", "clean:css"]);

gulp.task("typings", function ()
{
    return gulp.src(paths.typingsConfig)
        .pipe(typings());
});

gulp.task("tslint", function ()
{
    return gulp.src(paths.tsFiles)
        .pipe(tslint({
            configuration: "tslint.json",
            tslint: require("tslint")
        }))
        .pipe(tslint.report("verbose"));
});

var tsProject = typescript.createProject('tsconfig.json', {
    typescript: require('typescript')
});

gulp.task("js", ["typings", "tslint"], function ()
{
    return tsProject.src()
        .pipe(typescript(tsProject))
        .pipe(gulp.dest(paths.jsDir))
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(paths.jsDir));
});

gulp.task("css", function ()
{
    return gulp.src([paths.cssFiles, "!" + paths.cssMinFiles])
        .pipe(cleanCSS())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(paths.cssDir));
});

gulp.task("build", ["js", "css"]);

gulp.task("watch", function ()
{
    gulp.watch([paths.tsFiles, paths.typingsConfig], ["js"]);
    gulp.watch([paths.cssFiles, "!" + paths.cssMinFiles], ["css"]);
});
