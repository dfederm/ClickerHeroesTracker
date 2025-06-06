import { Component, Directive, Input, LOCALE_ID } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { ActivatedRoute, NavigationExtras, Params, provideRouter, Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import Decimal from "decimal.js";

import { UserProgressComponent } from "./userProgress";
import { UserService, IProgressData } from "../../services/userService/userService";
import { ChartDataset, ChartOptions, ScatterDataPoint } from "chart.js";
import { SettingsService } from "../../services/settingsService/settingsService";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { PercentPipe } from "@angular/common";
import { NgxSpinnerModule } from "ngx-spinner";
import { BaseChartDirective } from "ng2-charts";

describe("UserProgressComponent", () => {
    let component: UserProgressComponent;
    let fixture: ComponentFixture<UserProgressComponent>;
    let routeParams: BehaviorSubject<Params>;
    let queryParams: BehaviorSubject<Params>;

    @Component({ selector: "ngx-spinner", template: "" })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    @Directive({
        selector: "canvas[baseChart]",
    })
    class MockBaseChartDirective {
        @Input()
        public type: string;

        @Input()
        public datasets: ChartDataset<"line">[];

        @Input()
        public options: ChartOptions<"line">;
    }
    
    let progress: IProgressData = {
        soulsSpentData: createData(0),
        titanDamageData: createData(1),
        heroSoulsSacrificedData: createData(2),
        totalAncientSoulsData: createData(3),
        transcendentPowerData: createData(4, false),
        rubiesData: createData(5),
        highestZoneThisTranscensionData: createData(6),
        highestZoneLifetimeData: createData(7),
        ascensionsThisTranscensionData: createData(8),
        ascensionsLifetimeData: createData(9),
        outsiderLevelData: {
            outsider0: createData(10),
            outsider1: createData(11),
            outsider2: createData(12),
        },
        ancientLevelData: {
            ancient0: createData(13),
            ancient1: createData(14),
            ancient2: createData(15),
        },
    };

    let expectedChartOrder = [
        { title: "Souls Spent", isLogarithmic: true, isPercent: false, data: progress.soulsSpentData },
        { title: "Titan Damage", isLogarithmic: false, isPercent: false, data: progress.titanDamageData },
        { title: "Hero Souls Sacrificed", isLogarithmic: true, isPercent: false, data: progress.heroSoulsSacrificedData },
        { title: "Total Ancient Souls", isLogarithmic: false, isPercent: false, data: progress.totalAncientSoulsData },
        { title: "Transcendent Power", isLogarithmic: false, isPercent: true, data: progress.transcendentPowerData },
        { title: "Rubies", isLogarithmic: false, isPercent: false, data: progress.rubiesData },
        { title: "Highest Zone This Transcension", isLogarithmic: true, isPercent: false, data: progress.highestZoneThisTranscensionData },
        { title: "Highest Zone Lifetime", isLogarithmic: false, isPercent: false, data: progress.highestZoneLifetimeData },
        { title: "Ascensions This Transcension", isLogarithmic: true, isPercent: false, data: progress.ascensionsThisTranscensionData },
        { title: "Ascensions Lifetime", isLogarithmic: false, isPercent: false, data: progress.ascensionsLifetimeData },
        { title: "Outsider0", isLogarithmic: true, isPercent: false, data: progress.outsiderLevelData.outsider0 },
        { title: "Outsider1", isLogarithmic: false, isPercent: false, data: progress.outsiderLevelData.outsider1 },
        { title: "Outsider2", isLogarithmic: true, isPercent: false, data: progress.outsiderLevelData.outsider2 },
        { title: "Ancient0", isLogarithmic: false, isPercent: false, data: progress.ancientLevelData.ancient0 },
        { title: "Ancient1", isLogarithmic: true, isPercent: false, data: progress.ancientLevelData.ancient1 },
        { title: "Ancient2", isLogarithmic: false, isPercent: false, data: progress.ancientLevelData.ancient2 },
    ];

    const settings = SettingsService.defaultSettings;
    let settingsSubject = new BehaviorSubject(settings);

    const locale = "en-us";
    const percentPipe = new PercentPipe(locale);

    beforeEach(async () => {
        let userService = {
            getProgress(): Promise<IProgressData> {
                return Promise.resolve(progress);
            },
        };

        let settingsService = { settings: () => settingsSubject };

        routeParams = new BehaviorSubject({ userName: "someUserName" });
        queryParams = new BehaviorSubject({});
        let route = { params: routeParams, queryParams: queryParams };

        await TestBed.configureTestingModule(
            {
                imports: [UserProgressComponent],
                providers: [
                    provideRouter([]),
                    { provide: ActivatedRoute, useValue: route },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: LOCALE_ID, useValue: locale },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(UserProgressComponent, {
            remove: { imports: [ NgxSpinnerModule, BaseChartDirective ]},
            add: { imports: [ MockNgxSpinnerComponent, MockBaseChartDirective ] },
        });

        fixture = TestBed.createComponent(UserProgressComponent);
        component = fixture.componentInstance;

        let router = TestBed.inject(Router);
        spyOn(router, "navigate").and.callFake((_commands: any[], extras?: NavigationExtras) => {
            if (extras.queryParams) {
                queryParams.next(extras.queryParams);
            }

            return Promise.resolve(true);
        });
    });

    describe("Range Selector", () => {
        it("should display", () => {
            fixture.detectChanges();

            expect(component.selectedRange).toEqual("1w");

            let rangeSelector = fixture.debugElement.query(By.css(".btn-group"));
            expect(rangeSelector).not.toBeNull();

            let ranges = rangeSelector.queryAll(By.css("button"));
            expect(ranges.length).toEqual(component.ranges.length);
            for (let i = 0; i < ranges.length; i++) {
                expect(ranges[i].nativeElement.textContent.trim()).toEqual(component.ranges[i]);
            }

            let disabledRanges = ranges.filter(range => range.classes.disabled);
            expect(disabledRanges.length).toEqual(1);
            expect(disabledRanges[0].nativeElement.textContent.trim()).toEqual(component.selectedRange);
        });

        it("should change the current date range when clicked", () => {
            fixture.detectChanges();

            expect(component.selectedRange).toEqual("1w");

            let rangeSelector = fixture.debugElement.query(By.css(".btn-group"));
            expect(rangeSelector).not.toBeNull();

            let ranges = rangeSelector.queryAll(By.css("button"));
            expect(ranges.length).toEqual(component.ranges.length);

            ranges[ranges.length - 1].nativeElement.click();

            fixture.detectChanges();

            expect(component.selectedRange).toEqual(component.ranges[ranges.length - 1]);

            let disabledRanges = ranges.filter(range => range.classes.disabled);
            expect(disabledRanges.length).toEqual(1);
            expect(disabledRanges[0].nativeElement.textContent.trim()).toEqual(component.selectedRange);
        });

        it("should request the proper date range", () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.callThrough();

            fixture.detectChanges();

            let now = Date.now();
            spyOn(Date, "now").and.returnValue(now);

            let expectedEnd = new Date(now);
            let expectedStarts: { [dateRange: string]: Date } = {
                "1d": new Date(new Date(now).setDate(expectedEnd.getDate() - 1)),
                "3d": new Date(new Date(now).setDate(expectedEnd.getDate() - 3)),
                "1w": new Date(new Date(now).setDate(expectedEnd.getDate() - 7)),
                "1m": new Date(new Date(now).setMonth(expectedEnd.getMonth() - 1)),
                "3m": new Date(new Date(now).setMonth(expectedEnd.getMonth() - 3)),
                "1y": new Date(new Date(now).setFullYear(expectedEnd.getFullYear() - 1)),
                "3y": new Date(new Date(now).setFullYear(expectedEnd.getFullYear() - 3)),
                "5y": new Date(new Date(now).setFullYear(expectedEnd.getFullYear() - 5)),
            };

            expect(component.ranges.length).toEqual(Object.keys(expectedStarts).length);

            for (let i = 0; i < component.ranges.length; i++) {
                let range = component.ranges[i];
                (userService.getProgress as jasmine.Spy).calls.reset();

                component.selectedRange = range;

                expect(userService.getProgress).toHaveBeenCalledWith("someUserName", expectedStarts[range], expectedEnd);
            }
        });
    });

    describe("Charts", () => {
        it("should display", async () => {
            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();

            let warning = fixture.debugElement.query(By.css(".alert-warning"));
            expect(warning).toBeNull();

            let charts = fixture.debugElement.queryAll(By.directive(MockBaseChartDirective));
            expect(charts.length).toEqual(16);

            for (let i = 0; i < charts.length; i++) {
                let chart = charts[i];

                expect(chart).not.toBeNull();
                expect(chart.properties.height).toEqual(235);
    
                let chartDirective = chart.injector.get(MockBaseChartDirective) as MockBaseChartDirective;
                expect(chartDirective).not.toBeNull();
                expect(chartDirective.type).toEqual("line");

                expect(chartDirective.options).toBeTruthy();
                expect(chartDirective.options.plugins.title.text).toEqual(expectedChartOrder[i].title);

                expect(chartDirective.datasets).toBeTruthy();
                expect(chartDirective.datasets.length).toEqual(1);

                let data = chartDirective.datasets[0].data as ScatterDataPoint[];
                let expectedData = expectedChartOrder[i].data;
                let isLogarithmic = expectedChartOrder[i].isLogarithmic;
                let isPercent = expectedChartOrder[i].isPercent;
                let dataKeys = Object.keys(expectedData);
                expect(data.length).toEqual(dataKeys.length);
                for (let j = 0; j < data.length; j++) {
                    let expectedDate = new Date(dataKeys[j]);
                    expect(data[j].x).toEqual(expectedDate.getTime());
                    expect((chartDirective.options.plugins.tooltip.callbacks.title as Function)([{ parsed: { x: dataKeys[j] } }])).toEqual(expectedDate.toLocaleString());

                    // When logarithmic, the value we plot is actually the log of the value to fake log scale
                    let rawExpectedValue = expectedData[dataKeys[j]];
                    let expectedValue = isLogarithmic
                        ? new Decimal(rawExpectedValue).log().toNumber()
                        : Number(rawExpectedValue);
                    expect(data[j].y).toEqual(expectedValue);

                    let expectedLabelNum = isLogarithmic
                        ? Decimal.pow(10, expectedValue)
                        : Number(expectedValue);
                    let expectedLabel = isPercent
                        ? percentPipe.transform(expectedValue / 100, "1.1-3")
                        : ExponentialPipe.formatNumber(expectedLabelNum, settings);
                    expect((chartDirective.options.plugins.tooltip.callbacks.label as Function)({ parsed: { y: expectedValue } })).toEqual(expectedLabel);
                    expect((<any>chartDirective.options.scales.yAxis.ticks).callback(expectedValue, null, null)).toEqual(expectedLabel);
                }

                // Linear even when logarithmic since we're manually managing log scale
                expect(chartDirective.options.scales.yAxis.type).toEqual("linear");
            }
        });

        it("should show an error when userService.getProgress fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();

            let warning = fixture.debugElement.query(By.css(".alert-warning"));
            expect(warning).toBeNull();

            let charts = fixture.debugElement.queryAll(By.css("canvas"));
            expect(charts.length).toEqual(0);
        });

        it("should show a warning when there is no data", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({} as any));

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();

            let warning = fixture.debugElement.query(By.css(".alert-warning"));
            expect(warning).not.toBeNull();

            let charts = fixture.debugElement.queryAll(By.css("canvas"));
            expect(charts.length).toEqual(0);
        });
    });

    function createData(index: number, useLargeNumbers: boolean = true): { [date: string]: string } {
        // Mix it up to let some charts be logarithmic
        let prefix = !useLargeNumbers || index % 2
            ? index.toString()
            : "1e10" + index;
        return {
            "2017-01-01T00:00:00Z": prefix + "1",
            "2017-01-02T00:00:00Z": prefix + "2",
            "2017-01-03T00:00:00Z": prefix + "3",
            "2017-01-04T00:00:00Z": prefix + "4",
            "2017-01-05T00:00:00Z": prefix + "5",
        };
    }
});
