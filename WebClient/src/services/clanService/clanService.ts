import { Injectable } from "@angular/core";
import { Http, RequestOptions, URLSearchParams } from "@angular/http";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { IPaginationMetadata } from "../../models";

export interface IGuildMember {
    highestZone: number;

    nickname: string;

    uid: string;
}

export interface IClanData {
    clanName: string;

    currentRaidLevel: number;

    guildMembers: IGuildMember[];

    messages: IMessage[];
}

export interface IMessage {
    date: string;

    username: string;

    content: string;
}

export interface ILeaderboardSummaryListResponse {
    pagination: IPaginationMetadata;

    leaderboardClans: ILeaderboardClan[];
}

export interface ILeaderboardClan {
    name: string;

    currentRaidLevel: number;

    memberCount: number;

    rank: number;

    isUserClan: boolean;
}

export interface ISendMessageResponse {
    success: boolean;
    reason?: string;
}

@Injectable()
export class ClanService {
    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
        private appInsights: AppInsightsService,
    ) { }

    // TODO: this is really the user's clan.
    public getClan(): Promise<IClanData> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get("/api/clans", options)
                    .toPromise();
            })
            .then(response => {
                if (response.status === 204) {
                    // This means the user is not part of a clan.
                    return null;
                }

                return response.json() as IClanData;
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("ClanService.getClan.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    // TODO this should be combined with the call above.
    public getUserClan(): Promise<ILeaderboardClan> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get("/api/clans/userClan", options)
                    .toPromise();
            })
            .then(response => {
                if (response.status === 204) {
                    // This means the user is not part of a clan.
                    return null;
                }

                return response.json() as ILeaderboardClan;
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("ClanService.getUserClan.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public getLeaderboard(page: number, count: number): Promise<ILeaderboardSummaryListResponse> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get("/api/clans/leaderboard?page=" + page + "&count=" + count, options)
                    .toPromise();
            })
            .then(response => {
                return response.json() as ILeaderboardSummaryListResponse;
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("ClanService.getLeaderboard.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public sendMessage(message: string, clanName: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers.append("Content-Type", "application/x-www-form-urlencoded");
                let options = new RequestOptions({ headers });
                let params = new URLSearchParams();
                params.append("message", message);
                params.append("clanName", clanName);
                return this.http
                    .post("/api/clans/messages", params.toString(), options)
                    .toPromise();
            })
            .then(response => {
                let sendMessageResponse = response.json() as ISendMessageResponse;
                if (!sendMessageResponse.success) {
                    return Promise.reject(sendMessageResponse.reason);
                }

                return Promise.resolve();
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("ClanService.getLeaderboard.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }
}
