import { Injectable } from "@angular/core";
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from "@angular/common/http";

import { Observable } from "rxjs";

// eslint-disable-next-line @angular-eslint/use-injectable-provided-in
@Injectable()
export class DeveloperHttpInterceptor implements HttpInterceptor {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Prepend the developer domain.
        req = req.clone({ url: `https://localhost:5001${req.url}` });
        return next.handle(req);
    }
}