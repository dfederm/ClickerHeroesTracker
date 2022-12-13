import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { AuthenticationService } from "../authenticationService/authenticationService";
import { IPaginationMetadata } from "../../models";
import { firstValueFrom } from "rxjs";

export type GuildClassType = "rogue" | "mage" | "priest";

export interface IGuildMember {
    highestZone: number;

    nickname: string;

    uid: string;

    userName: string;

    chosenClass?: GuildClassType;

    classLevel?: number;
}

export interface IClanData {
    clanName: string;

    currentRaidLevel: number;

    currentNewRaidLevel?: number;

    guildMembers: IGuildMember[];

    rank: number;

    isBlocked: boolean;
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

    currentNewRaidLevel?: number;

    memberCount: number;

    rank: number;

    isUserClan: boolean;
}

export interface ISendMessageResponse {
    success: boolean;
    reason?: string;
}

@Injectable({
    providedIn: "root",
})
export class ClanService {
    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
    ) { }

    public async getClan(clanName: string): Promise<IClanData> {
        try {
            const headers = await this.authenticationService.getAuthHeaders();
            let response = await firstValueFrom(this.http.get<IClanData>(`/api/clans/${clanName}`, { headers, observe: "response" }));
            if (response.status === 204) {
                // This means the user is not part of a clan.
                return null;
            }

            return response.body;
        } catch (err) {
            this.httpErrorHandlerService.logError("ClanService.getClan.error", err);
            return await Promise.reject(err);
        }
    }

    public async getLeaderboard(filter: string, page: number, count: number): Promise<ILeaderboardSummaryListResponse> {
        try {
            const headers = await this.authenticationService.getAuthHeaders();
            return await firstValueFrom(this.http.get<ILeaderboardSummaryListResponse>("/api/clans?filter=" + filter + "&page=" + page + "&count=" + count, { headers }));
        } catch (err) {
            this.httpErrorHandlerService.logError("ClanService.getLeaderboard.error", err);
            return await Promise.reject(err);
        }
    }

    public async getMessages(): Promise<IMessage[]> {
        try {
            const headers = await this.authenticationService.getAuthHeaders();
            return await firstValueFrom(this.http.get<IMessage[]>("/api/clans/messages", { headers }));
        } catch (err) {
            this.httpErrorHandlerService.logError("ClanService.getMessages.error", err);
            return await Promise.reject(err);
        }
    }

    public async sendMessage(message: string): Promise<void> {
        try {
            let headers = await this.authenticationService.getAuthHeaders();
            headers = headers.set("Content-Type", "application/x-www-form-urlencoded");
            let params = new HttpParams()
                .set("message", message);

            // Angular doesn't encode '+' correctly. See: https://github.com/angular/angular/issues/11058
            let body = params.toString().replace(/\+/gi, "%2B");

            let response = await firstValueFrom(this.http.post<ISendMessageResponse>("/api/clans/messages", body, { headers }));
            if (!response.success) {
                return Promise.reject(response.reason);
            }

            return Promise.resolve();
        } catch (err) {
            this.httpErrorHandlerService.logError("ClanService.sendMessage.error", err);
            return await Promise.reject(err);
        }
    }
}
