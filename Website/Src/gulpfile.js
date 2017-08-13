/// <binding AfterBuild='build' Clean='clean' ProjectOpened='watch' />
"use strict";

var gulp = require("gulp");
var gutil = require('gulp-util');
var rimraf = require("rimraf");
var typescript = require("gulp-typescript");
var sourcemaps = require("gulp-sourcemaps");
var rename = require('gulp-rename');
var mergeStream = require("merge-stream");

var paths = {
    webroot: "./wwwroot/"
};

paths.jsDir = paths.webroot + "js/";
paths.jsFiles = paths.jsDir + "**/*.js";

paths.tsFiles = paths.jsDir + "**/*.ts";

paths.cssDir = paths.webroot + "css/";
paths.cssFiles = paths.cssDir + "**/*.css";
paths.cssMinFiles = paths.cssDir + "**/*.min.css";

paths.libDir = paths.webroot + "lib/";

gulp.task("clean:js", function (cb)
{
    rimraf(paths.jsFiles, cb);
});

gulp.task("clean:css", function (cb)
{
    rimraf(paths.cssMinFiles, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

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

gulp.task("copy", function ()
{
    var packages = [
        "@angular",
        "core-js",
        "rxjs",
        "systemjs",
        "systemjs-plugin-json",
        "zone.js",
        "@ng-bootstrap/ng-bootstrap",
        "ngx-clipboard",
        "ngx-window-token", // Required by ngx-clipboard
        "time-ago-pipe",
        "ng2-adsense",
    ];

    var merged = mergeStream();
    for (let pkg of packages)
    {
        merged.add(gulp.src("node_modules/" + pkg + "/**")
            .pipe(gulp.dest(paths.libDir + pkg)));
    }

    return merged;
});

gulp.task("build", ["js", "css", "copy"]);

gulp.task("test", ["js", "copy"], function (done)
{
    runKarma(done, { singleRun: true });
});

gulp.task("test-debug", ["js", "copy"], function (done)
{
    runKarma(done, { reporters: ['kjhtml'] });
});

function runKarma(done, config)
{
    config = config || {};
    config.configFile = __dirname + '/karma.conf.js';
    var karma = require("karma").Server;
    karma.start(config, function (exitStatus)
    {
        done(exitStatus ? new gutil.PluginError('karma', { message: 'Karma Tests failed' }) : undefined);
    });
}

gulp.task("watch", function ()
{
    gulp.watch([paths.tsFiles], ["js"]);
    gulp.watch([paths.cssFiles, "!" + paths.cssMinFiles], ["css"]);
});

gulp.task("test-watch", ["watch", "test-debug"]);
