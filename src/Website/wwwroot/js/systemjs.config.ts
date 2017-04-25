(function (): void {
  SystemJS.config({
    // Map tells the System loader where to look for things
    map: {
      // Our app is within the app folder
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
      "angular-in-memory-web-api": "lib/angular-in-memory-web-api/bundles/in-memory-web-api.umd.js",
    },
    // Packages tells the System loader how to load when no filename and/or no extension
    packages: {
      app: { defaultExtension: "js" },
      rxjs: { defaultExtension: "js" },
    },
  });

  SystemJS.import("js/main.js").catch(console.error.bind(console));
})();
