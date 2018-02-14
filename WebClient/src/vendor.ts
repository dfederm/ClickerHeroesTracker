// tslint:disable:no-require-imports
// tslint:disable:no-var-requires

// Angular
if (process.env.ENV === "prod") {
    require("@angular/platform-browser");

    // Force the ngfactories into this chunk
    require("@ng-bootstrap/ng-bootstrap/index.ngfactory");
    require("@ng-bootstrap/ng-bootstrap/pagination/pagination.ngfactory");
    require("@ng-bootstrap/ng-bootstrap/tabset/tabset.ngfactory");
    require("@ng-bootstrap/ng-bootstrap/progressbar/progressbar.ngfactory");
    require("ng2-adsense/ng2-adsense.ngfactory");
    require("ngx-loading/ngx-loading.ngfactory");
    require("jw-bootstrap-switch-ng2/dist/directive.ngfactory");
} else {
    require("@angular/platform-browser-dynamic");
}

import "@angular/core";
import "@angular/common";
import "@angular/common/http";
import "@angular/router";
import "@ng-bootstrap/ng-bootstrap";
import "@markpieszak/ng-application-insights";
import "angular-compare-validator";
import "crypto-js";
import "decimal.js";
import "jw-bootstrap-switch-ng2";
import "jwt-decode";
import "msal";
import "ng2-adsense";
import "ng2-charts";
import "ngx-clipboard";
import "ngx-loading";
import "pako";
import "rxjs/_esm5/operators";
import "time-ago-pipe";
import "toformat";
