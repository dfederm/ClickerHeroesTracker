Error.stackTraceLimit = Infinity;

require('reflect-metadata');

require('zone.js/dist/zone');
require('zone.js/dist/zone-testing');

var appContext = require.context('./src', true, /\.spec\.ts/);

appContext.keys().forEach(appContext);

var testing = require('@angular/core/testing');
var browser = require('@angular/platform-browser-dynamic/testing');

testing.getTestBed().initTestEnvironment(browser.BrowserDynamicTestingModule, browser.platformBrowserDynamicTesting());

// Prevent crashes when pretty printing DebugElements
jasmine.MAX_PRETTY_PRINT_DEPTH = 5;