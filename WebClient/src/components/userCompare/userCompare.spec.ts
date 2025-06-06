import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { ActivatedRoute, NavigationExtras, Params, provideRouter, Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import Decimal from "decimal.js";

import { UserCompareComponent } from "./userCompare";
import { UserService, IProgressData } from "../../services/userService/userService";
import { ChartDataset, ChartOptions, ScatterDataPoint } from "chart.js";
import { SettingsService } from "../../services/settingsService/settingsService";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { Component, Directive, Input } from "@angular/core";
import { NgxSpinnerModule } from "ngx-spinner";
import { BaseChartDirective } from "ng2-charts";

describe("UserCompareComponent", () => {
    let component: UserCompareComponent;
    let fixture: ComponentFixture<UserCompareComponent>;
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

    let userProgress: IProgressData = {
        soulsSpentData: createData(100),
        titanDamageData: createData(101),
        heroSoulsSacrificedData: createData(102),
        totalAncientSoulsData: createData(103),
        transcendentPowerData: createData(104),
        rubiesData: createData(105),
        highestZoneThisTranscensionData: createData(106),
        highestZoneLifetimeData: createData(107),
        ascensionsThisTranscensionData: createData(108),
        ascensionsLifetimeData: createData(109),
        outsiderLevelData: {
            outsider0: createData(110),
            outsider1: createData(111),
            outsider2: createData(112),
        },
        ancientLevelData: {
            ancient0: createData(113),
            ancient1: createData(114),
            ancient2: createData(115),
        },
    };

    let compareProgress: IProgressData = {
        soulsSpentData: createData(200),
        titanDamageData: createData(201),
        heroSoulsSacrificedData: createData(202),
        totalAncientSoulsData: createData(203),
        transcendentPowerData: createData(204),
        rubiesData: createData(205),
        highestZoneThisTranscensionData: createData(206),
        highestZoneLifetimeData: createData(207),
        ascensionsThisTranscensionData: createData(208),
        ascensionsLifetimeData: createData(209),
        outsiderLevelData: {
            outsider0: createData(210),
            outsider1: createData(211),
            outsider2: createData(212),
        },
        ancientLevelData: {
            ancient0: createData(213),
            ancient1: createData(214),
            ancient2: createData(215),
        },
    };

    let expectedChartOrder = [
        { title: "Souls Spent", isLogarithmic: true, data1: userProgress.soulsSpentData, data2: compareProgress.soulsSpentData },
        { title: "Titan Damage", isLogarithmic: false, data1: userProgress.titanDamageData, data2: compareProgress.titanDamageData },
        { title: "Hero Souls Sacrificed", isLogarithmic: true, data1: userProgress.heroSoulsSacrificedData, data2: compareProgress.heroSoulsSacrificedData },
        { title: "Total Ancient Souls", isLogarithmic: false, data1: userProgress.totalAncientSoulsData, data2: compareProgress.totalAncientSoulsData },
        { title: "Transcendent Power", isLogarithmic: true, data1: userProgress.transcendentPowerData, data2: compareProgress.transcendentPowerData },
        { title: "Rubies", isLogarithmic: false, data1: userProgress.rubiesData, data2: compareProgress.rubiesData },
        { title: "Highest Zone This Transcension", isLogarithmic: true, data1: userProgress.highestZoneThisTranscensionData, data2: compareProgress.highestZoneThisTranscensionData },
        { title: "Highest Zone Lifetime", isLogarithmic: false, data1: userProgress.highestZoneLifetimeData, data2: compareProgress.highestZoneLifetimeData },
        { title: "Ascensions This Transcension", isLogarithmic: true, data1: userProgress.ascensionsThisTranscensionData, data2: compareProgress.ascensionsThisTranscensionData },
        { title: "Ascensions Lifetime", isLogarithmic: false, data1: userProgress.ascensionsLifetimeData, data2: compareProgress.ascensionsLifetimeData },
        { title: "Outsider0", isLogarithmic: true, data1: userProgress.outsiderLevelData.outsider0, data2: compareProgress.outsiderLevelData.outsider0 },
        { title: "Outsider1", isLogarithmic: false, data1: userProgress.outsiderLevelData.outsider1, data2: compareProgress.outsiderLevelData.outsider1 },
        { title: "Outsider2", isLogarithmic: true, data1: userProgress.outsiderLevelData.outsider2, data2: compareProgress.outsiderLevelData.outsider2 },
        { title: "Ancient0", isLogarithmic: false, data1: userProgress.ancientLevelData.ancient0, data2: compareProgress.ancientLevelData.ancient0 },
        { title: "Ancient1", isLogarithmic: true, data1: userProgress.ancientLevelData.ancient1, data2: compareProgress.ancientLevelData.ancient1 },
        { title: "Ancient2", isLogarithmic: false, data1: userProgress.ancientLevelData.ancient2, data2: compareProgress.ancientLevelData.ancient2 },
    ];

    const settings = SettingsService.defaultSettings;

    let settingsSubject = new BehaviorSubject(settings);

    beforeEach(async () => {
        let userService = {
            getProgress(userName: string): Promise<IProgressData> {
                if (userName === "someUserName") {
                    return Promise.resolve(userProgress);
                }

                if (userName === "someOtherUserName") {
                    return Promise.resolve(compareProgress);
                }

                return Promise.reject("No user: " + userName);
            },
        };

        let settingsService = { settings: () => settingsSubject };

        routeParams = new BehaviorSubject({ userName: "someUserName", compareUserName: "someOtherUserName" });
        queryParams = new BehaviorSubject({});
        let route = { params: routeParams, queryParams: queryParams };

        await TestBed.configureTestingModule(
            {
                imports: [UserCompareComponent],
                providers: [
                    provideRouter([]),
                    { provide: ActivatedRoute, useValue: route },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(UserCompareComponent, {
            remove: { imports: [ NgxSpinnerModule, BaseChartDirective ]},
            add: { imports: [ MockNgxSpinnerComponent, MockBaseChartDirective ] },
        });

        fixture = TestBed.createComponent(UserCompareComponent);
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

            let disabledranges = ranges.filter(range => range.classes.disabled);
            expect(disabledranges.length).toEqual(1);
            expect(disabledranges[0].nativeElement.textContent.trim()).toEqual(component.selectedRange);
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

            let disabledranges = ranges.filter(range => range.classes.disabled);
            expect(disabledranges.length).toEqual(1);
            expect(disabledranges[0].nativeElement.textContent.trim()).toEqual(component.selectedRange);
        });

        it("should request the proper date range", () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.callThrough();

            fixture.detectChanges();

            let now = Date.now();
            spyOn(Date, "now").and.returnValue(now);

            let expectedEnd = new Date(now);
            let expectedStarts: { [range: string]: Date } = {
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

                let isLogarithmic = expectedChartOrder[i].isLogarithmic;
                expect(chartDirective.datasets).toBeTruthy();
                expect(chartDirective.datasets.length).toEqual(2);

                let data1 = chartDirective.datasets[0].data as ScatterDataPoint[];
                let expectedData1 = expectedChartOrder[i].data1;
                let dataKeys1 = Object.keys(expectedData1);
                expect(data1.length).toEqual(dataKeys1.length);
                for (let j = 0; j < data1.length; j++) {
                    let expectedDate = new Date(dataKeys1[j]);
                    expect(data1[j].x).toEqual(expectedDate.getTime());
                    expect((chartDirective.options.plugins.tooltip.callbacks.title as Function)([{ parsed: { x: dataKeys1[j] } }])).toEqual(expectedDate.toLocaleString());

                    // When logarithmic, the value we plot is actually the log of the value to fake log scale
                    let rawExpectedValue = expectedData1[dataKeys1[j]];
                    let expectedValue = isLogarithmic
                        ? new Decimal(rawExpectedValue).log().toNumber()
                        : Number(rawExpectedValue);
                    expect(data1[j].y).toEqual(expectedValue);

                    let expectedLabelNum = isLogarithmic
                        ? Decimal.pow(10, expectedValue)
                        : Number(expectedValue);
                    let expectedLabel = ExponentialPipe.formatNumber(expectedLabelNum, settings);
                    expect((chartDirective.options.plugins.tooltip.callbacks.label as Function)({ parsed: { y: expectedValue }, datasetIndex: 0 })).toEqual("someUserName: " + expectedLabel);
                    expect((<any>chartDirective.options.scales.yAxis.ticks).callback(expectedValue, null, null)).toEqual(expectedLabel);
                }

                let data2 = chartDirective.datasets[1].data as ScatterDataPoint[];
                let expectedData2 = expectedChartOrder[i].data2;
                let dataKeys2 = Object.keys(expectedData2);
                expect(data2.length).toEqual(dataKeys2.length);
                for (let j = 0; j < data2.length; j++) {
                    let expectedDate = new Date(dataKeys2[j]);
                    expect(data2[j].x).toEqual(expectedDate.getTime());
                    expect((chartDirective.options.plugins.tooltip.callbacks.title as Function)([{ parsed: { x: dataKeys2[j] } }])).toEqual(expectedDate.toLocaleString());

                    // When logarithmic, the value we plot is actually the log of the value to fake log scale
                    let rawExpectedValue = expectedData2[dataKeys2[j]];
                    let expectedValue = isLogarithmic
                        ? new Decimal(rawExpectedValue).log().toNumber()
                        : Number(rawExpectedValue);
                    expect(data2[j].y).toEqual(expectedValue);

                    let expectedLabelNum = isLogarithmic
                        ? Decimal.pow(10, expectedValue)
                        : Number(expectedValue);
                    let expectedLabel = ExponentialPipe.formatNumber(expectedLabelNum, settings);
                    expect((chartDirective.options.plugins.tooltip.callbacks.label as Function)({ parsed: { y: expectedValue }, datasetIndex: 1 })).toEqual("someOtherUserName: " + expectedLabel);
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

    function createData(index: number): { [date: string]: string } {
        // Mix it up to let some charts be logarithmic
        let prefix = index % 2
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
