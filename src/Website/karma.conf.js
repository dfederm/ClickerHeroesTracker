module.exports = function(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: 'wwwroot/',

    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine'],

    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('karma-junit-reporter')
    ],

    client: {
      // leave Jasmine Spec Runner output visible in browser
      clearContext: false
    },

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

      // Transpiled application & spec code paths loaded via module imports
      { pattern: 'js/**/*.js', included: false, watched: true },

      // Asset (HTML & CSS) paths loaded via Angular's component compiler
      { pattern: 'js/**/*.html', included: false, watched: true },
      ////{ pattern: 'js/**/*.css', included: false, watched: true },
    ],

    // Proxied base paths for loading assets
    proxies: {
      // required for modules fetched by SystemJS
      '/js/': '/base/js/'
    },

    // list of files to exclude
    exclude: [
      'js/**/*.min.js',
      'lib/**/*.spec.js',
    ],

    // test results reporter to use
    reporters: ['progress', 'coverage', 'kjhtml', 'junit'],

    preprocessors: {
      // source files, that you wanna generate coverage for
      // do not include tests or libraries
      // (these files will be instrumented by Istanbul)
      'js/**/!(*.spec).js': ['coverage']
    },

    coverageReporter: {
      dir: 'coverage',
      includeAllSources: true,
      reporters: [
        { type: 'html', subdir: 'report-html' },
        { type: 'cobertura', subdir: '.', file: 'cobertura.xml' },
      ]
    },

    junitReporter: {
      outputDir: '',
      outputFile: 'test-results.xml',
      useBrowserName: false,
    },

    // web server port
    port: 9876,

    // enable / disable colors in the output (reporters and logs)
    colors: true,

    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_INFO,

    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: true,

    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    browsers: ['Chrome'],

    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: false,

    // Concurrency level
    // how many browser should be started simultaneous
    concurrency: Infinity
  })
}
