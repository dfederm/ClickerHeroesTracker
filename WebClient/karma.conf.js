module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma')
    ],
    client: {
      jasmine: {
        // you can add configuration options for Jasmine here
        // the possible options are listed at https://jasmine.github.io/api/edge/Configuration.html
        // for example, you can disable the random execution with `random: false`
        // or set a specific seed with `seed: 4321`
      },
      clearContext: false // leave Jasmine Spec Runner output visible in browser
    },
    jasmineHtmlReporter: {
      suppressAll: true // removes the duplicated traces
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './logs/coverage'),
      subdir: '.',
      includeAllSources: true,
      reporters: [
        {
          type: 'html',
          subdir: 'html'
        },
        {
          type: 'text-summary'
        },
        {
          type: 'cobertura',
          file: 'cobertura.xml',
        },
      ],
      check: {
        // thresholds for all files
        global: {
          statements: 80,
          branches: 70,
          functions: 85,
          lines: 80
        },
        // thresholds per file
        each: {
          statements: 75,
          branches: 50,
          functions: 75,
          lines: 75,
          excludes: [
            // Models are ported from game code so some code might not be used
            'src/models/*.ts',
            // timeAgoPipe was largely copied from elsewhere.
            'src/pipes/timeAgoPipe.ts',
            // The logging service is a very thin wrapper and doesn't need coverage.
            'src/services/loggingService/loggingService.ts',
          ]
        }
      }
    },
    reporters: ['progress', 'kjhtml'],
    port: 9876,
    colors: true,
    logLevel: config.LOG_INFO,
    autoWatch: true,
    browsers: ['Chrome'],
    customLaunchers: {
      ChromeHeadlessCI: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox']
      }
    },
    singleRun: false,
    restartOnFileChange: true
  });
};
