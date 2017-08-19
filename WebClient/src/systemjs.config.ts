SystemJS.config({
  // Map tells the System loader where to look for things
  map: {
    // Our app is within the js folder
    "app": "js",

    // Angular bundles
    "@angular/animations": "lib/@angular/animations/bundles/animations.umd.js",
    "@angular/animations/browser": "lib/@angular/animations/bundles/animations-browser.umd.js",
    "@angular/core": "lib/@angular/core/bundles/core.umd.js",
    "@angular/common": "lib/@angular/common/bundles/common.umd.js",
    "@angular/compiler": "lib/@angular/compiler/bundles/compiler.umd.js",
    "@angular/platform-browser": "lib/@angular/platform-browser/bundles/platform-browser.umd.js",
    "@angular/platform-browser/animations": "lib/@angular/platform-browser/bundles/platform-browser-animations.umd.js",
    "@angular/platform-browser-dynamic": "lib/@angular/platform-browser-dynamic/bundles/platform-browser-dynamic.umd.js",
    "@angular/http": "lib/@angular/http/bundles/http.umd.js",
    "@angular/router": "lib/@angular/router/bundles/router.umd.js",
    "@angular/router/upgrade": "lib/@angular/router/bundles/router-upgrade.umd.js",
    "@angular/forms": "lib/@angular/forms/bundles/forms.umd.js",
    "@angular/upgrade": "lib/@angular/upgrade/bundles/upgrade.umd.js",
    "@angular/upgrade/static": "lib/@angular/upgrade/bundles/upgrade-static.umd.js",

    // Other libraries
    "rxjs": "lib/rxjs",
    "@ng-bootstrap/ng-bootstrap": "lib/@ng-bootstrap/ng-bootstrap/bundles/ng-bootstrap.js",
    "ngx-clipboard/dist": "lib/ngx-clipboard/dist/bundles/ngxClipboard.umd.js",
    "ngx-window-token": "lib/ngx-window-token/dist/bundles/ngxWindowToken.umd.js", // Required by ngx-clipboard
    "time-ago-pipe": "lib/time-ago-pipe/time-ago-pipe.js",
    "ng2-adsense": "lib/ng2-adsense/ng2-adsense.umd.js",
    "decimal.js": "lib/decimal.js/decimal.min.js",
    "toFormat": "lib/toFormat/toFormat.js",
  },

  meta: {
    "*.json": {
      loader: "lib/systemjs-plugin-json/json.js",
    },
  },

  // Packages tells the System loader how to load when no filename and/or no extension
  packages: {
    app: { defaultExtension: "js" },
    rxjs: { defaultExtension: "js" },
  },
});
