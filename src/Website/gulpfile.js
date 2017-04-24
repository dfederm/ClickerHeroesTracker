/// <binding AfterBuild='build' Clean='clean' ProjectOpened='watch' />
"use strict";

var gulp = require("gulp");
var rimraf = require("rimraf");
var typescript = require("gulp-typescript");
var rename = require('gulp-rename');
var mergeStream = require("merge-stream");

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

paths.libDir = paths.webroot + "lib/";

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
    var typings = require("gulp-typings");
    return gulp.src(paths.typingsConfig)
        .pipe(typings());
});

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

gulp.task("js", ["typings", "tslint"], function ()
{
    var uglify = require("gulp-uglify");

    return tsProject.src()
        .pipe(tsProject())
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

gulp.task("copy", function ()
{
    var packages = [
        "@angular",
        "core-js",
        "rxjs",
        "systemjs",
        "zone.js",
    ];

    var merged = mergeStream();
    for (let pkg of packages)
    {
        merged.add(gulp.src("node_modules/" + pkg + "/**", { read: false })
            .pipe(gulp.dest(paths.libDir + pkg)));
    }

    return merged;
});

gulp.task("build", ["js", "css", "copy"]);

gulp.task("watch", function ()
{
    gulp.watch([paths.tsFiles, paths.typingsConfig], ["js"]);
    gulp.watch([paths.cssFiles, "!" + paths.cssMinFiles], ["css"]);
});
