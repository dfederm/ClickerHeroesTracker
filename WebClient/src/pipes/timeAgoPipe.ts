import { Pipe, PipeTransform, NgZone, ChangeDetectorRef, OnDestroy } from "@angular/core";

// Adjusted from https://github.com/AndrewPoyntz/time-ago-pipe, which doesn't support newer version of Angular
@Pipe({
    name: "timeAgo",
    pure: false,
    standalone: true,
})
export class TimeAgoPipe implements PipeTransform, OnDestroy {
    private timer: number;

    constructor(
        private readonly changeDetectorRef: ChangeDetectorRef,
        private readonly ngZone: NgZone,
    ) {
    }

    public transform(value: string): string {
        this.removeTimer();
        let d = new Date(value);
        let now = new Date();
        let seconds = Math.round(Math.abs((now.getTime() - d.getTime()) / 1000));
        let timeToUpdate = (Number.isNaN(seconds)) ? 1000 : this.getSecondsUntilUpdate(seconds) * 1000;
        this.timer = this.ngZone.runOutsideAngular(() => {
            if (typeof window !== "undefined") {
                return window.setTimeout(
                    () => {
                        this.ngZone.run(() => this.changeDetectorRef.markForCheck());
                    },
                    timeToUpdate);
            }
            return null;
        });
        let minutes = Math.round(Math.abs(seconds / 60));
        let hours = Math.round(Math.abs(minutes / 60));
        let days = Math.round(Math.abs(hours / 24));
        let months = Math.round(Math.abs(days / 30.416));
        let years = Math.round(Math.abs(days / 365));
        if (Number.isNaN(seconds)) {
            return "";
        } else if (seconds <= 45) {
            return "a few seconds ago";
        } else if (seconds <= 90) {
            return "a minute ago";
        } else if (minutes <= 45) {
            return minutes + " minutes ago";
        } else if (minutes <= 90) {
            return "an hour ago";
        } else if (hours <= 22) {
            return hours + " hours ago";
        } else if (hours <= 36) {
            return "a day ago";
        } else if (days <= 25) {
            return days + " days ago";
        } else if (days <= 45) {
            return "a month ago";
        } else if (days <= 345) {
            return months + " months ago";
        } else if (days <= 545) {
            return "a year ago";
        } else { // (days > 545)
            return years + " years ago";
        }
    }

    public ngOnDestroy(): void {
        this.removeTimer();
    }

    private removeTimer(): void {
        if (this.timer) {
            window.clearTimeout(this.timer);
            this.timer = null;
        }
    }

    private getSecondsUntilUpdate(seconds: number): number {
        let min = 60;
        let hr = min * 60;
        let day = hr * 24;
        if (seconds < min) { // Less than 1 min, update every 2 secs
            return 2;
        } else if (seconds < hr) { // Less than an hour, update every 30 secs
            return 30;
        } else if (seconds < day) { // Less then a day, update every 5 mins
            return 300;
        } else { // Update every hour
            return 3600;
        }
    }
}