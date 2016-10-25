declare interface IUpload
{
    id: number;

    user: IUser;

    timeSubmitted: string;

    uploadContent: string;

    playStyle: string;

    stats: IMap<number>;
}
