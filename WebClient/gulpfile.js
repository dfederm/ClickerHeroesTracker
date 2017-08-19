/// <binding AfterBuild='build' Clean='clean' ProjectOpened='watch' />
"use strict";

var gulp = require("gulp");
var gutil = require('gulp-util');
var rimraf = require("rimraf");
var typescript = require("gulp-typescript");
var sourcemaps = require("gulp-sourcemaps");
var mergeStream = require("merge-stream");

var paths = {
    src: "./src/",
    data: "../Website/Src/wwwroot/data/",
    outputRoot: "./dist/",
};

paths.tsFiles = paths.src + "**/*.ts";
paths.htmlFiles = paths.src + "**/*.html";
paths.cssFiles = paths.src + "**/*.css";
paths.dataFiles = paths.data + "**/*.json";

paths.appOutDir = paths.outputRoot + "js/";
paths.libOutDir = paths.outputRoot + "lib/";
paths.dataOutDir = paths.outputRoot + "data/";

gulp.task("clean", cb =>
{
    rimraf(paths.outputRoot, cb);
});

gulp.task("tslint", () =>
{
    var gulpTslint = require("gulp-tslint");
    var tslint = require("tslint");

    return gulp.src(paths.tsFiles)
        .pipe(gulpTslint({
            configuration: "./tslint.json",
            tslint: tslint,
            formatter: "verbose",
            program: tslint.Linter.createProgram("./tsconfig.json")
        }))
        .pipe(gulpTslint.report());
});

var tsProject = typescript.createProject('./tsconfig.json', {
    typescript: require('typescript')
});

gulp.task("js", ["tslint"], () =>
{
    var uglify = require("gulp-uglify");

    return tsProject.src()
        .pipe(sourcemaps.init())
        .pipe(tsProject())
        .pipe(sourcemaps.write())
        .pipe(gulp.dest(paths.appOutDir));
});

gulp.task("copy", () =>
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
        "decimal.js",
        "toFormat",
    ];

    var merged = mergeStream();
    for (let pkg of packages)
    {
        merged.add(gulp.src("node_modules/" + pkg + "/**")
            .pipe(gulp.dest(paths.libOutDir + pkg)));
    }

    // Also copy some other app assets
    merged.add(gulp.src(paths.htmlFiles)
        .pipe(gulp.dest(paths.appOutDir)));
    merged.add(gulp.src(paths.cssFiles)
        .pipe(gulp.dest(paths.appOutDir)));

    // Also copy data from the Website for tests. We should find a better way to share data files between projects
    merged.add(gulp.src(paths.dataFiles)
        .pipe(gulp.dest(paths.dataOutDir)));

    return merged;
});

gulp.task("build", ["js", "copy"]);

gulp.task("test", ["build"], done =>
{
    runKarma(done, { singleRun: true });
});

gulp.task("test-debug", ["js", "copy"], done =>
{
    runKarma(done, { reporters: ['kjhtml'] });
});

function runKarma(done, config)
{
    config = config || {};
    config.configFile = __dirname + '/karma.conf.js';
    var karma = require("karma").Server;
    karma.start(config, exitStatus =>
    {
        done(exitStatus ? new gutil.PluginError('karma', { message: 'Karma Tests failed' }) : undefined);
    });
}

gulp.task("watch", () =>
{
    gulp.watch(paths.tsFiles, ["js"]);
    gulp.watch([paths.htmlFiles, paths.cssFiles], ["copy"]);
});

gulp.task("test-watch", ["watch", "test-debug"]);
