import { Component, OnInit } from "@angular/core";

import { UserService, IProgressData, IFollowsData } from "../../services/userService/userService";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";

import { Decimal } from "decimal.js";
import { ChartDataSets, ChartOptions, ChartTooltipItem } from "chart.js";
import { ActivatedRoute } from "@angular/router";

interface IProgressViewModel {
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
  selector: "user",
  templateUrl: "./user.html",
})
export class UserComponent implements OnInit {
  public userName: string;

  public isProgressError: boolean;
  public isProgressLoading: boolean;
  public progress: IProgressViewModel;

  public isFollowsError: boolean;
  public isFollowsLoading: boolean;
  public follows: string[];

  public isActionsError: boolean;
  public isActionsLoading: boolean;
  public currentUserName: string;
  public isCurrentUserFollowing: boolean;
  private currentUserFollows: string[];

  private settings: IUserSettings;

  constructor(
    private userService: UserService,
    private settingsService: SettingsService,
    private route: ActivatedRoute,
    private authenticationService: AuthenticationService,
  ) { }

  public ngOnInit(): void {
    this.route
      .params
      .subscribe(params => this.handleUser(params.userName));

    this.settingsService
      .settings()
      .subscribe(settings => this.handleSettings(settings));

    this.authenticationService
      .userInfo()
      .subscribe(userInfo => this.handleCurrentUserInfo(userInfo));
  }

  public follow(): void {
    this.isActionsLoading = true;
    this.userService.addFollow(this.currentUserName, this.userName)
      .then(() => {
        this.isActionsLoading = false;
        this.isCurrentUserFollowing = true;
      })
      .catch(() => {
        this.isActionsError = true;
      });
  }

  public unfollow(): void {
    this.isActionsLoading = true;
    this.userService.removeFollow(this.currentUserName, this.userName)
      .then(() => {
        this.isActionsLoading = false;
        this.isCurrentUserFollowing = false;
      })
      .catch(() => {
        this.isActionsError = true;
      });
  }

  private handleUser(userName: string): void {
    this.isProgressError = false;
    this.isFollowsError = false;
    this.progress = null;
    this.follows = null;

    this.userName = userName;
    this.refresh();
    this.refreshActions();
  }

  private handleSettings(settings: IUserSettings): void {
    this.settings = settings;
    this.refresh();
  }

  private handleCurrentUserInfo(currentUserInfo: IUserInfo): void {
    this.isActionsError = false;
    if (currentUserInfo.isLoggedIn) {
      this.currentUserName = currentUserInfo.username;
      this.isActionsLoading = true;
      this.userService.getFollows(this.currentUserName)
        .then(data => {
          this.isActionsLoading = false;
          this.currentUserFollows = data.follows;
          this.refreshActions();
        })
        .catch(() => this.isActionsError = true);
    } else {
      this.currentUserName = null;
      this.currentUserFollows = null;
      this.refreshActions();
    }
  }

  private refresh(): void {
    // Ensure we have some value for each
    if (!this.userName || !this.settings) {
      return;
    }

    // Show the last week's worth
    let now = Date.now();
    let end = new Date(now);
    let start = new Date(now);
    start.setDate(start.getDate() - 7);

    this.isProgressLoading = true;
    this.userService.getProgress(this.userName, start, end)
      .then(progress => this.handleProgressData(progress))
      .catch(() => this.isProgressError = true);

    this.isFollowsLoading = true;
    this.userService.getFollows(this.userName)
      .then(follows => this.handleFollowsData(follows))
      .catch(() => this.isFollowsError = true);
  }

  private refreshActions(): void {
    this.isCurrentUserFollowing = false;
    if (this.currentUserFollows) {
      for (let i = 0; i < this.currentUserFollows.length; i++) {
        if (this.currentUserFollows[i] === this.userName) {
          this.isCurrentUserFollowing = true;
          break;
        }
      }
    }
  }

  private handleProgressData(progress: IProgressData): void {
    this.isProgressLoading = false;
    if (progress && Object.keys(progress.soulsSpentData).length === 0) {
      // No data
      return;
    }

    let data = progress.soulsSpentData;

    let min = new Decimal(Infinity);
    let max = new Decimal(0);
    let requiresDecimal = false;

    let decimalData: { x: number, y: Decimal }[] = [];
    for (let i in data) {
      let time = Date.parse(i);
      let value = new Decimal(data[i]);

      if (min.greaterThan(value)) {
        min = value;
      }

      if (max.lessThan(value)) {
        max = value;
      }

      requiresDecimal = requiresDecimal || !isFinite(value.toNumber());

      decimalData.push({ x: time, y: value });
    }

    let isLogarithmic = (this.settings.useLogarithmicGraphScale && max.minus(min).greaterThan(this.settings.logarithmicGraphScaleThreshold)) || requiresDecimal;

    let seriesData: { x: number, y: number }[] = [];
    for (let i = 0; i < decimalData.length; i++) {
      let time = decimalData[i].x;
      let value = decimalData[i].y;

      seriesData.push({
        x: time,
        y: isLogarithmic
          ? value.log().toNumber()
          : value.toNumber(),
      });
    }

    this.progress = {
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
          text: "Souls Spent",
        },
        legend: {
          display: false,
        },
        tooltips: {
          callbacks: {
            title: (tooltipItems: ChartTooltipItem[]) => {
              return new Date(tooltipItems[0].xLabel).toLocaleString();
            },
            label: (tooltipItem: ChartTooltipItem) => {
              return isLogarithmic
                ? Decimal.pow(10, tooltipItem.yLabel).toExponential(3)
                : Number(tooltipItem.yLabel).toExponential(3);
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
              type: "time",
            },
          ],
          yAxes: [
            {
              type: "linear",
              ticks: {
                callback: (value: number): string => {
                  return isLogarithmic
                    ? Decimal.pow(10, value).toExponential(3)
                    : value.toExponential(3);
                },
              },
            },
          ],
        },
      },
    };
  }

  private handleFollowsData(data: IFollowsData): void {
    this.isFollowsLoading = false;
    if (!data || !data.follows || !data.follows.length) {
      // No data
      return;
    }

    this.follows = data.follows;
  }
}
