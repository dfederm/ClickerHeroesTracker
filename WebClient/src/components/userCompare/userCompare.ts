import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Params, Router, RouterLink } from "@angular/router";
import { UserService, IProgressData } from "../../services/userService/userService";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";

import { Decimal } from "decimal.js";
import { ChartDataset, ChartOptions, TooltipItem } from "chart.js";
import 'chartjs-adapter-date-fns';
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { BaseChartDirective } from "ng2-charts";
import { NgClass } from "@angular/common";

interface IChartViewModel {
    isProminent: boolean;
    datasets: ChartDataset<"line">[];
    options: ChartOptions<"line">;
}

@Component({
    selector: "userCompare",
    templateUrl: "./userCompare.html",
    imports: [
        BaseChartDirective,
        NgClass,
        NgxSpinnerModule,
        RouterLink,
    ]
})
export class UserCompareComponent implements OnInit {
    private static readonly timeRanges = [
        "1d",
        "3d",
        "1w",
        "1m",
        "3m",
        "1y",
        "3y",
        "5y",
    ];
    private static readonly ascensionRanges = [
        "10",
        "25",
        "50",
        "100",
    ];

    public isError: boolean;
    public userName: string;
    public compareUserName: string;
    public _selectedRange: string;
    public ranges: string[];
    public charts: IChartViewModel[];

    private settings: IUserSettings;
    private defaultRange: string;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly userService: UserService,
        private readonly settingsService: SettingsService,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public get selectedRange(): string {
        return this._selectedRange ?? this.defaultRange;
    }
    public set selectedRange(value: string) {
        if (this._selectedRange !== value) {
            this._selectedRange = value;

            this.router.navigate(
                [],
                {
                    relativeTo: this.route,
                    queryParams: { range: value },
                    queryParamsHandling: "merge"
                });
        }
    }

    public ngOnInit(): void {
        this.route.params.subscribe({
            next: (params: Params) => {
                this.userName = params.userName;
                this.compareUserName = params.compareUserName;
                this.fetchData();
            },
            error: () => this.isError = true
        });

        this.route.queryParams.subscribe({
            next: (params: Params) => {
                this._selectedRange = params.range;
                this.fetchData();
            },
            error: () => this.isError = true
        });

        this.settingsService
            .settings()
            .subscribe(settings => {
                this.settings = settings;
                switch (this.settings.graphSpacingType) {
                    case "ascension": {
                        this.ranges = UserCompareComponent.ascensionRanges;
                        this.defaultRange = "10";
                        break;
                    }
                    case "time":
                    default: {
                        this.ranges = UserCompareComponent.timeRanges;
                        this.defaultRange = "1w";
                    }
                }
                this.validateRange();
                this.fetchData();
            });
    }

    private fetchData(): void {
        if (!this.userName || !this.compareUserName || !this.settings) {
            return;
        }

        let now = Date.now();
        let startOrPage: number | Date = new Date(now);
        let endOrCount: number | Date = new Date(now);

        switch (this.selectedRange) {
            case "1d":
                startOrPage.setDate(startOrPage.getDate() - 1);
                break;
            case "3d":
                startOrPage.setDate(startOrPage.getDate() - 3);
                break;
            case "1w":
                startOrPage.setDate(startOrPage.getDate() - 7);
                break;
            case "1m":
                startOrPage.setMonth(startOrPage.getMonth() - 1);
                break;
            case "3m":
                startOrPage.setMonth(startOrPage.getMonth() - 3);
                break;
            case "1y":
                startOrPage.setFullYear(startOrPage.getFullYear() - 1);
                break;
            case "3y":
                startOrPage.setFullYear(startOrPage.getFullYear() - 3);
                break;
            case "5y":
                startOrPage.setFullYear(startOrPage.getFullYear() - 5);
                break;
            default:
                // Using Ascension range
                startOrPage = 1;
                endOrCount = Number(this.selectedRange);
        }

        this.spinnerService.show("userCompare");
        Promise.all([
            this.userService.getProgress(this.userName, startOrPage, endOrCount),
            this.userService.getProgress(this.compareUserName, startOrPage, endOrCount),
        ])
            .then(data => this.handleData(data[0], data[1]))
            .catch(() => this.isError = true)
            .finally(() => {
                this.spinnerService.hide("userCompare");
            });
    }

    private handleData(progress1: IProgressData, progress2: IProgressData): void {
        if (!progress1 || !progress2) {
            this.charts = [];
            return;
        }

        let charts: IChartViewModel[] = [];

        charts.push(this.createChart("Souls Spent", true, progress1.soulsSpentData, progress2.soulsSpentData));
        charts.push(this.createChart("Titan Damage", true, progress1.titanDamageData, progress2.titanDamageData));
        charts.push(this.createChart("Hero Souls Sacrificed", true, progress1.heroSoulsSacrificedData, progress2.heroSoulsSacrificedData));
        charts.push(this.createChart("Total Ancient Souls", true, progress1.totalAncientSoulsData, progress2.totalAncientSoulsData));
        charts.push(this.createChart("Transcendent Power", true, progress1.transcendentPowerData, progress2.transcendentPowerData));
        charts.push(this.createChart("Rubies", true, progress1.rubiesData, progress2.rubiesData));
        charts.push(this.createChart("Highest Zone This Transcension", true, progress1.highestZoneThisTranscensionData, progress2.highestZoneThisTranscensionData));
        charts.push(this.createChart("Highest Zone Lifetime", true, progress1.highestZoneLifetimeData, progress2.highestZoneLifetimeData));
        charts.push(this.createChart("Ascensions This Transcension", true, progress1.ascensionsThisTranscensionData, progress2.ascensionsThisTranscensionData));
        charts.push(this.createChart("Ascensions Lifetime", true, progress1.ascensionsLifetimeData, progress2.ascensionsLifetimeData));

        for (let outsider in progress1.outsiderLevelData) {
            charts.push(this.createChart(this.toTitleCase(outsider), false, progress1.outsiderLevelData[outsider], progress2.outsiderLevelData[outsider]));
        }

        for (let ancient in progress1.ancientLevelData) {
            charts.push(this.createChart(this.toTitleCase(ancient), false, progress1.ancientLevelData[ancient], progress2.ancientLevelData[ancient]));
        }

        // Only valid charts
        charts = charts.filter(chart => chart != null);

        this.charts = charts;
    }

    // eslint-disable-next-line complexity
    private createChart(
        title: string,
        isProminent: boolean,
        data1: { [time: string]: string },
        data2: { [time: string]: string },
    ): IChartViewModel {
        if (!data1 && !data2) {
            return null;
        }

        let min = new Decimal(Infinity);
        let max = new Decimal(0);
        let requiresDecimal = false;
        let isTime = this.settings.graphSpacingType === "time";

        let decimalData1: { x: number, y: Decimal }[] = [];
        let index = 0; // When not using time, normalize the ascension numbers
        for (let i in data1) {
            let x = isTime ? Date.parse(i) : Number(index++);
            let value = new Decimal(data1[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            requiresDecimal = requiresDecimal || !isFinite(value.toNumber());

            decimalData1.push({ x, y: value });
        }

        let decimalData2: { x: number, y: Decimal }[] = [];
        index = 0;
        for (let i in data2) {
            let x = isTime ? Date.parse(i) : Number(index++);
            let value = new Decimal(data2[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            requiresDecimal = requiresDecimal || !isFinite(value.toNumber());

            decimalData2.push({ x, y: value });
        }

        let isLogarithmic = (this.settings.useLogarithmicGraphScale && max.minus(min).greaterThan(this.settings.logarithmicGraphScaleThreshold)) || requiresDecimal;

        let seriesData1: { x: number, y: number }[] = [];
        for (let i = 0; i < decimalData1.length; i++) {
            let x = decimalData1[i].x;
            let value = decimalData1[i].y;

            seriesData1.push({
                x,
                y: isLogarithmic
                    ? value.log().toNumber()
                    : value.toNumber(),
            });
        }

        let seriesData2: { x: number, y: number }[] = [];
        for (let i = 0; i < decimalData2.length; i++) {
            let time = decimalData2[i].x;
            let value = decimalData2[i].y;

            seriesData2.push({
                x: time,
                y: isLogarithmic
                    ? value.log().toNumber()
                    : value.toNumber(),
            });
        }

        if (seriesData1.length === 0 && seriesData2.length === 0) {
            return null;
        }

        return {
            isProminent,
            datasets: [
                {
                    data: seriesData1,
                    fill: true,
                    backgroundColor: "rgba(151,187,205,0.4)",
                    borderColor: "rgba(151,187,205,1)",
                    pointBackgroundColor: "rgba(151,187,205,1)",
                    pointBorderColor: "#fff",
                    pointHoverBackgroundColor: "rgba(151,187,205,0.8)",
                    pointHoverBorderColor: "#fff",
                },
                {
                    data: seriesData2,
                    fill: true,
                    backgroundColor: "rgba(169,68,66,0.4)",
                    borderColor: "rgba(169,68,66,1)",
                    pointBackgroundColor: "rgba(169,68,66,1)",
                    pointBorderColor: "#fff",
                    pointHoverBackgroundColor: "rgba(169,68,66,0.8)",
                    pointHoverBorderColor: "#fff",
                },
            ],
            options: {
                plugins: {
                    title: {
                        display: true,
                        font: {
                            size: 18,
                        },
                        text: title,
                    },
                    legend: {
                        display: false,
                    },
                    tooltip: {
                        callbacks: {
                            title: (tooltipItems: TooltipItem<"line">[]) => {
                                return isTime
                                    ? new Date(tooltipItems[0].parsed.x).toLocaleString()
                                    : null;
                            },
                            label: (tooltipItem: TooltipItem<"line">) => {
                                let tooltipUser = tooltipItem.datasetIndex === 0
                                    ? this.userName
                                    : this.compareUserName;
                                let tooltipValue = this.formatNumber(tooltipItem.parsed.y, isLogarithmic);
                                return `${tooltipUser}: ${tooltipValue}`;
                            },
                        },
                    },
                },
                elements: {
                    line: {
                        // Disables bezier curves
                        tension: 0,
                    },
                },
                scales: {
                    xAxis: {
                        display: isTime,
                        type: isTime ? "time" : "linear",
                    },
                    yAxis: {
                        type: "linear",
                        ticks: {
                            callback: (value: number): string => {
                                return this.formatNumber(value, isLogarithmic);
                            },
                        },
                    },
                },
            },
        };
    }

    private formatNumber(value: string | number, isLogarithmic: boolean): string {
        let num = isLogarithmic
            ? Decimal.pow(10, value)
            : Number(value);
        return ExponentialPipe.formatNumber(num, this.settings);
    }

    private toTitleCase(srt: string): string {
        return srt[0].toUpperCase() + srt.substring(1);
    }

    private validateRange(): void {
        for (let i = 0; i < this.ranges.length; i++) {
            if (this._selectedRange == this.ranges[i]) {
                return;
            }
        }

        // Reset the value
        this.selectedRange = null;
    }
}
