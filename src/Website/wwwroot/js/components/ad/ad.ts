import { Component, AfterViewInit } from "@angular/core";

declare global
{
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window
    {
        adsbygoogle: object[];
    }
}

@Component({
    selector: "ad",
    templateUrl: "./js/components/ad/ad.html",
})
export class AdComponent implements AfterViewInit
{
    public ngAfterViewInit(): void
    {
        (window.adsbygoogle = window.adsbygoogle || []).push({});
     }
}
