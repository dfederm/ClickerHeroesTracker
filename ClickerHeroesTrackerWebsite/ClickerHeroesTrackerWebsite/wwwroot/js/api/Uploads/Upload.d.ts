declare interface IUpload
{
    id: number;

    user: IUser;

    timeSubmitted: string;

    uploadContent: string;

    stats: IMap<number>;
}
