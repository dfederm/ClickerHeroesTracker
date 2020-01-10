import { Component, OnInit, Inject, LOCALE_ID } from "@angular/core";
import { ActivatedRoute, Params } from "@angular/router";
import { UserService, IProgressData } from "../../services/userService/userService";

import { Decimal } from "decimal.js";
import { ChartDataSets, ChartOptions, ChartTooltipItem } from "chart.js";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { PercentPipe } from "@angular/common";

interface IChartViewModel {
    isProminent: boolean;
    datasets: ChartDataSets[];
    options: ChartOptions;
    colors: {
        backgroundColor: string,
        borderColor: string,
        pointBackgroundColor: string,
        pointBorderColor: string,
        pointHoverBackgroundColor: string,
        pointHoverBorderColor: string,
    }[];
}

@Component({
    selector: "userProgress",
    templateUrl: "./userProgress.html",
})
export class UserProgressComponent implements OnInit {
    private static readonly timeRanges = [
        "1d",
        "3d",
        "1w",
        "1m",
        "3m",
        "1y",
    ];
    private static readonly ascensionRanges = [
        "10",
        "25",
        "50",
        "100",
    ];

    public isError: boolean;
    public isLoading: boolean;
    public userName: string;
    public _selectedRange: string;
    public ranges: string[];
    public charts: IChartViewModel[];

    private settings: IUserSettings;
    private readonly percentPipe: PercentPipe;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly userService: UserService,
        private readonly settingsService: SettingsService,
        @Inject(LOCALE_ID) locale: string,
    ) {
        this.percentPipe = new PercentPipe(locale);
    }

    public get selectedRange(): string {
        return this._selectedRange;
    }
    public set selectedRange(value: string) {
        if (this._selectedRange !== value) {
            this._selectedRange = value;
            this.fetchData();
        }
    }

    public ngOnInit(): void {
        this.route.params.subscribe(
            (params: Params) => {
                this.userName = params.userName;
                this.fetchData();
            },
            () => this.isError = true);

        this.settingsService
            .settings()
            .subscribe(settings => {
                this.settings = settings;
                switch (this.settings.graphSpacingType) {
                    case "ascension": {
                        this.ranges = UserProgressComponent.ascensionRanges;
                        this._selectedRange = "10";
                        break;
                    }
                    case "time":
                    default: {
                        this.ranges = UserProgressComponent.timeRanges;
                        this._selectedRange = "1w";
                    }
                }
                this.fetchData();
            });
    }

    private fetchData(): void {
        if (!this.userName || !this.settings) {
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
            case "1m":
                startOrPage.setMonth(startOrPage.getMonth() - 1);
                break;
            case "3m":
                startOrPage.setMonth(startOrPage.getMonth() - 3);
                break;
            case "1y":
                startOrPage.setFullYear(startOrPage.getFullYear() - 1);
                break;
            case "1w":
                startOrPage.setDate(startOrPage.getDate() - 7);
                break;
            default:
                // Using Ascension range
                startOrPage = 1;
                endOrCount = Number(this.selectedRange);
        }

        this.isLoading = true;
        this.userService.getProgress(this.userName, startOrPage, endOrCount)
            .then(progress => this.handleData(progress))
            .catch(() => this.isError = true);
    }

    private handleData(progress: IProgressData): void {
        this.isLoading = false;
        if (!progress) {
            this.charts = [];
            return;
        }

        let charts: IChartViewModel[] = [];

        let formatExponential = this.formatExponential.bind(this);
        let formatPercent = this.formatPercent.bind(this);

        charts.push(this.createChart("Souls Spent", true, progress.soulsSpentData, formatExponential));
        charts.push(this.createChart("Titan Damage", true, progress.titanDamageData, formatExponential));
        charts.push(this.createChart("Hero Souls Sacrificed", true, progress.heroSoulsSacrificedData, formatExponential));
        charts.push(this.createChart("Total Ancient Souls", true, progress.totalAncientSoulsData, formatExponential));
        charts.push(this.createChart("Transcendent Power", true, progress.transcendentPowerData, formatPercent));
        charts.push(this.createChart("Rubies", true, progress.rubiesData, formatExponential));
        charts.push(this.createChart("Highest Zone This Transcension", true, progress.highestZoneThisTranscensionData, formatExponential));
        charts.push(this.createChart("Highest Zone Lifetime", true, progress.highestZoneLifetimeData, formatExponential));
        charts.push(this.createChart("Ascensions This Transcension", true, progress.ascensionsThisTranscensionData, formatExponential));
        charts.push(this.createChart("Ascensions Lifetime", true, progress.ascensionsLifetimeData, formatExponential));

        for (let outsider in progress.outsiderLevelData) {
            charts.push(this.createChart(this.toTitleCase(outsider), false, progress.outsiderLevelData[outsider], formatExponential));
        }

        for (let ancient in progress.ancientLevelData) {
            charts.push(this.createChart(this.toTitleCase(ancient), false, progress.ancientLevelData[ancient], formatExponential));
        }

        // Only valid charts
        charts = charts.filter(chart => chart != null);

        this.charts = charts;
    }

    private createChart(
        title: string,
        isProminent: boolean,
        data: { [time: string]: string },
        formatValue: (value: string | number, isLogarithmic: boolean) => string,
    ): IChartViewModel {
        if (!data) {
            return null;
        }

        let min = new Decimal(Infinity);
        let max = new Decimal(0);
        let requiresDecimal = false;
        let isTime = this.settings.graphSpacingType === "time";

        let decimalData: { x: number, y: Decimal }[] = [];
        for (let i in data) {
            let x = isTime ? Date.parse(i) : Number(i);
            let value = new Decimal(data[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            requiresDecimal = requiresDecimal || !isFinite(value.toNumber());

            decimalData.push({ x, y: value });
        }

        let isLogarithmic = (this.settings.useLogarithmicGraphScale && max.minus(min).greaterThan(this.settings.logarithmicGraphScaleThreshold)) || requiresDecimal;

        let seriesData: { x: number, y: number }[] = [];
        for (let i = 0; i < decimalData.length; i++) {
            let x = decimalData[i].x;
            let value = decimalData[i].y;

            seriesData.push({
                x,
                y: isLogarithmic
                    ? value.log().toNumber()
                    : value.toNumber(),
            });
        }

        if (seriesData.length === 0) {
            return null;
        }

        return {
            isProminent,
            datasets: [{
                data: seriesData,
            }],
            colors: [{
                backgroundColor: "rgba(151,187,205,0.4)",
                borderColor: "rgba(151,187,205,1)",
                pointBackgroundColor: "rgba(151,187,205,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "rgba(151,187,205,0.8)",
                pointHoverBorderColor: "#fff",
            }],
            options: {
                title: {
                    display: true,
                    fontSize: 18,
                    text: title,
                },
                legend: {
                    display: false,
                },
                tooltips: {
                    callbacks: {
                        title: (tooltipItems: ChartTooltipItem[]) => {
                            return isTime
                                ? new Date(tooltipItems[0].xLabel).toLocaleString()
                                : tooltipItems[0].xLabel.toString();
                        },
                        label: (tooltipItem: ChartTooltipItem) => {
                            return formatValue(tooltipItem.yLabel, isLogarithmic);
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
                    xAxes: [
                        {
                            type: isTime ? "time" : "linear",
                        },
                    ],
                    yAxes: [
                        {
                            type: "linear",
                            ticks: {
                                callback: (value: number): string => {
                                    return formatValue(value, isLogarithmic);
                                },
                            },
                        },
                    ],
                },
            },
        };
    }

    private formatExponential(value: string | number, isLogarithmic: boolean): string {
        let num = isLogarithmic
            ? Decimal.pow(10, value)
            : Number(value);
        return ExponentialPipe.formatNumber(num, this.settings);
    }

    private formatPercent(value: string | number, isLogarithmic: boolean): string {
        let num = isLogarithmic
            ? Decimal.pow(10, value).toNumber()
            : Number(value);
        return this.percentPipe.transform(num / 100, "1.1-3");
    }

    private toTitleCase(srt: string): string {
        return srt[0].toUpperCase() + srt.substring(1);
    }
}
