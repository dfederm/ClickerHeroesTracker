// tslint:disable:no-require-imports
// tslint:disable:no-var-requires

import "core-js/es6";
import "core-js/es7/reflect";

require("zone.js/dist/zone");

if (process.env.ENV === "prod") {
    // Production
} else {
    // Development and test
    Error.stackTraceLimit = Infinity;
    require("zone.js/dist/long-stack-trace-zone");
}
