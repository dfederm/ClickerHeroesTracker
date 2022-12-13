import { Component, OnInit } from "@angular/core";
import { ClanService, IGuildMember, IMessage } from "../../services/clanService/clanService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";
import { ActivatedRoute } from "@angular/router";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../../services/httpErrorHandlerService/httpErrorHandlerService";
import { IBlockClanRequest } from "../../models";
import { NgxSpinnerService } from "ngx-spinner";

@Component({
    selector: "clan",
    templateUrl: "./clan.html",
    styleUrls: ["./clan.css"],
})
export class ClanComponent implements OnInit {
    public isClanInformationError = false;

    public isClanInformationLoading: boolean;

    public messagesError = "";

    public clanName: string;

    public currentRaidLevel?: number;

    public currentLegacyRaidLevel: number;

    public rank: number;

    public isBlocked: boolean;

    public guildMembers: IGuildMember[];

    public messages: IMessage[];

    public newMessage = "";

    public isUserClan: boolean;

    public isAdmin: boolean;

    public isActionsError: boolean;

    private userClanName: string;

    constructor(
        private readonly clanService: ClanService,
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
        private readonly route: ActivatedRoute,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public ngOnInit(): void {
        this.route
            .params
            .subscribe(params => this.handleClan(params.clanName));

        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.handleUser(userInfo));
    }

    public sendMessage(): void {
        this.messagesError = "";
        this.spinnerService.show("clanMessages");
        this.clanService.sendMessage(this.newMessage)
            .then(() => {
                this.newMessage = "";
                this.getMessages();
            })
            .catch(() => {
                this.messagesError = "There was a problem sending your message. Please try again.";
            });
    }

    public toggleBlock(): void {
        this.spinnerService.show("clanActions");
        this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/json");
                let body: IBlockClanRequest = {
                    clanName: this.clanName,
                    isBlocked: !this.isBlocked,
                };
                return this.http
                    .post("/api/admin/blockclan", body, { headers })
                    .toPromise();
            })
            .then(() => {
                this.refreshClanData();
            })
            .catch((err: HttpErrorResponse) => {
                this.isActionsError = true;
                this.httpErrorHandlerService.logError("ClanComponent.toggleBlock.error", err);
            })
            .finally(() => {
                this.spinnerService.hide("clanActions");
            });
    }

    private handleClan(clanName: string): void {
        this.clanName = clanName;
        this.refreshClanData()
            .finally(() => this.refreshMessageBoard());
    }

    private handleUser(userInfo: IUserInfo): void {
        if (userInfo.isLoggedIn) {
            this.userService.getUser(userInfo.username)
                .then(user => {
                    if (!user || !user.clanName) {
                        return null;
                    }

                    this.userClanName = user.clanName;
                    this.refreshMessageBoard();
                });
        } else {
            this.userClanName = null;
            this.refreshMessageBoard();
        }

        this.isAdmin = userInfo.isLoggedIn && userInfo.isAdmin;
    }

    private refreshClanData(): Promise<void> {
        this.isClanInformationError = false;
        this.isClanInformationLoading = true;
        this.spinnerService.show("clanInformation");
        return this.clanService.getClan(this.clanName)
            .then(response => {
                if (!response) {
                    return;
                }

                // Set the clan name again since it will be normalized
                this.clanName = response.clanName;

                this.currentRaidLevel = response.currentNewRaidLevel;
                this.currentLegacyRaidLevel = response.currentRaidLevel;
                this.rank = response.rank;
                this.isBlocked = response.isBlocked;
                this.guildMembers = response.guildMembers;
            })
            .catch((err: HttpErrorResponse) => {
                if (!err || err.status !== 404) {
                    this.isClanInformationError = true;
                }
            })
            .finally(() => {
                this.isClanInformationLoading = false;
                this.spinnerService.hide("clanInformation");
            });
    }

    private refreshMessageBoard(): void {
        this.isUserClan = this.clanName === this.userClanName;
        this.messages = null;
        this.getMessages();
    }

    private getMessages(): void {
        if (!this.isUserClan) {
            return;
        }

        this.messagesError = "";
        this.spinnerService.show("clanMessages");
        this.clanService.getMessages()
            .then(messages => {
                this.messages = messages;
            })
            .catch(() => {
                this.messagesError = "There was a problem getting your clan's messages. Please try again.";
            })
            .finally(() => {
                this.spinnerService.hide("clanMessages");
            });
    }
}
