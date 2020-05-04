module.exports = function (config) {
  config.set({
    basePath: '',

    frameworks: ['jasmine'],

    files: [
      { pattern: './karma-test-shim.js', watched: false }
    ],

    preprocessors: {
      './karma-test-shim.js': ['webpack', 'sourcemap']
    },

    webpack: require('./webpack.config')(),

    webpackMiddleware: {
      stats: 'errors-only'
    },

    webpackServer: {
      noInfo: true
    },

    // test results reporter to use
    reporters: ['progress', 'coverage-istanbul', 'kjhtml', 'junit'],

    coverageIstanbulReporter: {
      reports: ['html', 'cobertura', 'text-summary'],
      dir: './logs/coverage',
      fixWebpackSourcePaths: true,
      'report-config': {
        html: {
          subdir: 'html'
        },
        cobertura: {
          file: 'cobertura.xml'
        }
      },
      thresholds: {
        emitWarning: false,
        // thresholds for all files
        global: {
          // TODO: Increase to 90
          statements: 85,
        },
        // thresholds per file. Really should raise this, but the game models hold it back.
        each: {
          statements: 45,
        }
      },
    },

    junitReporter: {
      outputDir: '',
      outputFile: './logs/test-results.xml',
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
    autoWatch: false,

    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    browsers: ['ChromeHeadless'],

    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: true,

    // Concurrency level
    // how many browser should be started simultaneous
    concurrency: Infinity,

    // How long will Karma wait for a message from a browser before disconnecting from it (in ms).
    // Sometimes dealing with very large numbers can cause a small hang.
    browserNoActivityTimeout: 60000,
  });
};
