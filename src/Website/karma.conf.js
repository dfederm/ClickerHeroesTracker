module.exports = function(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: 'wwwroot/',

    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine'],

    // list of files / patterns to load in the browser
    files: [
      // System.js for module loading
      'lib/systemjs/dist/system.src.js',
      'lib/systemjs/dist/system-polyfills.js',

      // Polyfills
      'lib/core-js/client/shim.js',

      // zone.js
      'lib/zone.js/dist/zone.js',
      'lib/zone.js/dist/long-stack-trace-zone.js',
      'lib/zone.js/dist/proxy.js',
      'lib/zone.js/dist/sync-test.js',
      'lib/zone.js/dist/jasmine-patch.js',
      'lib/zone.js/dist/async-test.js',
      'lib/zone.js/dist/fake-async-test.js',

      // RxJs
      { pattern: 'lib/rxjs/**/*.js', included: false, watched: false },
      { pattern: 'lib/rxjs/**/*.js.map', included: false, watched: false },

      // Paths loaded via module imports:
      // Angular itself
      { pattern: 'lib/@angular/**/*.js', included: false, watched: false },
      { pattern: 'lib/@angular/**/*.js.map', included: false, watched: false },


      { pattern: 'js/systemjs.config.js', included: false, watched: false },
      '../karma-test-shim.js',

      { pattern: 'js/**/*.js', included: false, watched: true },
    ],

    // list of files to exclude
    exclude: [
      'js/**/*.min.js',
      'lib/**/*.spec.js',
    ],

    // test results reporter to use
    // possible values: 'dots', 'progress'
    // available reporters: https://npmjs.org/browse/keyword/karma-reporter
    reporters: ['progress', 'kjhtml'],

    // web server port
    port: 9876,

    // enable / disable colors in the output (reporters and logs)
    colors: true,

    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_INFO,

    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: false,

    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    browsers: ['PhantomJS'],

    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: true,

    // Concurrency level
    // how many browser should be started simultaneous
    concurrency: Infinity
  })
}
