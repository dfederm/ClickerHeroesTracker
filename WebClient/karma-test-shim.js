// /*global jasmine, __karma__, window*/
Error.stackTraceLimit = 0; // "No stacktrace"" is usually best for app testing.

// Uncomment to get full stacktrace output. Sometimes helpful, usually not.
// Error.stackTraceLimit = Infinity; //

jasmine.DEFAULT_TIMEOUT_INTERVAL = 1000;

__karma__.loaded = function () { };

function isSpecFile(path)
{
  return /\.spec\.(.*\.)?js$/.test(path);
}

var allSpecFiles = Object.keys(window.__karma__.files).filter(isSpecFile);

System.config({
  baseURL: 'base/',
  // Map the angular testing umd bundles
  map: {
    '@angular/core/testing': 'lib/@angular/core/bundles/core-testing.umd.js',
    '@angular/common/testing': 'lib/@angular/common/bundles/common-testing.umd.js',
    '@angular/compiler/testing': 'lib/@angular/compiler/bundles/compiler-testing.umd.js',
    '@angular/platform-browser/testing': 'lib/@angular/platform-browser/bundles/platform-browser-testing.umd.js',
    '@angular/platform-browser-dynamic/testing': 'lib/@angular/platform-browser-dynamic/bundles/platform-browser-dynamic-testing.umd.js',
    '@angular/http/testing': 'lib/@angular/http/bundles/http-testing.umd.js',
    '@angular/router/testing': 'lib/@angular/router/bundles/router-testing.umd.js',
    '@angular/forms/testing': 'lib/@angular/forms/bundles/forms-testing.umd.js',
  },
});

System.import('js/systemjs.config.js')
  .then(initTestBed)
  .then(initTesting);

function initTestBed()
{
  return Promise.all([
    System.import('@angular/core/testing'),
    System.import('@angular/platform-browser-dynamic/testing')
  ])
    .then(function (providers)
    {
      var coreTesting = providers[0];
      var browserTesting = providers[1];

      coreTesting.TestBed.initTestEnvironment(
        browserTesting.BrowserDynamicTestingModule,
        browserTesting.platformBrowserDynamicTesting());
    })
}

// Import all spec files and start karma
function initTesting()
{
  return Promise.all(
    allSpecFiles.map(function (moduleName)
    {
      return System.import(moduleName);
    })
  )
    .then(__karma__.start, __karma__.error);
}
