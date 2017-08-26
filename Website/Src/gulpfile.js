/// <binding AfterBuild='build' Clean='clean' ProjectOpened='watch' />
"use strict";

var gulp = require("gulp");
var rimraf = require("rimraf");
var typescript = require("gulp-typescript");
var sourcemaps = require("gulp-sourcemaps");
var rename = require('gulp-rename');

var paths = {
    webroot: "./wwwroot/"
};

paths.jsDir = paths.webroot + "js/";
paths.jsFiles = paths.jsDir + "**/*.js";

paths.tsFiles = paths.jsDir + "**/*.ts";
paths.htmlFiles = paths.jsDir + "**/*.html";

paths.cssDir = paths.webroot + "css/";
paths.cssFiles = paths.cssDir + "**/*.css";
paths.cssMinFiles = paths.cssDir + "**/*.min.css";

paths.dataDir = paths.webroot + "data/";
paths.libDir = paths.webroot + "lib/";

paths.webClient = "../../WebClient/dist/*.*";

gulp.task("clean:js", function (cb)
{
    rimraf(paths.jsFiles, cb);
});

gulp.task("clean:css", function (cb)
{
    rimraf(paths.cssMinFiles, cb);
});

gulp.task("clean:html", function (cb)
{
    rimraf(paths.htmlFiles, cb);
});

gulp.task("clean", ["clean:js", "clean:css", "clean:html"]);

gulp.task("tslint", function ()
{
    var gulpTslint = require("gulp-tslint");
    var tslint = require("tslint");

    return gulp.src(paths.tsFiles)
        .pipe(gulpTslint({
            configuration: "tslint.json",
            tslint: tslint,
            formatter: "verbose",
            program: tslint.Linter.createProgram("./tsconfig.json")
        }))
        .pipe(gulpTslint.report());
});

var tsProject = typescript.createProject('tsconfig.json', {
    typescript: require('typescript')
});

gulp.task("js", ["tslint"], function ()
{
    var uglify = require("gulp-uglify");

    return tsProject.src()
        .pipe(sourcemaps.init())
        .pipe(tsProject())
        .pipe(sourcemaps.write())
        .pipe(gulp.dest(paths.jsDir))
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(paths.jsDir));
});

gulp.task("css", function ()
{
    var cleanCSS = require("gulp-clean-css");

    return gulp.src([paths.cssFiles, "!" + paths.cssMinFiles])
        .pipe(cleanCSS())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(paths.cssDir));
});

gulp.task("copy", function () {
    return gulp.src(paths.webClient)
        .pipe(gulp.dest(paths.webroot));
});

gulp.task("build", ["js", "css", "copy"]);

gulp.task("watch", function ()
{
    gulp.watch(paths.webClient, ["copy"]);
    gulp.watch(paths.tsFiles, ["js"]);
    gulp.watch([paths.cssFiles, "!" + paths.cssMinFiles], ["css"]);
});
