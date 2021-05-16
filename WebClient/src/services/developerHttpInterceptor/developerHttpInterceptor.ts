import { Injectable } from "@angular/core";
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from "@angular/common/http";

import { Observable } from "rxjs";

// tslint:disable-next-line: use-injectable-provided-in
@Injectable()
export class DeveloperHttpInterceptor implements HttpInterceptor {
    // tslint:disable-next-line: no-any
    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Prepend the developer domain.
        req = req.clone({ url: `https://localhost:5001${req.url}` });
        return next.handle(req);
    }
}